using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;

namespace BsdecGui.Views
{
    partial class MessageBox : Window
    {
        public enum MessageBoxButtons
        {
            Ok,
            OkCancel,
            YesNo,
            YesNoCancel,
            DieCancel
        }

        public enum MessageBoxResult
        {
            Ok,
            Cancel,
            Yes,
            No,
            Kill
        }

        public MessageBox()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public static Task<MessageBoxResult> Show(Window? parent, string text, string title, MessageBoxButtons buttons, CancellationTokenSource tokenSource)
        {
            MessageBox msgbox = new()
            {
                Title = title
            };
            TextBlock? textBlock = msgbox.FindControl<TextBlock>("Text");
            if (textBlock != null)
            {
                textBlock.Text = text;
            }

            StackPanel? buttonPanel = msgbox.FindControl<StackPanel>("Buttons") ?? throw new System.Exception("Can't find control 'Buttons' in view.");
            MessageBoxResult result = MessageBoxResult.Ok;

            void AddButton(string caption, MessageBoxResult r, bool defaultResult = false)
            {
                Button button = new() { Content = caption };
                button.Click += (_, __) =>
                {
                    result = r;
                    msgbox.Close();
                };
                if (r == MessageBoxResult.Cancel)
                    button.IsCancel = true;
                if (defaultResult)
                {
                    result = r;
                    button.IsDefault = true;
                }
                buttonPanel.Children.Add(button);
            }

            if (buttons == MessageBoxButtons.Ok || buttons == MessageBoxButtons.OkCancel)
                AddButton("Ok", MessageBoxResult.Ok, buttons == MessageBoxButtons.Ok);
            if (buttons == MessageBoxButtons.YesNo || buttons == MessageBoxButtons.YesNoCancel)
            {
                AddButton("Yes", MessageBoxResult.Yes);
                AddButton("No", MessageBoxResult.No, buttons == MessageBoxButtons.YesNo);
            }

            if (buttons == MessageBoxButtons.OkCancel || buttons == MessageBoxButtons.YesNoCancel)
                AddButton("Cancel", MessageBoxResult.Cancel, true);
            if (buttons == MessageBoxButtons.DieCancel)
            {
                AddButton("\"Die! Die! Die!\"", MessageBoxResult.Kill);
                AddButton("Cancel", MessageBoxResult.Cancel, true);
            }

            TaskCompletionSource<MessageBoxResult> tcs = new();

            MessageBoxResult defaultCancellationResult = MessageBoxResult.Cancel;
            CancellationTokenRegistration registration = tokenSource.Token.Register(() =>
            {
                result = defaultCancellationResult;
                Dispatcher.UIThread.InvokeAsync(() =>
                {
                    msgbox.Close();
                    return Task.CompletedTask;
                });
            });

            msgbox.Closed += delegate
            {
                registration.Unregister();
                tcs.TrySetResult(result);
            };

            // Just in case
            if (tokenSource.IsCancellationRequested)
            {
                registration.Unregister();
                tcs.TrySetResult(defaultCancellationResult);
                return tcs.Task;
            }

            if (parent != null)
                msgbox.ShowDialog(parent);
            else msgbox.Show();
            return tcs.Task;
        }
    }
}
