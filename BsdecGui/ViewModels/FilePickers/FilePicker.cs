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
using ReactiveUI;
using System;
using System.Threading.Tasks;
using static BsdecGui.Logging;

namespace BsdecGui.ViewModels.FilePickers
{
    internal abstract class FilePicker : ViewModelBase
    {
        private string? path;
        public string? Path
        {
            get => path;
            set => this.RaiseAndSetIfChanged(ref path, value);
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

        public abstract Task OpenPicker();

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
