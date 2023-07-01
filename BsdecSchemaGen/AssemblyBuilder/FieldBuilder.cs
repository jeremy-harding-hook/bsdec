using Mono.Cecil;
using System.Linq;

namespace BsdecSchemaGen.AssemblyBuilder
{
    internal static class FieldBuilder
    {
        public static FieldReference AddFieldReference(ModuleDefinition newModule, ModuleDefinition oldModule, FieldReference fieldRef)
        {
            TypeReference fieldType = TypeBuilder.FindOrCreateGenericType(newModule, oldModule, fieldRef.FieldType);
            TypeDefinition declaringType = TypeBuilder.FindOrCreateGenericType(newModule, oldModule, fieldRef.DeclaringType)
                .Resolve();
            FieldReference? fieldReference = declaringType.Fields.FirstOrDefault(x => x.Name == fieldRef.Name);
            if (fieldReference == null)
            {
                FieldDefinition fieldDefinition = new(fieldRef.Name, fieldRef.Resolve().Attributes, fieldType);
                declaringType.Fields.Add(fieldDefinition);
                fieldReference = fieldDefinition;
            }
            return newModule.ImportReference(fieldReference);
        }
    }
}
