//-----------------------------------------------------------------------
//
// Copyright 2023 Jeremy Harding Hook
//
// This file is part of Bsdec.
//
// Bsdec is free software: you can redistribute it and/or modify it under
// the terms of the GNU General Public License as published by the Free
// Software Foundation, either version 3 of the License, or (at your option)
// any later version.
//
// Bsdec is distributed in the hope that it will be useful, but WITHOUT ANY
// WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
// FOR A PARTICULAR PURPOSE. See the GNU General Public License for more
// details.
//
// You should have received a copy of the GNU General Public License along with
// Bsdec. If not, see <https://www.gnu.org/licenses/>.
//
//-----------------------------------------------------------------------

using System;
namespace Bsdec.Functions
{
    internal static class About
    {
        private static readonly string aboutText =
            $"Bsdec Copyright (C) 2023  Jeremy Harding Hook\n\n" +
            $"Bsdec is free software: you can redistribute it and/or modify it under\n" +
            $"the terms of the GNU General Public License as published by the Free\n" +
            $"Software Foundation, either version 3 of the License, or (at your option)\n" +
            $"any later version.\n\n" +
            $"Bsdec is distributed in the hope that it will be useful, but WITHOUT ANY\n" +
            $"WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS\n" +
            $"FOR A PARTICULAR PURPOSE. See the GNU General Public License for more\n" +
            $"details, available through the command '{Program.ProgramFileName} --license'.";

        public static void ShowAboutText()
        {
            Console.WriteLine(aboutText);
        }
    }
}
