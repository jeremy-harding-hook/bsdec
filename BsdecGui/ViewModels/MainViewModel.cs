using Avalonia.Platform.Storage;

namespace BsdecGui.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public SchemaGen SchemaGen { get; }

        public MainViewModel(IStorageProvider storageProvider) { 
            SchemaGen = new SchemaGen(storageProvider);
        }
    }
}