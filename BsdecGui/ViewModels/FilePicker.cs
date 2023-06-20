using Avalonia.Platform.Storage;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static BsdecCore.Logging;

namespace BsdecGui.ViewModels
{
    public class FilePicker : ViewModelBase
    {
        private string? path;
        public string? Path
        {
            get => path;
            private set => this.RaiseAndSetIfChanged(ref path, value);
        }

        private readonly IStorageProvider storageProvider;
        private readonly Mode mode;

        public FilePicker(IStorageProvider storageProvider, Mode mode)
        {
            this.storageProvider = storageProvider;
            this.mode = mode;
        }

        public void Browse()
        {
            switch (mode)
            {
                case Mode.Open:
                    OpenFile();
                    break;
                case Mode.Save: SaveFile(); break;
                default:
                    throw new ArgumentException();
            }
        }

        private async void OpenFile()
        {
            Log.Debug("Browsing for file ot open...");
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

        private async void SaveFile()
        {
            Log.Debug("Browsing for file to save...");
            FilePickerSaveOptions options = new()
            {
                SuggestedFileName = System.IO.Path.GetFileName(Path)
            };
            await SetBasicOptions(options);
            IStorageFile? pick = await storageProvider.SaveFilePickerAsync(options);
            if (pick != null)
            {
                Path = pick.TryGetLocalPath() ?? pick.Path.ToString();
                Log.Debug("File picked: {0}", Path);
            }
            Log.Debug("No file picked.");
        }

        private async Task<PickerOptions> SetBasicOptions(PickerOptions options)
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

        public enum Mode
        {
            Open,
            Save
        }
    }
}
