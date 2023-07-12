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

