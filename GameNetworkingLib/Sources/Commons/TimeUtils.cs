using System;

namespace GameNetworking.Commons {
    public interface ITimeProvider {
        double time { get; }
    }

    public static class TimeUtils {
        public static ITimeProvider provider;

        public static double CurrentTime() {
            return provider?.time ?? TimeSpan.FromTicks(DateTime.Now.Ticks).TotalSeconds;
        }

        public static bool IsOverdue(double startedTime, double interval) {
            return (CurrentTime() - startedTime) > interval;
        }
    }
}