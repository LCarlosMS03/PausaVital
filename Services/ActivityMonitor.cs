using System;
using System.Runtime.InteropServices;

namespace PausaVital.Services
{
    public class ActivityMonitor
    {
        // Required structure for the GetLastInputInfo function.
        [StructLayout(LayoutKind.Sequential)]
        private struct LASTINPUTINFO
        {
            public uint cbSize;
            public uint dwTime;
        }

        // Native function from Windows.
        [DllImport("user32.dll")]
        private static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

        /// <summary>
        /// Calculates the total time the user has been completely idle (no mouse/keyboard).
        /// </summary>
        public static TimeSpan GetIdleTime()
        {
            LASTINPUTINFO lastInput = new LASTINPUTINFO
            {
                cbSize = (uint)Marshal.SizeOf<LASTINPUTINFO>()
            };

            if (!GetLastInputInfo(ref lastInput))
            {
                return TimeSpan.Zero;
            }

            // GetLastInputInfo returns a 32-bit Windows tick count. Environment.TickCount64 is safer,
            // but we intentionally keep only the lower 32 bits so subtraction handles DWORD wraparound.
            uint currentTick = unchecked((uint)Environment.TickCount64);
            uint idleTicks = unchecked(currentTick - lastInput.dwTime);

            return TimeSpan.FromMilliseconds(idleTicks);
        }
    }
}
