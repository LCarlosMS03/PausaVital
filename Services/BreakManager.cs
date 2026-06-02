using System;

namespace PausaVital.Services
{
    public class BreakManager
    {
        private static readonly TimeSpan WorkThreshold = TimeSpan.FromMinutes(20);
        private static readonly TimeSpan ActiveThreshold = TimeSpan.FromSeconds(30);
        private static readonly TimeSpan LongIdleResetThreshold = TimeSpan.FromMinutes(5);

        private TimeSpan workTime = TimeSpan.Zero;

        public TimeSpan WorkTime => workTime;

        public bool ShouldTakeBreak(TimeSpan idleTime, TimeSpan elapsed)
        {
            if (elapsed <= TimeSpan.Zero)
            {
                return false;
            }

            // If the user has been away for a while, consider that a natural pause and restart the work cycle.
            if (idleTime >= LongIdleResetThreshold)
            {
                workTime = TimeSpan.Zero;
                return false;
            }

            // Count only active work. Small idle periods are normal, but long idle periods should not add time.
            if (idleTime < ActiveThreshold)
            {
                workTime += elapsed;
            }

            if (workTime < WorkThreshold)
            {
                return false;
            }

            workTime = TimeSpan.Zero;
            return true;
        }
    }
}
