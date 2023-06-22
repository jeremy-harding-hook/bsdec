using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using BsdecGui.ViewModels.FilePickers;
using BsdecGui.Views;
using NLog;
using ReactiveUI;
using System;
using System.Threading;
using static BsdecCore.Logging;

namespace BsdecGui.ViewModels
{
    internal class SchemaGen : ViewModelBase
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

        private string errors = string.Empty;
        public string Errors
        {
            get => errors;
            private set => this.RaiseAndSetIfChanged(ref errors, value);
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

        private readonly Window? mainWindow = null;

        private readonly FilePickerFileType bsdecFileType = new("Bsdec Fileformat Description File")
        {
            Patterns = new[] { "*.bsdec" },
            MimeTypes = new[] { "application/bsdec" }
            // TODO: Figure out how the Apple filetype thing is supposed to be done.
        };

        public SchemaGen(IStorageProvider storageProvider, Window? mainWindow)
        {
            AssemblyFilePicker = new OpenFilePicker(storageProvider);
            OutputFilePicker = new SaveFilePicker(storageProvider, "*.bsdec", bsdecFileType);
            this.mainWindow = mainWindow;
        }

        // TODO: replace with something in the generator class
        bool generationOngoing = false;
        readonly System.Timers.Timer testingTimer = new(10000);
        public void ButtonMashed()
        {
            try
            {
                if (generationOngoing)
                {
                    IfDesiredStopGenerationAsync();
                }
                else
                {
                    generationOngoing = true;
                    testingTimer.Elapsed += OnGenerationStopped;
                    testingTimer.Start();
                    OnGenerationStarted();
                }
                // TODO: implement SchemaGenerator class to handle the i/o related to generation (via seperate app etc) as well as
                // passing the data into the (to be implemented) loader/sessionManager/whatever you want to call it.
                // Use events for OnGenerationStarted/OnGenerationEnded, as well as OnErrorsRecieved and (probably?) OnOutputRecieved
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unhandled exception caught in {0}.{1}", nameof(SchemaGen), nameof(ButtonMashed));
            }
        }

        private void OnGenerationStarted()
        {
            try
            {
                ButtonText = "Kill";
                ButtonBackground = Brushes.Red;
                generationOngoing = true;
                Errors += "Generation is not yet implemented. This is just a simulation.\n";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unhandled exception caught in {0}.{1}", nameof(SchemaGen), nameof(OnGenerationStarted));
            }
        }

        private void OnGenerationStopped(object? sender, EventArgs e)
        {
            try
            {
                killConfermationCancellation.Cancel();
                ButtonText = "Run";
                ButtonBackground = Brushes.Green;
                generationOngoing = false;
                testingTimer.Elapsed -= OnGenerationStopped;
                testingTimer.Stop();
                Output += "Here's some sample output:\n-int someProperty\n-string someOtherProperty\n";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unhandled exception caught in {0}.{1}", nameof(SchemaGen), nameof(OnGenerationStopped));
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
                    // TODO: implement proper killing of the process
                    OnGenerationStopped(this, new EventArgs());
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unhandled exception caught in {0}.{1}", nameof(SchemaGen), nameof(IfDesiredStopGenerationAsync));
            }
        }
    }
}
