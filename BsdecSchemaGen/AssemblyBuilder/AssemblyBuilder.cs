using Mono.Cecil;
using System;
using System.Linq;
using BsdecCore;
using Mono.Cecil.Rocks;
using Mono.Cecil.Cil;
using System.Collections.Generic;

namespace BsdecSchemaGen.AssemblyBuilder
{
    internal static class AssemblyBuilder
    {
        public static int BuildAssembly(ModuleDefinition sourceModule, string destinationFilepath, TypeDefinition topleveSavefileClass, MethodDefinition? toplevelReaderMethod, MethodDefinition? toplevelWriterMethod)
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
                        ? BuildAssembly(sourceModule, destinationFilepath, topleveSavefileClass, null, toplevelWriterMethod)
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
                        ? BuildAssembly(sourceModule, destinationFilepath, topleveSavefileClass, toplevelReaderMethod, null)
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

            AddConstructorsToTypes(schemaModule);
            PubliciseFields(schemaModule);

            AssemblyNameReference? sourceModuleReference = schemaModule.AssemblyReferences.FirstOrDefault(x => x.Name == sourceModule.Assembly.Name.Name);
            if (sourceModuleReference != null)
                schemaModule.AssemblyReferences.Remove(sourceModuleReference);

            AssemblyNameReference? bsdecSchemaGenModuleReference = schemaModule.AssemblyReferences.FirstOrDefault(x => x.Name == System.Reflection.Assembly.GetExecutingAssembly().GetName().Name);
            if (bsdecSchemaGenModuleReference != null)
                schemaModule.AssemblyReferences.Remove(bsdecSchemaGenModuleReference);

            try
            {
                schemaAssembly.Write(destinationFilepath);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.ToString());
                Console.Error.WriteLine("Failed to create schema, possibly due to a bug. Please create a PR or at least alert the author.");
                return 1;
            }

            return 0;
        }

        /// <summary>
        /// Adds parameterless constructors to anything that doesn't already have one.
        /// </summary>
        /// <param name="module">The newly created module.</param>
        static void AddConstructorsToTypes(ModuleDefinition module)
        {
            foreach (TypeDefinition type in module.GetTypes())
            {
                if (type.IsAbstract || type.IsInterface || type.IsEnum || type.IsValueType || type.FullName == "<Module>")
                {
                    continue;
                }
                IEnumerable<MethodDefinition> parameterlessConstructors = type.GetConstructors().Where(x => !x.HasParameters && x.IsPublic);
                if (!parameterlessConstructors.Any())
                {
                    // Add new empty constructor, we don't need to copy the old ones.
                    MethodAttributes methodAttributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName;
                    MethodDefinition method = new(".ctor", methodAttributes, module.ImportReference(typeof(void)));
                    method.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
                    MethodReference objectConstructor = module.ImportReference(module.ImportReference(typeof(object)).Resolve().GetConstructors().Single(x => x.IsPublic && !x.HasParameters && !x.IsStatic));
                    method.Body.Instructions.Add(Instruction.Create(OpCodes.Call, objectConstructor));
                    method.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
                    type.Methods.Add(method);
                }
            }
        }

        /// <summary>
        /// Makes private fields public so that serialisers will see them.
        /// </summary>
        /// <param name="module">The newly created module.</param>
        static void PubliciseFields(ModuleDefinition module)
        {
            foreach (TypeDefinition type in module.GetTypes())
            {
                foreach (FieldDefinition field in type.Fields)
                {
                    if (field.IsPrivate && !field.CustomAttributes.Any(x => x.AttributeType.FullName == "System.Runtime.CompilerServices.CompilerGeneratedAttribute"))
                    {
                        field.IsPublic = true;
                    }
                }
            }
        }
    }
}
