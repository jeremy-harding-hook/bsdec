using BsdecGui.Outsourcing;
using ReactiveUI;
using static BsdecGui.Logging;

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

        public void OnErrorRecieved(object? sender, StringOutputEventArgs e)
        {
            Errors += e.DataOut;
        }

        public void ClearErrors()
        {
            Errors = string.Empty;
        }

        public void AddError(string error)
        {
            Errors += error + '\n';
        }
    }
}
