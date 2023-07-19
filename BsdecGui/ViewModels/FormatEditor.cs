using System;
using System.IO;
using System.Threading;
using BsdecGui.Outsourcing;
using BsdecGui.ViewModels.FilePickers;
using ReactiveUI;

namespace BsdecGui.ViewModels
{
    internal class FormatEditor : ErrorViewModel
    {
        public required SaveFilePicker SaveFilePicker { get; set; }
        private readonly Timer syncTimer;

        public bool TextChangePending = false;
        private string text = string.Empty;
        public string Text
        {
            get => text;
            set
            {
                if (text != value)
                {
                    TextChangePending = true;
                    this.RaiseAndSetIfChanged(ref text, value);
                }
            }
        }

        public FormatEditor()
        {
            syncTimer = new(SyncTimer_Tick, null, 1000, 500);
        }

        public required Action Sync { get; set; }
        public required Action Validate { get; set; }
        public required Action Reimport { get; set; }
        public required Action Import { get; set; }
        public required Action Export { get; set; }
        public required Action ExportAs { get; set; }


        readonly object syncLock = new();
        private void SyncTimer_Tick(object? stateInfo)
        {
            if (!Monitor.TryEnter(syncLock))
                return;
            try
            {
                if (!TextChangePending)
                    return;
                TextChangePending = false;
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
                File.WriteAllText(SaveFilePicker.Path, Text);
            else
            {
                ClearErrors();
                AddError("No savefile specified.");
            }
        }

        public void Save()
        {
            if (!string.IsNullOrEmpty(SaveFilePicker.Path))
                File.WriteAllText(SaveFilePicker.Path, Text);
            else
                SaveAs();
        }

        public void Bsdec_OnProcessCompleted(object? sender, BsdecCompletedEventArgs e)
        {
            // ExitCode -1 means no input
            if (!string.IsNullOrWhiteSpace(e.Stdout) || e.ExitCode == -1)
            {
                Text = e.Stdout;
                TextChangePending = false; // no need to validate the text we just put there...
            }
        }
    }
}
