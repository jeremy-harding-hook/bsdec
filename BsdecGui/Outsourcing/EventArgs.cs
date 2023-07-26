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
using System.IO;

namespace BsdecGui.Outsourcing
{
    internal class StringOutputEventArgs : EventArgs
    {
        public string DataOut { get; set; } = string.Empty;
    }

    internal class BsdecCompletedEventArgs : EventArgs
    {
        public int ExitCode { get; set; } = -1;
        public Stream? Stdout { get; set; } = null;
        public string Stderr { get; set; } = string.Empty;
    }
}
