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

using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace BsdecGui.Outsourcing
{
    internal class Bsdec
    {
        private readonly string schemaPath;
        private readonly Formats inputFormat;
        private readonly Formats outputFormat;

        public int ExitCode { get; private set; }
        private readonly string bsdecPath;
#if RELEASELINUX
        private const string ShippedBsdecFilename = "bsdec";
#else
        private const string ShippedBsdecFilename = "Bsdec.exe";
#endif

        public Bsdec(string schemaPath, Formats inputFormat, Formats outputFormat)
        {
            this.schemaPath = schemaPath;
            this.inputFormat = inputFormat;
            this.outputFormat = outputFormat;
            bsdecPath = Path.Combine(AppContext.BaseDirectory, ShippedBsdecFilename);
        }

        private Process? process;
        private Stream? stdout;

        public void Start(Stream? stdin, Stream stdout)
        {
            this.stdout = stdout;
            try
            {
                if (stdin == null || stdin.CanSeek && stdin.Length == 0)
                {
                    OnProcessCompleted?.Invoke(this, new BsdecCompletedEventArgs() { Stderr = "No input.", ExitCode = -1 });
                    return;
                }
#if DEBUG
                Logging.Log.Debug($"In production this would launch {bsdecPath}, however the path is different in debug mode.");
                ProcessStartInfo startInfo = new($@"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}\source\repos\Bsdec\bin\Release\win-x64\publish\{ShippedBsdecFilename}", BuildArgs())
#else
                ProcessStartInfo startInfo = new(bsdecPath, BuildArgs())
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
                process.Exited += Process_Exited;

                Logging.Log.Info($"Command line passed to Bsdec: {process.StartInfo.FileName} {process.StartInfo.Arguments}");

                process.Start();
                process.BeginErrorReadLine();

                // Don't complain when the program closes the pipe
                try
                {
                    stdin.CopyTo(process.StandardInput.BaseStream);
                    process.StandardInput.Close();
                }
                catch { }

                process.StandardOutput.BaseStream.CopyToAsync(stdout);
                Logging.Log.Info("Bsdec started.");
            }
            catch (Exception ex)
            {
                Errors += $"{ex}\n";
                process?.Kill();
                process?.Dispose();
                ExitCode = 3;
                OnProcessCompleted?.Invoke(this, new BsdecCompletedEventArgs() { Stdout = stdout, Stderr = Errors, ExitCode = ExitCode });
                Logging.Log.Error(ex, "Exception while launching Bsdec");
            }
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
            else
            {
                try
                {
                    process?.WaitForExit();
                }
                catch
                {
                    ExitCode = 3;
                }
            }

            // This extra WaitForExit has the effect of waiting for the streams to close.
            process?.Dispose();
            Logging.Log.Debug($"Errors from Bsdec: {Errors}");
            Logging.Log.Info($"Bsdec exited with code {ExitCode}");
            OnProcessCompleted?.Invoke(this, new BsdecCompletedEventArgs { Stdout = stdout, Stderr = Errors, ExitCode = ExitCode });
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
