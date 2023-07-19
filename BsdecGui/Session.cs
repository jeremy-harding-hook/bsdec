using BsdecGui.Outsourcing;
using BsdecGui.ViewModels;
using BsdecGui.ViewModels.FilePickers;
using System;
using System.Collections.Generic;
using System.IO;
using static BsdecGui.Outsourcing.Bsdec;
using static BsdecGui.Logging;

namespace BsdecGui
{
    internal class Session
    {
        public FormatEditor JsonContext { get; }
        public FormatEditor XmlContext { get; }
        public SchemaGen SchemaGen { get; }

        public Session(SchemaGen schemaGen)
        {
            SchemaGen = schemaGen;
            JsonContext = BuildFormatContextViewModel(Formats.Json, SchemaGen.JsonFilePicker);
            XmlContext = BuildFormatContextViewModel(Formats.Xml, SchemaGen.XmlFilePicker);
        }

        private async void SyncEditorPanes(Formats sourceFormat, Formats? currentPane = null, bool refreshSource = false, bool exportBinary = false)
        {
            Formats normalisedCurrentPane = currentPane ?? sourceFormat;
            ErrorViewModel errorViewModel = GetErrorViewModelFromContext(normalisedCurrentPane);
            errorViewModel.ClearErrors();
            try
            {
                if (string.IsNullOrEmpty(SchemaGen.SchemaFilePicker.Path))
                {
                    errorViewModel.AddError("No schema file is defined. Please set it in the \"Configuration\" tab.");
                    return;
                }
                string schemaPath = SchemaGen.SchemaFilePicker.Path;

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
                            await SchemaGen.ImportFilePicker.OpenPicker();
                            if (string.IsNullOrEmpty(SchemaGen.ImportFilePicker.Path))
                            {
                                errorViewModel.AddError("No import source is defined. Please set it in the \"Configuration\" tab.");
                                return;
                            }
                        }
                        input = File.ReadAllText(SchemaGen.ImportFilePicker.Path);
                        destinationFormats.Add(Formats.Xml);
                        destinationFormats.Add(Formats.Json);
                        break;
                }

                input ??= string.Empty;
                List<Bsdec> bsdecInstances = new();

                foreach (Formats format in destinationFormats)
                {
                    StreamWriter? outputWriter = null;
                    try
                    {
                        Bsdec bsdec = new(input, schemaPath, sourceFormat, format);

                        switch (format)
                        {
                            case Formats.Json:
                                bsdec.OnProcessCompleted += JsonContext.Bsdec_OnProcessCompleted;
                                break;
                            case Formats.Xml:
                                bsdec.OnProcessCompleted += XmlContext.Bsdec_OnProcessCompleted;
                                break;
                            case Formats.Binary:
                                if (string.IsNullOrEmpty(SchemaGen.ExportFilePicker.Path))
                                {
                                    await SchemaGen.ExportFilePicker.OpenPicker();
                                    if (string.IsNullOrEmpty(SchemaGen.ExportFilePicker.Path))
                                    {
                                        errorViewModel.AddError("No export file is defined. Please set it in the \"Configuration\" tab.");
                                        continue;
                                    }
                                }
                                bsdec.OnProcessCompleted += (sender, e) =>
                                {
                                    using (outputWriter = new(SchemaGen.ExportFilePicker.Path))
                                    {
                                        outputWriter.WriteLineAsync(e.Stdout);
                                    }
                                };
                                break;
                        }
                        bsdec.OnProcessCompleted += (sender, e) =>
                        {
                            if (!string.IsNullOrWhiteSpace(e.Stderr))
                            {
                                errorViewModel.AddError(e.Stderr);
                            }
                            bsdecInstances.Remove((Bsdec)sender!);
                        };
                        bsdec.Start();
                        bsdecInstances.Add(bsdec);
                    }
                    catch (Exception ex)
                    {
                        errorViewModel.AddError(ex.Message);
                        Log.Error(ex);
                    }
                }
            }
            catch (Exception ex)
            {
                errorViewModel.AddError(ex.Message);
                Log.Error(ex);
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

        private FormatEditor BuildFormatContextViewModel(Formats format, SaveFilePicker filePicker)
        {
            return new FormatEditor()
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
