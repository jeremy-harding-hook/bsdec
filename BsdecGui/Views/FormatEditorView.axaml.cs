using Avalonia.Controls;
using BsdecGui.ViewModels;
using System.Threading.Tasks;

namespace BsdecGui.Views
{
    public partial class FormatEditorView : UserControl
    {
        public FormatEditorView()
        {
            InitializeComponent();
        }

        string cachedText = string.Empty;

        public void OnTextChanged(object? sender, TextChangedEventArgs e)
        {
            if (DataContext is not FormatEditor editor)
                return;
            Task.Run(() =>
            {
                if (editor.TextChangePending)
                {
                    editor.TextChangePending = false;
                    editor.Sync();
                }
            });
        }
    }
}
