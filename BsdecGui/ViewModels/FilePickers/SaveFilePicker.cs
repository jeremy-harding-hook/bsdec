//-----------------------------------------------------------------------
//
// Copyright 2023 Jeremy Harding Hook
//
// This file is part of BsdecGui.
//
// BsdecGui is free software: you can redistribute it and/or modify it under
// the terms of the GNU General Public License as published by the Free
// Software Foundation, either version 3 of the License, or (at your option)
// any later version.
//
// BsdecGui is distributed in the hope that it will be useful, but WITHOUT ANY
// WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
// FOR A PARTICULAR PURPOSE. See the GNU General Public License for more
// details.
//
// You should have received a copy of the GNU General Public License along with
// BsdecGui. If not, see <https://www.gnu.org/licenses/>.
//
//-----------------------------------------------------------------------

using Avalonia.Platform.Storage;
using System.Collections.Generic;
using System.Threading.Tasks;
using static BsdecGui.Logging;

namespace BsdecGui.ViewModels.FilePickers
{
    internal class SaveFilePicker : FilePicker
    {
        private readonly string? defaultExtension;
        readonly List<FilePickerFileType> allowedFileTypes = new();

        public SaveFilePicker(IStorageProvider storageProvider, string? defaultExtension, FilePickerFileType? defaultFileType) : base(storageProvider)
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

        public async Task OpenPickerWithFileOpen()
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
