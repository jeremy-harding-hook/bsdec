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

using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using AvaloniaEdit;
using BsdecGui.Outsourcing;
using BsdecGui.ViewModels.FilePickers;

namespace BsdecGui.ViewModels
{
    internal class FormatEditor : ErrorViewModel
    {
        public required SaveFilePicker SaveFilePicker { get; set; }

        private bool textChangePending = false;
        private bool textChangedBySync = false;

        private string backupText = string.Empty;

        public async Task RefreshText()
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (TextEditor != null)
                    backupText = TextEditor.Text;
            }).GetTask();
        }

        public string Text
        {
            get
            {
                return backupText;
            }
            set
            {
                if (value == backupText)
                    return;
                backupText = value;
                if (TextEditor != null)
                    Dispatcher.UIThread.InvokeAsync(() => TextEditor.Text = value);
            }
        }

        private TextEditor? textEditor;
        private TextEditor? TextEditor
        {
            get => textEditor;
            set
            {
                textEditor = value;
                if (textEditor != null)
                    textEditor.Text = backupText;
            }
        }

        public void TextEditor_TextChanged(object? sender, EventArgs e)
        {
            textChangePending = !textChangedBySync;
        }

        public void TextEditor_DataContextChanged(object? sender, EventArgs e)
        {
            if (TextEditor == null && sender is TextEditor texEd)
                TextEditor = texEd;
        }

        public required Action Sync { get; set; }
        public required Action Validate { get; set; }
        public required Action Reimport { get; set; }
        public required Action Import { get; set; }
        public required Action Export { get; set; }
        public required Action ExportAs { get; set; }


        readonly object syncLock = new();
        public void SyncTimer_Tick()
        {
            if (!Monitor.TryEnter(syncLock))
                return;
            try
            {
                if (!textChangePending)
                    return;

                textChangePending = false;
                Sync();
            }
            finally
            {
                Monitor.Exit(syncLock);
            }
        }

        public async void Open()
        {
            await SaveFilePicker.OpenPickerWithFileOpen();
            if (SaveFilePicker.Path != null)
                Text = File.ReadAllText(SaveFilePicker.Path);
            else
            {
                ClearErrors();
                AddError("No file specified.");
            }
        }

        public async void SaveAs()
        {
            await SaveFilePicker.OpenPicker();
            if (SaveFilePicker.Path != null)
            {
                await RefreshText();
                File.WriteAllText(SaveFilePicker.Path, Text);
            }
            else
            {
                ClearErrors();
                AddError("No savefile specified.");
            }
        }

        public async void Save()
        {
            if (!string.IsNullOrEmpty(SaveFilePicker.Path))
            {
                await RefreshText();
                File.WriteAllText(SaveFilePicker.Path, Text);
            }
            else
                SaveAs();
        }

        public void Bsdec_OnProcessCompleted(object? sender, BsdecCompletedEventArgs e)
        {
            // ExitCode -1 means no input
            if (e.Stdout != null)
            {
                textChangedBySync = true;
                e.Stdout.Flush();
                if (e.Stdout is MemoryStream capturedStdout)
                    Text = Encoding.UTF8.GetString(capturedStdout.GetBuffer()).Trim('\x000');
                e.Stdout.Dispose();
                textChangedBySync = false;
            }
        }
    }
}
