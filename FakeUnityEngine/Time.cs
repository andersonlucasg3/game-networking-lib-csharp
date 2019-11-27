namespace UnityEngine {
    public class Time {
        public static float deltaTime { get; set; }
        public static float fixedDeltaTime { get; set; }
        public static float time { get; set; }
    }
}

public static class TimeHelp {
    public static void ResetTime() {
        UnityEngine.Time.time = 0F;
    }
}