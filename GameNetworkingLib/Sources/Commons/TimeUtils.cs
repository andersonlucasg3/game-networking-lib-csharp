using System;

namespace GameNetworking.Commons {
    public static class TimeUtils {
        public static double CurrentTime() {
            return TimeSpan.FromTicks(DateTime.Now.Ticks).TotalSeconds;
        }

        public static bool IsOverdue(double startedTime, double interval) {
            return (CurrentTime() - startedTime) > interval;
        }
    }
}