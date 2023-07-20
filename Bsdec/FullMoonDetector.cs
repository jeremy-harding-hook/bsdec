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

namespace Bsdec
{
    internal static class FullMoonDetector
    {
        public static bool IsFull(DateTime utcDateTime)
        {
            TimeSpan cycleLength = TimeSpan.FromDays(29.53);
            DateTime knownFullMoon = new(1920, 1, 5, 21, 4, 00, DateTimeKind.Utc);
            double deltaDays = TimeSpan.FromTicks((utcDateTime - knownFullMoon).Ticks % cycleLength.Ticks).TotalDays;
            return deltaDays < 1 || deltaDays > cycleLength.TotalDays - 1;
        }

        public static bool IsFullNow()
        {
            return IsFull(DateTime.UtcNow);
        }
    }
}

