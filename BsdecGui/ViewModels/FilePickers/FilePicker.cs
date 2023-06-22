using Avalonia.Platform.Storage;
using ReactiveUI;
using System;
using System.Threading.Tasks;
using static BsdecCore.Logging;

namespace BsdecGui.ViewModels.FilePickers
{
    internal abstract class FilePicker : ViewModelBase
    {
        private string? path;
        public string? Path
        {
            get => path;
            protected set => this.RaiseAndSetIfChanged(ref path, value);
        }

        protected readonly IStorageProvider storageProvider;

        public FilePicker(IStorageProvider storageProvider)
        {
            this.storageProvider = storageProvider;
        }

        // Called from view
        public void Browse()
        {
            OpenPicker();
        }

        protected abstract void OpenPicker();

        protected async Task<PickerOptions> SetBasicOptions(PickerOptions options)
        {
            try
            {
                string? folderPath = System.IO.Path.GetDirectoryName(Path);
                options.SuggestedStartLocation = string.IsNullOrEmpty(folderPath)
                    ? await storageProvider.TryGetWellKnownFolderAsync(WellKnownFolder.Documents)
                    : await storageProvider.TryGetFolderFromPathAsync(folderPath);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                options.SuggestedStartLocation = await storageProvider.TryGetWellKnownFolderAsync(WellKnownFolder.Documents);
            }
            return options;
        }
    }
}
