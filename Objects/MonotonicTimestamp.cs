using System.Runtime.InteropServices;

namespace UniversalTelemetryReplay.Objects
{
    public struct MonotonicTimestamp
    {
        private static class NativeMethods
        {
            [DllImport("kernel32.dll")]
            public static extern bool QueryPerformanceCounter(out long value);

            [DllImport("kernel32.dll")]
            public static extern bool QueryPerformanceFrequency(out long value);
        }

        private static double tickFrequency;

        private long timestamp;

        static MonotonicTimestamp()
        {
            if (!NativeMethods.QueryPerformanceFrequency(out var value))
            {
                throw new PlatformNotSupportedException("Requires Windows XP or later");
            }

            tickFrequency = 10000000.0 / (double)value;
        }

        private MonotonicTimestamp(long timestamp)
        {
            this.timestamp = timestamp;
        }

        public static MonotonicTimestamp Now()
        {
            NativeMethods.QueryPerformanceCounter(out var value);
            return new MonotonicTimestamp(value);
        }

        public static TimeSpan operator -(MonotonicTimestamp to, MonotonicTimestamp from)
        {
            if (to.timestamp == 0L)
            {
                throw new ArgumentException("Must be created using MonotonicTimestamp.Now(), not default(MonotonicTimestamp)", "to");
            }

            if (from.timestamp == 0L)
            {
                throw new ArgumentException("Must be created using MonotonicTimestamp.Now(), not default(MonotonicTimestamp)", "from");
            }

            return new TimeSpan((long)((double)(to.timestamp - from.timestamp) * tickFrequency));
        }
    }
}
