using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BsdecSchemaGen.AssemblyBuilder;
using Mono.Cecil.Rocks;

namespace BsdecSchemaGen.AssemblyBuilder
{
    internal static class TypeBuilder
    {
        static readonly List<TypeReference> handledTypes = new();
        public static TypeReference FindOrCreateGenericType(ModuleDefinition newModule, ModuleDefinition oldModule, TypeReference type)
        {
            if (type == null)
                return null!;

            TypeReference? foundType = handledTypes?.FirstOrDefault(x => x.FullName == type.FullName);
            if (foundType != null)
            {
                return foundType;
            }

            try
            {
                foundType = type.Resolve().Module.Name == oldModule.Name ? FindOrCreateType(newModule, oldModule, type) : newModule.ImportReference(type);
            }
            catch (AssemblyResolutionException ex)
            {
                Console.Error.WriteLine(ex.Message);
                Console.Error.WriteLine("Recovering by just returning object, mileage may vary.");
                return newModule.ImportReference(typeof(object));
            }
            handledTypes.Add(foundType);

            if (foundType is TypeDefinition definition && definition.IsEnum)
            {
                foreach (FieldDefinition field in type.Resolve().Fields)
                {
                    TypeReference fieldType = FindOrCreateGenericType(newModule, oldModule, field.FieldType);

                    FieldDefinition fieldDefinition = new(field.Name, field.Attributes, fieldType);
                    definition.Fields.Add(fieldDefinition);

                    // This must come after the definition is added to the module or it won't work.
                    if (field.Constant != null)
                        fieldDefinition.Constant = field.Constant;
                }
            }


            if (foundType is not GenericInstanceType genericType)
            {
                return foundType;
            }

            for (int i = 0; i < genericType.GenericArguments.Count; i++)
            {
                genericType.GenericArguments[i] = FindOrCreateGenericType(newModule, oldModule, genericType.GenericArguments[i]);
            }

            return genericType;
        }

        private static TypeDefinition FindOrCreateType(ModuleDefinition newModule, ModuleDefinition oldModule, TypeReference type)
        {
            TypeDefinition? definition = newModule.GetAllTypes().FirstOrDefault(x => x.FullName == type.FullName);
            if (definition == null)
            {
                definition = new(type.Namespace, type.Name, type.Resolve().Attributes, FindOrCreateGenericType(newModule, oldModule, type.Resolve().BaseType));
                newModule.Types.Add(definition);
            }

            return definition;
        }
    }
}
