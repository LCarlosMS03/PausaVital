using System;
using System.Runtime.InteropServices;

namespace PausaVital.Services
{
    public class ActivityMonitor
    {
        // Required structure for the GetLastInputInfo function
        [StructLayout(LayoutKind.Sequential)]
        private struct LASTINPUTINFO
        {
            public uint cbSize;
            public uint dwTime;
        }

        // Native function from Windows
        [DllImport("user32.dll")]
        private static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

        /// <summary>
        /// Calculates the total time the user has been completely idle (no mouse/keyboard).
        /// </summary>
        public static TimeSpan GetIdleTime()
        {
            LASTINPUTINFO lastInPut = new LASTINPUTINFO();
            lastInPut.cbSize = (uint)Marshal.SizeOf(lastInPut);

            if (GetLastInputInfo(ref lastInPut))
            {
                // Windows returns the tick count at the last input, so we need to calculate the difference from the current tick count
                uint lastInputTick = lastInPut.dwTime;
                uint currentTick = (uint)Environment.TickCount;
                uint idleTicks = currentTick - lastInputTick;

                return TimeSpan.FromMilliseconds(idleTicks);
            }

            return TimeSpan.Zero;
        }
    }
}