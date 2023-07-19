using System;

namespace BsdecGui.Outsourcing
{
    internal class StringOutputEventArgs : EventArgs
    {
        public string DataOut { get; set; } = string.Empty;
    }

    internal class BsdecCompletedEventArgs : EventArgs
    {
        public int ExitCode { get; set; } = -1;
        public string Stdout { get; set; } = string.Empty;
        public string Stderr { get; set; } = string.Empty;
    }
}
