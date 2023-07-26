//-----------------------------------------------------------------------
//
// Copyright 2023 Jeremy Harding Hook
//
// This file is part of BsdecGui.
//
// BsdecGui is free software: you can redistribute it and/or modify it under
// the terms of the GNU General Public License as published by the Free
// Software Foundation, either version 3 of the License, or (at your option)
// any later version.
//
// BsdecGui is distributed in the hope that it will be useful, but WITHOUT ANY
// WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
// FOR A PARTICULAR PURPOSE. See the GNU General Public License for more
// details.
//
// You should have received a copy of the GNU General Public License along with
// BsdecGui. If not, see <https://www.gnu.org/licenses/>.
//
//-----------------------------------------------------------------------

using Avalonia.Platform.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BsdecGui.ViewModels.FilePickers
{
    internal static class AdditionalFileTypes
    {
        public static readonly FilePickerFileType BsdecFileType = new("Bsdec Schema File")
        {
            Patterns = new[] { "*.dll" },
            MimeTypes = new[] { "application/x-msdownload" }
        };

        public static readonly FilePickerFileType JsonFileType = new("Json File")
        {
            Patterns = new[] { "*.json" },
            MimeTypes = new[] { "application/json" },
            AppleUniformTypeIdentifiers = new[] { "public.json" }
        };

        public static readonly FilePickerFileType XmlFileType = new("Xml File")
        {
            Patterns = new[] { "*.xml" },
            MimeTypes = new[] { "application/xml" },
            AppleUniformTypeIdentifiers = new[] { "public.xml" }
        };
    }
}
