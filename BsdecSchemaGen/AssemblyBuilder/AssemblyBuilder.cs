using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using BsdecSchemaGen.AssemblyBuilder;

namespace BsdecSchemaGen.AssemblyBuilder
{
    internal static class AssemblyBuilder
    {
        public static int BuildAssembly(ModuleDefinition sourceModule, TypeDefinition topleveSavefileClass, MethodDefinition? toplevelReaderMethod, MethodDefinition? toplevelWriterMethod)
        {
            string name = $"{topleveSavefileClass.Name}-savefile-Bsdec-schema";
            AssemblyDefinition schemaAssembly = AssemblyDefinition.CreateAssembly(new AssemblyNameDefinition(name, new Version(1, 0, 0, 0)), $"{name}.dll", ModuleKind.Dll);

            ModuleDefinition schemaModule = schemaAssembly.MainModule;

            MethodReference attributeConstructor = schemaModule.ImportReference(
                typeof(BsdecReadMethodAttribute).GetConstructor(Type.EmptyTypes));
            toplevelReaderMethod?.CustomAttributes.Add(new CustomAttribute(attributeConstructor));

            bool readMethodCreated = false;
            if (toplevelReaderMethod != null)
            {
                try
                {
                    MethodBuilder.FindOrCloneMethod(schemaModule, toplevelReaderMethod.Module, toplevelReaderMethod);
                    readMethodCreated = true;
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex.ToString());
                    string text = toplevelWriterMethod == null ? "Failed to create read method. No schema will be saved." : "Failed to create read method. The resulting schema will only allow writing.";
                    Console.Error.WriteLine(text);
                    // TODO: some attribute magic to achieve the readonly/writeonly stuff
                    // Actually, I may need to delete the assembly entirely as remnants here would probably not assemble
                }
            }

            if (toplevelWriterMethod != null)
            {
                try
                {
                    MethodBuilder.FindOrCloneMethod(schemaModule, toplevelWriterMethod.Module, toplevelWriterMethod);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex.ToString());
                    string text = readMethodCreated ? "Failed to create write method. The resulting schema will only allow reading." : "Failed to create write method. No schema will be saved.";
                    Console.Error.WriteLine(text);
                    // TODO: some attribute magic to achieve the readonly/writeonly stuff
                    // Actually, I may need to delete the assembly entirely as remnants here would probably not assemble
                }
            }

            AssemblyNameReference? sourceModuleReference = schemaModule.AssemblyReferences.FirstOrDefault(x => x.Name == sourceModule.Assembly.Name.Name);
            if (sourceModuleReference != null)
                schemaModule.AssemblyReferences.Remove(sourceModuleReference);

            schemaAssembly.Write($"{name}.dll");

            return 0;
        }

        public static bool IsIndexerPropertyMethod(this MethodReference method)
        {
            TypeReference? declaringType = method.DeclaringType;
            if (declaringType is null) return false;
            PropertyReference? indexerProperty = method.DeclaringType.GetIndexerProperty();
            if (indexerProperty is null) return false;
            return false;
        }

        private static PropertyReference GetIndexerProperty(this TypeReference type)
        {
            Mono.Collections.Generic.Collection<CustomAttribute> defaultPropertyAttribute = type.Resolve().CustomAttributes;
            foreach (CustomAttribute customAttribute in defaultPropertyAttribute)
            {
                Console.WriteLine($"{customAttribute.AttributeType.FullName}");
            }
            return null;
        }
    }
}
