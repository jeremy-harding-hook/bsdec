using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace BsdecGui.ViewModels
{
    internal class MainViewModel : ViewModelBase
    {
        public SchemaGen SchemaGen { get; }

        public MainViewModel(IStorageProvider storageProvider, Window? mainWindow = null) { 
            SchemaGen = new SchemaGen(storageProvider, mainWindow);
        }
    }
}