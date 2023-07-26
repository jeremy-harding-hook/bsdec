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
