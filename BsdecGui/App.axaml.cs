using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using BsdecGui.ViewModels;
using BsdecGui.Views;
using static BsdecCore.Logging;

namespace BsdecGui
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow();
                desktop.MainWindow.DataContext = new MainViewModel(desktop.MainWindow.StorageProvider);
            }
            else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
            {
                singleViewPlatform.MainView = new MainView();

                TopLevel topLevel = TopLevel.GetTopLevel(singleViewPlatform.MainView) ??
                    throw new System.ApplicationException($"TopLevel is null at {nameof(OnFrameworkInitializationCompleted)}! This will prevent us from doing anything useful.");
                
                singleViewPlatform.MainView.DataContext = new MainViewModel(topLevel.StorageProvider);
            }

            base.OnFrameworkInitializationCompleted();
        }


    }
}