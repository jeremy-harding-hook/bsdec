using Avalonia.Platform.Storage;
using System.Collections.Generic;
using static BsdecCore.Logging;

namespace BsdecGui.ViewModels.FilePickers
{
    internal sealed class OpenFilePicker : FilePicker
    {
        public OpenFilePicker(IStorageProvider storageProvider) : base(storageProvider) { }

        protected override async void OpenPicker()
        {
            Log.Debug("Browsing for file to open...");
            FilePickerOpenOptions options = new()
            {
                AllowMultiple = false
            };
            await SetBasicOptions(options);
            IReadOnlyList<IStorageFile> picks = await storageProvider.OpenFilePickerAsync(options);
            if (picks.Count > 0)
            {
                Path = picks[0].TryGetLocalPath() ?? picks[0]?.Path.ToString();
                Log.Debug("File picked: {0}", Path);
            }
            Log.Debug("No file picked.");
        }
    }
}
