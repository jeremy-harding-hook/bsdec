using BsdecGui.Outsourcing;
using ReactiveUI;

namespace BsdecGui.ViewModels
{
    internal class ErrorViewModel : ViewModelBase
    {
        private string errors = string.Empty;
        public string Errors
        {
            get => errors;
            protected set => this.RaiseAndSetIfChanged(ref errors, value);
        }

        private int errorCaretIndex;
        public int ErrorCaretIndex
        {
            get => errorCaretIndex;
            set => this.RaiseAndSetIfChanged(ref errorCaretIndex, value);
        }

        public void OnErrorRecieved(object? sender, StringOutputEventArgs e)
        {
            Errors += e.DataOut;
            ErrorCaretIndex = Errors.Length - 1;
        }

        public void ClearErrors()
        {
            Errors = string.Empty;
        }

        public void AddError(string error)
        {
            Errors += error + '\n';
            ErrorCaretIndex = Errors.Length - 1;
        }
    }
}
