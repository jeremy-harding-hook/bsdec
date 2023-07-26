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

using BsdecGui.Outsourcing;
using BsdecGui.ViewModels;
using BsdecGui.ViewModels.FilePickers;
using System;
using System.Collections.Generic;
using System.IO;
using static BsdecGui.Outsourcing.Bsdec;
using static BsdecGui.Logging;
using System.Threading;
using System.Text;

namespace BsdecGui
{
    internal class Session
    {
        public FormatEditor JsonContext { get; }
        public FormatEditor XmlContext { get; }
        public SchemaGen SchemaGen { get; }

        /// <summary>
        /// Timer for automatic syncing of the panes.
        /// </summary>
        /// <remarks>
        /// In theory this should be disposed, however it runs for the full lifetime of the program
        /// so there's not really a good way to do that.
        /// </remarks>
        // Unread just because there's no need to read it, however we can't remove it since it triggers our syncs.
#pragma warning disable IDE0052 // Remove unread private members
        private readonly Timer syncTimer;
#pragma warning restore IDE0052 // Remove unread private members

        public Session(SchemaGen schemaGen)
        {
            SchemaGen = schemaGen;
            JsonContext = BuildFormatContextViewModel(Formats.Json, SchemaGen.JsonFilePicker);
            XmlContext = BuildFormatContextViewModel(Formats.Xml, SchemaGen.XmlFilePicker);
            syncTimer = new(x => { JsonContext.SyncTimer_Tick(); XmlContext.SyncTimer_Tick(); }, null, 1000, 500);
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

                Func<Stream>? getInput = null;
                List<Formats> destinationFormats = new();
                Queue<Stream> inputs = new();
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
                        await JsonContext.RefreshText();
                        getInput = () => new MemoryStream(Encoding.UTF8.GetBytes(JsonContext.Text));
                        destinationFormats.Add(Formats.Xml);
                        break;
                    case Formats.Xml:
                        await XmlContext.RefreshText();
                        getInput = () => new MemoryStream(Encoding.UTF8.GetBytes(XmlContext.Text));
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
                        getInput = () => File.OpenRead(SchemaGen.ImportFilePicker.Path);
                        destinationFormats.Add(Formats.Xml);
                        destinationFormats.Add(Formats.Json);
                        break;
                    default: throw new NotImplementedException();
                }

                foreach (Formats format in destinationFormats)
                {
                    inputs.Enqueue(getInput());
                }

                foreach (Formats format in destinationFormats)
                {
                    try
                    {
                        Stream input = inputs.Dequeue();
                        Stream? output = null;
                        Bsdec bsdec = new(schemaPath, sourceFormat, format);

                        switch (format)
                        {
                            case Formats.Json:
                                output = new MemoryStream();
                                bsdec.OnProcessCompleted += JsonContext.Bsdec_OnProcessCompleted;
                                break;
                            case Formats.Xml:
                                output = new MemoryStream();
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
                                output = new FileStream(SchemaGen.ExportFilePicker.Path, FileMode.Create);
                                bsdec.OnProcessCompleted += (sender, e) =>
                                {
                                    output?.Flush();
                                    output?.Dispose();
                                };
                                break;
                            default: throw new NotImplementedException();
                        }
                        bsdec.OnProcessCompleted += (sender, e) =>
                        {
                            if (!string.IsNullOrWhiteSpace(e.Stderr))
                            {
                                errorViewModel.AddError(e.Stderr);
                            }
                            // Note: This disposal must happen after the memoryStreams for the editor views have been read
                            input?.Flush();
                            input?.Dispose();
                        };
                        bsdec.Start(input, output);
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
