using Avalonia.Platform.Storage;
using System.Collections.Generic;
using System.Threading.Tasks;
using static BsdecGui.Logging;

namespace BsdecGui.ViewModels.FilePickers
{
    internal class SaveFilePicker : FilePicker
    {
        private readonly string defaultExtension;
        readonly List<FilePickerFileType> allowedFileTypes = new();

        public SaveFilePicker(IStorageProvider storageProvider, string defaultExtension, FilePickerFileType? defaultFileType) : base(storageProvider)
        {
            this.defaultExtension = defaultExtension;
            if (defaultFileType != null)
            {
                allowedFileTypes.Add(defaultFileType);
            }
            allowedFileTypes.Add(FilePickerFileTypes.All);
        }

        public override async Task OpenPicker()
        {
            Log.Debug("Browsing for file to save...");
            FilePickerSaveOptions options = new()
            {
                SuggestedFileName = System.IO.Path.GetFileName(Path),
                FileTypeChoices = allowedFileTypes,
                DefaultExtension = defaultExtension
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

        public async void OpenPickerWithFileOpen()
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
