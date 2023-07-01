using Mono.Cecil.Cil;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BsdecSchemaGen.AssemblyBuilder
{
    internal static class MethodBuilder
    {
        static readonly List<MethodReference>? handledMethods = new();
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
                ExceptionHandler handler = new ExceptionHandler(item.HandlerType)
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

            return methodCopy;
        }
    }
}
