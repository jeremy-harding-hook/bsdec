using System;
using System.IO;
using BsdecGui.Outsourcing;
using BsdecGui.ViewModels.FilePickers;
using ReactiveUI;

namespace BsdecGui.ViewModels
{
    internal class FormatContextViewModel : ErrorViewModel
    {
        public required SaveFilePicker SaveFilePicker { get; set; }

        private string text = string.Empty;
        public string Text
        {
            get => text;
            set => this.RaiseAndSetIfChanged(ref text, value);
        }

        public required Action Sync { get; set; }
        public required Action Validate { get; set; }
        public required Action Reimport { get; set; }
        public required Action Import { get; set; }
        public required Action Export { get; set; }
        public required Action ExportAs { get; set; }

        public void Open()
        {
            SaveFilePicker.OpenPickerWithFileOpen();
            if (SaveFilePicker.Path != null)
                Text = File.ReadAllText(SaveFilePicker.Path);
        }

        public void SaveAs()
        {
            SaveFilePicker.Browse();
            if(SaveFilePicker.Path != null)
                File.WriteAllText(SaveFilePicker.Path, Text);
        }

        public void Save()
        {
            if (SaveFilePicker.Path != null)
                File.WriteAllText(SaveFilePicker.Path, Text);
        }

        public void Bsdec_OnOutputRecieved(object? sender, StringOutputEventArgs e)
        {
            Text += e.DataOut;
        }
    }
}
