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

using System;
using System.Reflection;
using Avalonia;
using Avalonia.ReactiveUI;
using static BsdecGui.Logging;

[assembly: AssemblyVersion("1.0.0.0")]

namespace BsdecGui.Desktop
{
    class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static void Main(string[] args)
        {
            int exitCode = 0;
            try
            {
                Log.Debug("Starting GUI...");
                Log.Debug($"Args: {string.Join(", ", args)}");
                BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
                Log.Debug("GUI closed.");
            }
            catch (Exception ex)
            {
                exitCode = 1;
                Log.Fatal(ex, "There was a fatal exception in the program!");
            }
            finally
            {
                FlushLogs();
                Environment.Exit(exitCode);
            }
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace()
                .UseReactiveUI();
    }
}
