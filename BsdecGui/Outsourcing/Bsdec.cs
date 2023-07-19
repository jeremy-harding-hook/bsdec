using System;
using System.Diagnostics;
using System.Text;

namespace BsdecGui.Outsourcing
{
    internal class Bsdec
    {
        private readonly string stdin;
        private readonly string schemaPath;
        private readonly Formats inputFormat;
        private readonly Formats outputFormat;

        public int ExitCode { get; private set; }
#if RELEASELINUX
        private const string ShippedBsdecFilename = "bsdec";
#else
        private const string ShippedBsdecFilename = "Bsdec.exe";
#endif

        public Bsdec(string stdin, string schemaPath, Formats inputFormat, Formats outputFormat)
        {
            this.stdin = stdin;
            this.schemaPath = schemaPath;
            this.inputFormat = inputFormat;
            this.outputFormat = outputFormat;
        }

        private Process? process;

        public void Start()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(stdin))
                {
                    OnProcessCompleted?.Invoke(this, new BsdecCompletedEventArgs() { Stdout = string.Empty, Stderr = "No input.", ExitCode = -1 });
                    return;
                }
#if DEBUG
                ProcessStartInfo startInfo = new($@"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}\source\repos\Bsdec\bin\Release\win-x64\publish\{ShippedBsdecFilename}", BuildArgs())
#else
                ProcessStartInfo startInfo = new(ShippedBsdecFilename, BuildArgs())
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

                process.Start();
                process.BeginErrorReadLine();
                process.BeginOutputReadLine();
                try
                {
                    // Don't complain when the program closes the pipe.
                    process.StandardInput.Write(stdin);
                    process.StandardInput.Close();
                }
                catch { }
                Logging.Log.Info("Bsdec started.");
            }
            catch (Exception ex)
            {
                Errors += $"{ex}\n";
                process?.Dispose();
                ExitCode = 3;
                OnProcessCompleted?.Invoke(this, new BsdecCompletedEventArgs() { Stderr = Errors, ExitCode = ExitCode });
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

        public string Output { get; private set; } = string.Empty;
        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Data))
                return;
            Output += e.Data + '\n';
        }

        public string Errors { get; private set; } = string.Empty;
        private void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Data))
                return;
            Errors += e.Data + '\n';
        }

        private void Process_Exited(object? sender, EventArgs e)
        {
            if (ExitCode == 0)
            {
                try
                {
                    ExitCode = process?.ExitCode ?? 0;
                    process?.WaitForExit();
                }
                catch
                {
                    ExitCode = 0;
                }
            }
            // This extra WaitForExit has the effect of waiting for the streams to close.
            process?.Dispose();
            Logging.Log.Debug($"Output from Bsdec: {Output}");
            Logging.Log.Debug($"Errors from Bsdec: {Errors}");
            Logging.Log.Info($"Bsdec exited with code {ExitCode}");
            OnProcessCompleted?.Invoke(this, new BsdecCompletedEventArgs { Stdout = Output, Stderr = Errors, ExitCode = ExitCode });
        }
        public event EventHandler<BsdecCompletedEventArgs>? OnProcessCompleted;

        private string BuildArgs()
        {
            StringBuilder returnValue = new();

            returnValue.Append($"-i{MapToChar(inputFormat)}{MapToChar(outputFormat)} {schemaPath}");
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
