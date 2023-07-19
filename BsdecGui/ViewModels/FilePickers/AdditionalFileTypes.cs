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
            // TODO: Figure out how the Apple filetype thing is supposed to be done.
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
