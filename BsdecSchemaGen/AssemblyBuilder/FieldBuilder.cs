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

                CustomAttribute? compilerGeneraterAttribute = fieldRef.Resolve().CustomAttributes.FirstOrDefault(x => x.AttributeType.FullName == "System.Runtime.CompilerServices.CompilerGeneratedAttribute");
                if (compilerGeneraterAttribute != null)
                {
                    MethodReference attributeConstructor = newModule.ImportReference(compilerGeneraterAttribute.Constructor);
                    fieldDefinition.CustomAttributes.Add(new CustomAttribute(attributeConstructor));
                }

                fieldReference = fieldDefinition;
            }
            return newModule.ImportReference(fieldReference);
        }
    }
}
