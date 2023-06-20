using System;

using Avalonia;
using Avalonia.ReactiveUI;
using static BsdecCore.Logging;

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