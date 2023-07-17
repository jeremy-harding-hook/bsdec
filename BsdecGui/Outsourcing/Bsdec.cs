using System;
using System.Diagnostics;
using System.Text;

namespace BsdecGui.Outsourcing
{
    internal class Bsdec
    {
        private readonly string stdin;
        private readonly Formats inputFormat;
        private readonly Formats outputFormat;

        public int ExitCode { get; private set; }
#if RELEASELINUX
        private const string ShippedBsdecFilename = "bsdec";
#else
        private const string ShippedBsdecFilename = "Bsdec.exe";
#endif

        public Bsdec(string stdin, Formats inputFormat, Formats outputFormat)
        {
            this.stdin = stdin;
            this.inputFormat = inputFormat;
            this.outputFormat = outputFormat;
        }

        private Process? process;

        public void Start()
        {
            try
            {
#if DEBUG
                ProcessStartInfo startInfo = new($@"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}\source\repos\Bsdec\bin\Release\win-x64\publish\{ShippedBsdecFilename}", BuildArgs())
#else
                ProcessStartInfo startInfo = new(ShippedSchemaGenFilename, BuildArgs())
#endif
                {
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                };

                process = new Process
                {
                    StartInfo = startInfo,
                    EnableRaisingEvents = true,
                };

                process.ErrorDataReceived += Process_ErrorDataReceived;
                process.OutputDataReceived += Process_OutputDataReceived;
                process.Exited += Process_Exited;

                Logging.Log.Info($"Command line passed to Bsdec: {process.StartInfo.FileName} {process.StartInfo.Arguments}");

                process.StandardInput.Write(stdin);
                process.Start();
                process.BeginErrorReadLine();
                process.BeginOutputReadLine();
                Logging.Log.Info("Bsdec started.");
            }
            catch (Exception ex)
            {
                OnErrorRecieved?.Invoke(this, new StringOutputEventArgs { DataOut = $"{ex}\n" });
                process?.Dispose();
                ExitCode = 3;
                OnProcessCompleted?.Invoke(this, EventArgs.Empty);
                Logging.Log.Error(ex, "Exception while launching Bsdec");
            }
        }

        public void Kill()
        {
            Logging.Log.Info($"Killing Bsdec");
            ExitCode = 4;
            process?.Kill();
            process?.Dispose();
        }
        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            OnOutputRecieved?.Invoke(this, new StringOutputEventArgs { DataOut = e.Data + '\n' });
            Logging.Log.Info($"Output from Bsdec: {e.Data}");
        }

        private void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            OnErrorRecieved?.Invoke(this, new StringOutputEventArgs { DataOut = e.Data + '\n' });
            Logging.Log.Warn($"Error from Bsdec: {e.Data}");
        }

        private void Process_Exited(object? sender, EventArgs e)
        {
            if (ExitCode == 0)
            {
                ExitCode = process?.ExitCode ?? 0;
            }
            process?.Dispose();
            OnProcessCompleted?.Invoke(this, EventArgs.Empty);
            Logging.Log.Info($"Bsdec exited with code {ExitCode}");
        }

        public event EventHandler<StringOutputEventArgs>? OnOutputRecieved;
        public event EventHandler<StringOutputEventArgs>? OnErrorRecieved;
        public event EventHandler? OnProcessCompleted;

        private string BuildArgs()
        {
            // TODO: add argument for schema
            StringBuilder returnValue = new();

            returnValue.Append($"-i{MapToChar(inputFormat)}{MapToChar(outputFormat)}");
            return returnValue.ToString();
        }

        private static char MapToChar(Formats format)
        {
            return format switch
            {
                Formats.Binary => 'b',
                Formats.Json => 'j',
                Formats.Xml => 'x',
                _ => throw new NotImplementedException($"Format {format} not supported!"),
            };
        }

        public enum Formats
        {
            Binary,
            Json,
            Xml
        }
    }
}
