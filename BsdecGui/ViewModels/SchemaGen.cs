using Avalonia.Platform.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BsdecGui.ViewModels
{
    public class SchemaGen : ViewModelBase
    {
        public FilePicker AssemblyFilePicker { get; }
        public FilePicker OutputFilePicker { get; }
        public SchemaGen(IStorageProvider storageProvider) {
            AssemblyFilePicker = new FilePicker(storageProvider, FilePicker.Mode.Open);
            OutputFilePicker = new FilePicker(storageProvider, FilePicker.Mode.Save);
        }
    }
}
