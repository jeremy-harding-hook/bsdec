using Avalonia;
using BsdecGui.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BsdecGui.Outsourcing
{
    internal class SchemaGenerator
    {
        public string AssemblyPath { get; }
        public string OutputPath { get; }
        public string TopLevelClassName { get; }
        public string? ReadMethodName { get; }
        public string? WriteMethodName { get; }
        public int ExitCode { get; private set; }
#if RELEASELINUX
        private const string ShippedSchemaGenFilename = "bsdec-schema-gen";
#else
        private const string ShippedSchemaGenFilename = "BsdecSchemaGen.exe";
#endif

        public SchemaGenerator(string assemblyPath, string outputPath, string topLevelClassName, string? readMethodName, string? writeMethodName)
        {
            AssemblyPath = assemblyPath;
            OutputPath = outputPath;
            TopLevelClassName = topLevelClassName;
            ReadMethodName = readMethodName;
            WriteMethodName = writeMethodName;
        }
        private Process? process;

        public void Start()
        {
            try
            {
#if DEBUG
                ProcessStartInfo startInfo = new($@"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}\source\repos\Bsdec\bin\Release\win-x64\publish\{ShippedSchemaGenFilename}", BuildArgs())
#else
                ProcessStartInfo startInfo = new(ShippedSchemaGenFilename, BuildArgs())
#endif
                {
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                };

                process = new Process
                {
                    StartInfo = startInfo,
                    EnableRaisingEvents = true,
                };

                process.ErrorDataReceived += Process_ErrorDataReceived;
                process.Exited += Process_Exited;

                Logging.Log.Info($"Command line passed to BsdecSchemaGen: {process.StartInfo.FileName} {process.StartInfo.Arguments}");

                process.Start();
                process.BeginErrorReadLine();
                OnGenerationCommenced?.Invoke(this, EventArgs.Empty);
                Logging.Log.Info("BsdecSchemaGen started.");
            }
            catch (Exception ex)
            {
                OnErrorRecieved?.Invoke(this, new StringOutputEventArgs { DataOut = $"{ex}\n" });
                process?.Dispose();
                ExitCode = 3;
                OnGenerationCompleted?.Invoke(this, EventArgs.Empty);
                Logging.Log.Error(ex, "Exception while launching BsdecSchemaGen");
            }
        }

        public void Kill()
        {
            Logging.Log.Info($"Killing BsdecSchemaGen");
            ExitCode = 4;
            process?.Kill();
            process?.Dispose();
        }

        private void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            OnErrorRecieved?.Invoke(this, new StringOutputEventArgs { DataOut = e.Data + '\n' });
            Logging.Log.Info($"Output from BsdecSchemaGen: {e.Data}");
        }

        private void Process_Exited(object? sender, EventArgs e)
        {
            if(ExitCode == 0)
            {
                ExitCode = process?.ExitCode ?? 0;
            }
            process?.Dispose();
            OnGenerationCompleted?.Invoke(this, EventArgs.Empty);
            Logging.Log.Info($"BsdecSchemaGen exited with code {ExitCode}");
        }

        public event EventHandler<StringOutputEventArgs>? OnErrorRecieved;
        public event EventHandler? OnGenerationCommenced;
        public event EventHandler? OnGenerationCompleted;

        private string BuildArgs()
        {
            StringBuilder returnValue = new StringBuilder();
            if (ReadMethodName != null)
                returnValue.Append($"-r {ReadMethodName} ");
            if (WriteMethodName != null)
                returnValue.Append($"-w {WriteMethodName} ");
            returnValue.Append($"{TopLevelClassName} \"{AssemblyPath}\" \"{OutputPath}\"");
            return returnValue.ToString();
        }
    }
}
