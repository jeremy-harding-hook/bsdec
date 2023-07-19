using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace BsdecGui.ViewModels
{
    internal class MainViewModel : ViewModelBase
    {
        private readonly Session session;
        public SchemaGen SchemaGen { get; }
        public FormatEditor JsonEditor { get; }
        public FormatEditor XmlEditor { get; }

        public MainViewModel(IStorageProvider storageProvider, Window? mainWindow = null) { 
            SchemaGen = new SchemaGen(storageProvider, mainWindow);
            session = new Session(SchemaGen);
            JsonEditor = session.JsonContext;
            XmlEditor = session.XmlContext;
        }
    }
}