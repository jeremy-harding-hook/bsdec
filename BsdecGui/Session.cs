using Avalonia.Platform.Storage;
using BsdecGui.Outsourcing;
using BsdecGui.ViewModels;
using BsdecGui.ViewModels.FilePickers;
using System;
using System.Collections.Generic;
using System.IO;
using static BsdecGui.Outsourcing.Bsdec;
using static BsdecGui.Logging;
using BsdecGui.Views;

namespace BsdecGui
{
    internal class Session
    {
        public FormatContextViewModel JsonContext { get; }
        public FormatContextViewModel XmlContext { get; }
        public SchemaGen SchemaGen { get; }

        public Session(SchemaGen schemaGen)
        {
            SchemaGen = schemaGen;
            JsonContext = BuildFormatContextViewModel(Formats.Json, SchemaGen.JsonFilePicker);
            XmlContext = BuildFormatContextViewModel(Formats.Xml, SchemaGen.XmlFilePicker);
        }

        private void SyncEditorPanes(Formats sourceFormat, Formats? currentPane = null, bool refreshSource = false, bool exportBinary = false)
        {
            Formats normalisedCurrentPane = currentPane ?? sourceFormat;
            ErrorViewModel errorViewModel = GetErrorViewModelFromContext(normalisedCurrentPane);
            errorViewModel.ClearErrors();

            string? input = null;
            List<Formats> destinationFormats = new();
            if (refreshSource)
            {
                destinationFormats.Add(sourceFormat);
            }
            if (exportBinary)
            {
                destinationFormats.Add(Formats.Binary);
            }
            switch (sourceFormat)
            {
                case Formats.Json:
                    input = JsonContext.Text;
                    destinationFormats.Add(Formats.Xml);
                    break;
                case Formats.Xml:
                    input = XmlContext.Text;
                    destinationFormats.Add(Formats.Json);
                    break;
                case Formats.Binary:
                    if (string.IsNullOrEmpty(SchemaGen.ImportFilePicker.Path))
                    {
                        errorViewModel.AddError("No schema file is defined. Please set it in the \"Configuration\" tab.");
                        return;
                    }
                    input = File.ReadAllText(SchemaGen.ImportFilePicker.Path);
                    destinationFormats.Add(Formats.Xml);
                    destinationFormats.Add(Formats.Json);
                    break;
            }

            input ??= string.Empty;
            foreach (Formats format in destinationFormats)
            {
                StreamWriter? outputWriter = null;
                try
                {
                    Bsdec bsdec = new(input, sourceFormat, format);
                    bsdec.OnErrorRecieved += errorViewModel.OnErrorRecieved;

                    switch (format)
                    {
                        case Formats.Json:
                            JsonContext.Text = string.Empty;
                            bsdec.OnOutputRecieved += JsonContext.Bsdec_OnOutputRecieved;
                            break;
                        case Formats.Xml:
                            XmlContext.Text = string.Empty;
                            bsdec.OnOutputRecieved += XmlContext.Bsdec_OnOutputRecieved;
                            break;
                        case Formats.Binary:
                            if (string.IsNullOrEmpty(SchemaGen.ExportFilePicker.Path))
                            {
                                errorViewModel.AddError("No schema file is defined. Please set it in the \"Configuration\" tab.");
                                continue;
                            }
                            bsdec.OnProcessCompleted += (sender, e) => { outputWriter?.Dispose(); };
                            outputWriter = new(SchemaGen.ExportFilePicker.Path);
                            bsdec.OnOutputRecieved += (sender, e) => { outputWriter.WriteLine(e.DataOut); };
                            break;
                    }
                    bsdec.Start();
                }
                catch (Exception ex)
                {
                    errorViewModel.AddError(ex.Message);
                    Log.Error(ex);
                }
            }

        }

        private async void ImportBinary(Formats currentPane)
        {
            await SchemaGen.ImportFilePicker.OpenPicker();
            SyncEditorPanes(Formats.Binary, currentPane, false);
        }

        private async void ExportBinary(Formats currentPane)
        {
            await SchemaGen.ExportFilePicker.OpenPicker();
            SyncEditorPanes(currentPane, null, false, true);
        }

        private FormatContextViewModel BuildFormatContextViewModel(Formats format, SaveFilePicker filePicker)
        {
            return new FormatContextViewModel()
            {
                Sync = () => SyncEditorPanes(format),
                SaveFilePicker = filePicker,
                Validate = () => SyncEditorPanes(format, refreshSource: true),
                Import = () => ImportBinary(format),
                Reimport = () => SyncEditorPanes(Formats.Binary, format, false),
                Export = () => SyncEditorPanes(format, null, false, true),
                ExportAs = () => ExportBinary(format)
            };
        }

        private ErrorViewModel GetErrorViewModelFromContext(Formats currentPane)
        {
            return currentPane switch
            {
                Formats.Json => JsonContext,
                Formats.Xml => XmlContext,
                Formats.Binary => SchemaGen,
                _ => throw new NotImplementedException($"Format {currentPane} not supported!"),
            };
        }
    }
}
