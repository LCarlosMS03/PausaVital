using System;

namespace PausaVital.Services
{
    public class BreakManager
    {
        private TimeSpan workThreshold = TimeSpan.FromMinutes(20);

        private static readonly TimeSpan ActiveThreshold =
            TimeSpan.FromSeconds(30);

        private static readonly TimeSpan LongIdleResetThreshold =
            TimeSpan.FromMinutes(5);

        private TimeSpan workTime = TimeSpan.Zero;

        public TimeSpan WorkTime => workTime;

        public void SetMode(string mode)
        {
            if (mode == "Pomodoro")
            {
                workThreshold = TimeSpan.FromSeconds(10);
            }
            else if (mode == "20-20-20")
            {
                workThreshold = TimeSpan.FromSeconds(10);
            }
        }

        public bool ShouldTakeBreak(
            TimeSpan idleTime,
            TimeSpan elapsed)
        {
            if (elapsed <= TimeSpan.Zero)
            {
                return false;
            }

            if (idleTime >= LongIdleResetThreshold)
            {
                workTime = TimeSpan.Zero;
                return false;
            }

            if (idleTime < ActiveThreshold)
            {
                workTime += elapsed;
            }

            if (workTime < workThreshold)
            {
                return false;
            }

            workTime = TimeSpan.Zero;
            return true;
        }
    }

}
