//-----------------------------------------------------------------------
//
// Copyright 2023 Jeremy Harding Hook
//
// This file is part of BsdecSchemaGen.
//
// BsdecSchemaGen is free software: you can redistribute it and/or modify it under
// the terms of the GNU General Public License as published by the Free
// Software Foundation, either version 3 of the License, or (at your option)
// any later version.
//
// BsdecSchemaGen is distributed in the hope that it will be useful, but WITHOUT ANY
// WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
// FOR A PARTICULAR PURPOSE. See the GNU General Public License for more
// details.
//
// You should have received a copy of the GNU General Public License along with
// BsdecSchemaGen. If not, see <https://www.gnu.org/licenses/>.
//
//-----------------------------------------------------------------------

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
