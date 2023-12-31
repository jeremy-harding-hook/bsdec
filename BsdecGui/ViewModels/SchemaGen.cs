﻿//-----------------------------------------------------------------------
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

using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using BsdecGui.Outsourcing;
using BsdecGui.ViewModels.FilePickers;
using BsdecGui.Views;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using static BsdecGui.Logging;

namespace BsdecGui.ViewModels
{
    internal class SchemaGen : ErrorViewModel
    {
        #region view-bound properties
        private string? topLevelClassName;
        public string? TopLevelClassName
        {
            get => topLevelClassName;
            set => this.RaiseAndSetIfChanged(ref topLevelClassName, value);
        }

        private string? writeMethodName;
        public string? WriteMethodName
        {
            get => writeMethodName;
            set => this.RaiseAndSetIfChanged(ref writeMethodName, value);
        }

        private string? readMethodName;
        public string? ReadMethodName
        {
            get => readMethodName;
            set => this.RaiseAndSetIfChanged(ref readMethodName, value);
        }

        private bool loadSchema = true;
        public bool LoadSchema
        {
            get => loadSchema;
            set => this.RaiseAndSetIfChanged(ref loadSchema, value);
        }

        private string? buttonText = "Run";
        public string? ButtonText
        {
            get => buttonText;
            private set => this.RaiseAndSetIfChanged(ref buttonText, value);
        }

        private string output = string.Empty;
        public string Output
        {
            get => output;
            private set => this.RaiseAndSetIfChanged(ref output, value);
        }

        private IImmutableSolidColorBrush buttonBackground = Brushes.Green;
        public IImmutableSolidColorBrush ButtonBackground
        {
            get => buttonBackground;
            set => this.RaiseAndSetIfChanged(ref buttonBackground, value);
        }
        #endregion

        public OpenFilePicker AssemblyFilePicker { get; }
        public SaveFilePicker OutputFilePicker { get; }

        public OpenFilePicker SchemaFilePicker { get; }
        public OpenFilePicker ImportFilePicker { get; }
        public SaveFilePicker ExportFilePicker { get; }
        public SaveFilePicker JsonFilePicker { get; }
        public SaveFilePicker XmlFilePicker { get; }

        private readonly Window? mainWindow = null;

        public SchemaGen(IStorageProvider storageProvider, Window? mainWindow)
        {
            AssemblyFilePicker = new OpenFilePicker(storageProvider);
            OutputFilePicker = new SaveFilePicker(storageProvider, "*.dll", AdditionalFileTypes.BsdecFileType);

            SchemaFilePicker = new OpenFilePicker(storageProvider, new List<FilePickerFileType>() { AdditionalFileTypes.BsdecFileType });

            ImportFilePicker = new OpenFilePicker(storageProvider);
            ExportFilePicker = new SaveFilePicker(storageProvider, "*.*", null);
            JsonFilePicker = new SaveFilePicker(storageProvider, "*.json", AdditionalFileTypes.JsonFileType);
            XmlFilePicker = new SaveFilePicker(storageProvider, "*.xml", AdditionalFileTypes.XmlFileType);

            this.mainWindow = mainWindow;
        }

        SchemaGenerator? generator = null;
        bool GenerationOngoing => generator != null;

        public void ButtonMashed()
        {
            try
            {
                if (GenerationOngoing)
                {
                    IfDesiredStopGenerationAsync();
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(ReadMethodName))
                        ReadMethodName = null;
                    if (string.IsNullOrWhiteSpace(WriteMethodName))
                        WriteMethodName = null;

                    if (string.IsNullOrEmpty(AssemblyFilePicker.Path) ||
                        string.IsNullOrWhiteSpace(TopLevelClassName) ||
                        ReadMethodName == null && WriteMethodName == null)
                    {
                        string message =
                            "The following fields are required:\n" +
                            "\tTop-level class name\n" +
                            "\tAt least one of Reader or Writer method names (preferably both)\n" +
                            "\tProgram file path";
                        MessageBox.Show(mainWindow, message, "Empty fields", MessageBox.MessageBoxButtons.Ok, null, true);
                        return;
                    }
                    string outputPath = string.IsNullOrWhiteSpace(OutputFilePicker.Path) ? Path.GetTempFileName() : OutputFilePicker.Path;
                    generator = new(AssemblyFilePicker.Path, outputPath, TopLevelClassName, ReadMethodName, WriteMethodName);

                    generator.OnGenerationCommenced += Generator_OnGenerationCommenced;
                    generator.OnGenerationCompleted += Generator_OnGenerationCompleted;
                    generator.OnErrorRecieved += OnErrorRecieved;

                    Errors = string.Empty;

                    generator.Start();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unhandled exception caught in {0}.{1}", nameof(SchemaGen), nameof(ButtonMashed));
            }
        }

        private void Generator_OnGenerationCommenced(object? sender, EventArgs e)
        {
            try
            {
                ButtonText = "Kill";
                ButtonBackground = Brushes.Red;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unhandled exception caught in {0}.{1}", nameof(SchemaGen), nameof(Generator_OnGenerationCommenced));
            }
        }

        private void Generator_OnGenerationCompleted(object? sender, EventArgs e)
        {
            if (generator!.ExitCode == 0)
            {
                if (LoadSchema)
                    SchemaFilePicker.Path = generator!.OutputPath;
                else if (!string.IsNullOrWhiteSpace(OutputFilePicker.Path))
                    Errors += $"The output file can be found in {Path.GetFullPath(generator!.OutputPath)}\n";
            }
            if (generator!.ExitCode == 4)
                Errors += "Process killed.\n";

            try
            {
                generator = null;
                killConfermationCancellation.Cancel();
                ButtonText = "Run";
                ButtonBackground = Brushes.Green;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unhandled exception caught in {0}.{1}", nameof(SchemaGen), nameof(Generator_OnGenerationCompleted));
            }
        }

        CancellationTokenSource killConfermationCancellation = new();
        private async void IfDesiredStopGenerationAsync()
        {
            try
            {
                killConfermationCancellation = new CancellationTokenSource();
                string message = "Schema generation is currently running, and stopping it will lose any progress already made.\nAre you sure you want to kill it?";
                MessageBox.MessageBoxResult result = await MessageBox.Show(mainWindow, message, "Stop generation?", MessageBox.MessageBoxButtons.DieCancel, killConfermationCancellation);
                if (result == MessageBox.MessageBoxResult.Kill)
                {
                    generator?.Kill();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unhandled exception caught in {0}.{1}", nameof(SchemaGen), nameof(IfDesiredStopGenerationAsync));
            }
        }
    }
}
