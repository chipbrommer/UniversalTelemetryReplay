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

        public static bool operator ==(MonotonicTimestamp left, MonotonicTimestamp right)
        {
            return left.timestamp == right.timestamp;
        }

        public static bool operator !=(MonotonicTimestamp left, MonotonicTimestamp right)
        {
            return !(left == right);
        }

        public static bool operator <(MonotonicTimestamp left, MonotonicTimestamp right)
        {
            return left.timestamp < right.timestamp;
        }

        public static bool operator >(MonotonicTimestamp left, MonotonicTimestamp right)
        {
            return left.timestamp > right.timestamp;
        }

        public static bool operator <=(MonotonicTimestamp left, MonotonicTimestamp right)
        {
            return left.timestamp <= right.timestamp;
        }

        public static bool operator >=(MonotonicTimestamp left, MonotonicTimestamp right)
        {
            return left.timestamp >= right.timestamp;
        }

        public static MonotonicTimestamp operator +(MonotonicTimestamp timestamp, TimeSpan span)
        {
            long ticks = (long)(span.Ticks / tickFrequency);
            return new MonotonicTimestamp(timestamp.timestamp + ticks);
        }

        public static MonotonicTimestamp operator -(MonotonicTimestamp timestamp, TimeSpan span)
        {
            long ticks = (long)(span.Ticks / tickFrequency);
            return new MonotonicTimestamp(timestamp.timestamp - ticks);
        }

        public double ToSeconds()
        {
            return timestamp * tickFrequency;
        }

        public double ToMilliseconds()
        {
            return timestamp * (tickFrequency / 1000.0);
        }

        public double ToMicroseconds()
        {
            return timestamp * (tickFrequency / 1000000.0);
        }

        public double ToNanoseconds()
        {
            return timestamp * (tickFrequency / 1000000000.0);
        }
    }
}
