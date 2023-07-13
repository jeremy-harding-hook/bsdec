using Mono.Cecil.Cil;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil.Rocks;

namespace BsdecSchemaGen.AssemblyBuilder
{
    internal static class MethodBuilder
    {
        static readonly List<MethodReference> handledMethods = new();
        public static MethodReference FindOrCloneMethod(ModuleDefinition newModule, ModuleDefinition oldModule, MethodReference methodReference)
        {
            MethodReference? foundMethod = handledMethods?.FirstOrDefault(x => x.FullName == methodReference.FullName);
            if (foundMethod != null)
            {
                return foundMethod;
            }

            MethodDefinition methodDefinition;
            try
            {
                methodDefinition = methodReference.Resolve();
            }
            catch (AssemblyResolutionException ex)
            {
                if (methodReference.FullName.Contains("Log") &&
                    methodReference.Parameters.Count == 1 &&
                    methodReference.ReturnType.FullName == "System.Void")
                {
                    Console.Error.WriteLine(ex.Message);
                    Console.Error.WriteLine("Looks like it might just be a logging method, we'll try to work with that.");
                    TypeDefinition consoleRef = newModule.ImportReference(typeof(Console)).Resolve();
                    MethodReference logRef = newModule.ImportReference(consoleRef.Methods.First(x => x.FullName == "System.Void System.Console::WriteLine(System.Object)"));
                    return logRef;
                }
                throw;
            }

            foundMethod = methodDefinition.Module == oldModule
                ? CloneMethod(newModule, oldModule, methodDefinition)
                : newModule.ImportReference(methodDefinition);

            if (methodReference is GenericInstanceMethod oldGenericMethod)
            {
                GenericInstanceMethod newGenericMethod = new(foundMethod);

                foreach (TypeReference argument in oldGenericMethod.GenericArguments)
                {
                    newGenericMethod.GenericArguments.Add(TypeBuilder.FindOrCreateGenericType(newModule, oldModule, argument));
                }

                foundMethod = newGenericMethod;
            }

            if (foundMethod is not MethodSpecification)
            {
                foundMethod.DeclaringType = TypeBuilder.FindOrCreateGenericType(newModule, oldModule, methodReference.DeclaringType);
            }

            foreach (TypeDefinition derivedType in GetAllDerivedTypesInModule(oldModule, methodDefinition.DeclaringType))
            {
                MethodDefinition? overridingMethod = derivedType.GetMethods().FirstOrDefault(x => x.GetBaseMethod() == methodDefinition);
                if (overridingMethod != null)
                    FindOrCloneMethod(newModule, oldModule, overridingMethod);
            }

            return foundMethod;
        }

        private static MethodDefinition CloneMethod(ModuleDefinition newModule, ModuleDefinition oldModule, MethodDefinition method)
        {
            TypeDefinition newDefiningType = TypeBuilder.FindOrCreateGenericType(newModule, oldModule, method.DeclaringType).Resolve();

            MethodDefinition methodCopy = new(method.Name, method.Attributes, TypeBuilder.FindOrCreateGenericType(newModule, oldModule, method.ReturnType));

            handledMethods.Add(methodCopy);

            foreach (ParameterDefinition? parameter in method.Parameters)
            {
                ParameterDefinition newParam = new(parameter.Name, parameter.Attributes, TypeBuilder.FindOrCreateGenericType(newModule, oldModule, parameter.ParameterType));
                if (parameter.HasDefault)
                {
                    newParam.Constant = parameter.Constant;
                }
                methodCopy.Parameters.Add(newParam);
            }

            if (method.Body.InitLocals)
            {
                methodCopy.Body.InitLocals = true;
                foreach (VariableDefinition? item in method.Body.Variables)
                {
                    methodCopy.Body.Variables.Add(new VariableDefinition(TypeBuilder.FindOrCreateGenericType(newModule, oldModule, item.VariableType)));
                }
            }

            ILProcessor newProcessor = methodCopy.Body.GetILProcessor();
            newDefiningType.Methods.Add(methodCopy);

            foreach (Instruction? il in method.Body.Instructions)
            {
                if (il.Operand is MethodReference methodRef)
                {
                    il.Operand = FindOrCloneMethod(newModule, oldModule, methodRef);
                }
                else if (il.Operand is FieldReference fieldRef)
                {
                    il.Operand = FieldBuilder.AddFieldReference(newModule, oldModule, fieldRef);
                }
                else if (il.Operand is TypeReference typeReference)
                {
                    il.Operand = TypeBuilder.FindOrCreateGenericType(newModule, oldModule, typeReference);
                }

                newProcessor.Append(il);
            }

            foreach (ExceptionHandler item in method.Body.ExceptionHandlers)
            {
                ExceptionHandler handler = new(item.HandlerType)
                {
                    FilterStart = item.FilterStart,
                    HandlerStart = item.HandlerStart,
                    HandlerEnd = item.HandlerEnd,
                    TryStart = item.TryStart,
                    TryEnd = item.TryEnd,
                    CatchType = TypeBuilder.FindOrCreateGenericType(newModule, oldModule, item.CatchType)
                };
                methodCopy.Body.ExceptionHandlers.Add(handler);
            }

            if (method.IsGetter || method.IsSetter)
            {
                PropertyDefinition? property = method.DeclaringType.Properties.FirstOrDefault(x => x.SetMethod == method || x.GetMethod == method);
                if (property != null)
                {
                    PropertyDefinition? propertyCopy = methodCopy.DeclaringType.Properties.FirstOrDefault(x => x.Name == property.Name);
                    if (propertyCopy == null)
                    {
                        TypeReference propertyType = property.SetMethod == method ? methodCopy.Parameters.First().ParameterType : methodCopy.ReturnType;
                        propertyCopy = new(property.Name, property.Attributes, propertyType);
                        methodCopy.DeclaringType.Properties.Add(propertyCopy);
                    }
                    if (property.SetMethod == method)
                    {
                        propertyCopy.SetMethod = methodCopy;
                    }
                    else
                    {
                        propertyCopy.GetMethod = methodCopy;
                    }
                }
            }

            CustomAttribute? compilerGeneraterAttribute = method.CustomAttributes.FirstOrDefault(x => x.AttributeType.FullName == "System.Runtime.CompilerServices.CompilerGeneratedAttribute");
            if (compilerGeneraterAttribute != null)
            {
                MethodReference attributeConstructor = newModule.ImportReference(compilerGeneraterAttribute.Constructor);
                methodCopy.CustomAttributes.Add(new CustomAttribute(attributeConstructor));
            }

            return methodCopy;
        }

        /// <summary>
        /// Gets the derived types in the current module recursively, but excluding types that inherit indirectly through an
        /// out-of-module third party. I.e. if A : B : C and C is the value of <paramref name="typeDefinition"/>, A will be
        /// in the return set if and only if B is defined in the current module. Due to this, it is also true that if A is
        /// returned then B will also be returned.
        /// </summary>
        /// <param name="module">The module to search in.</param>
        /// <param name="typeDefinition">The type to search for. This does not have to be defined in the module passed as
        /// the first parameter, but any out-of-module third parties will not be considered in the inheritence tree.</param>
        /// <returns>A list of the definitions of found types.</returns>
        private static List<TypeDefinition> GetAllDerivedTypesInModule(ModuleDefinition module, TypeDefinition typeDefinition)
        {
            List<TypeDefinition> derivedTypes = module.GetAllTypes().Where(x => x.BaseType != null && x.BaseType.FullName == typeDefinition.FullName).ToList();
            for (int i = 0; i < derivedTypes.Count; i++)
            {
                derivedTypes.AddRange(GetAllDerivedTypesInModule(module, derivedTypes[i]));
            }
            return derivedTypes;
        }
    }
}
