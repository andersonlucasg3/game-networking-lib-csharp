using System;

namespace GameNetworking.Commons {
    public interface ITimeProvider {
        float time { get; }
    }

    public static class TimeUtils {
        public static ITimeProvider provider;

        public static double CurrentTime() {
            if (provider != null) {
                return (double)provider.time;
            }
            return TimeSpan.FromTicks(DateTime.Now.Ticks).TotalSeconds;
        }

        public static bool IsOverdue(double startedTime, double interval) {
            return (CurrentTime() - startedTime) > interval;
        }
    }
}