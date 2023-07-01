using Mono.Cecil;
using System;
using System.Linq;
using BsdecCore;

namespace BsdecSchemaGen.AssemblyBuilder
{
    internal static class AssemblyBuilder
    {
        public static int BuildAssembly(ModuleDefinition sourceModule, TypeDefinition topleveSavefileClass, MethodDefinition? toplevelReaderMethod, MethodDefinition? toplevelWriterMethod)
        {
            string name = $"{topleveSavefileClass.Name}-savefile-Bsdec-schema";
            AssemblyDefinition schemaAssembly = AssemblyDefinition.CreateAssembly(new AssemblyNameDefinition(name, new Version(1, 0, 0, 0)), $"{name}.dll", ModuleKind.Dll);

            ModuleDefinition schemaModule = schemaAssembly.MainModule;

            TypeDefinition? newTopLevelSaveClass = null;
            MethodReference? attributeConstructor = null;

            if (toplevelReaderMethod != null)
            {
                try
                {
                    MethodDefinition newTopLevelReader = MethodBuilder.FindOrCloneMethod(schemaModule, toplevelReaderMethod.Module, toplevelReaderMethod).Resolve();

                    attributeConstructor = schemaModule.ImportReference(
                        typeof(BsdecReadMethodAttribute).GetConstructor(Type.EmptyTypes));
                    newTopLevelReader.CustomAttributes.Add(new CustomAttribute(attributeConstructor));

                    newTopLevelSaveClass = newTopLevelReader.DeclaringType;
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex.ToString());
                    string text = toplevelWriterMethod == null ? "Failed to create read method. No schema will be saved." : "Failed to create read method. The resulting schema will only allow writing.";
                    Console.Error.WriteLine(text);
                    return toplevelWriterMethod != null
                        ? BuildAssembly(sourceModule, topleveSavefileClass, null, toplevelWriterMethod)
                        : 1;
                }
            }

            if (toplevelWriterMethod != null)
            {
                try
                {
                    MethodDefinition newTopLevelWriter = MethodBuilder.FindOrCloneMethod(schemaModule, toplevelWriterMethod.Module, toplevelWriterMethod).Resolve();

                    attributeConstructor = schemaModule.ImportReference(
                        typeof(BsdecWriteMethodAttribute).GetConstructor(Type.EmptyTypes));
                    newTopLevelWriter.CustomAttributes.Add(new CustomAttribute(attributeConstructor));

                    newTopLevelSaveClass = newTopLevelWriter.DeclaringType;
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex.ToString());
                    string text = toplevelReaderMethod == null ? "Failed to create write method. The resulting schema will only allow reading." : "Failed to create write method. No schema will be saved.";
                    Console.Error.WriteLine(text);
                    return toplevelReaderMethod != null
                        ? BuildAssembly(sourceModule, topleveSavefileClass, toplevelReaderMethod, null)
                        : 1;
                }
            }

            if (newTopLevelSaveClass == null)
            {
                Console.Error.WriteLine("Failed to create schema, undoubtedly due to a bug. Please create a PR or at least alert the author.");
                return 1;
            }

            attributeConstructor = schemaModule.ImportReference(
                typeof(BsdecToplevelSavefileClassAttribute).GetConstructor(Type.EmptyTypes));
            newTopLevelSaveClass.CustomAttributes.Add(new CustomAttribute(attributeConstructor));

            AssemblyNameReference? sourceModuleReference = schemaModule.AssemblyReferences.FirstOrDefault(x => x.Name == sourceModule.Assembly.Name.Name);
            if (sourceModuleReference != null)
                schemaModule.AssemblyReferences.Remove(sourceModuleReference);

            AssemblyNameReference? bsdecSchemaGenModuleReference = schemaModule.AssemblyReferences.FirstOrDefault(x => x.Name == System.Reflection.Assembly.GetExecutingAssembly().GetName().Name);
            if (bsdecSchemaGenModuleReference != null)
                schemaModule.AssemblyReferences.Remove(bsdecSchemaGenModuleReference);

            try
            {
                schemaAssembly.Write($"{name}.dll");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.ToString());
                Console.Error.WriteLine("Failed to create schema, possibly due to a bug. Please create a PR or at least alert the author.");
                return 1;
            }

            return 0;
        }
    }
}
