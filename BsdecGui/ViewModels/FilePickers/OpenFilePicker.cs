using Avalonia.Platform.Storage;
using System.Collections.Generic;
using System.Threading.Tasks;
using static BsdecGui.Logging;

namespace BsdecGui.ViewModels.FilePickers
{
    internal sealed class OpenFilePicker : FilePicker
    {
        readonly List<FilePickerFileType>? allowedFileTypes = null;

        public OpenFilePicker(IStorageProvider storageProvider) : base(storageProvider) { }

        public OpenFilePicker(IStorageProvider storageProvider, List<FilePickerFileType> allowedFileTypes) : base(storageProvider)
        {
            if(allowedFileTypes != null)
            {
                this.allowedFileTypes = allowedFileTypes;
                this.allowedFileTypes.Add(FilePickerFileTypes.All);
            }
        }

        public override async Task OpenPicker()
        {
            Log.Debug("Browsing for file to open...");
            FilePickerOpenOptions options = new()
            {
                AllowMultiple = false,
                FileTypeFilter = allowedFileTypes
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
