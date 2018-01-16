using System;
using System.Diagnostics;

namespace Bulb.Core
{
    static class BulbEvents
    {
        // CATEGORIES
        public static string Category_UserEvents = "User Events";
        public static string Category_GeneralEvents = "General Events";

        // ACTIONS
        public static string Action_Level = "Level Action";
        public static string Action_Game = "Game Action";

        // EVENTS
        public static string GameLaunched = "GameLaunched";
        public static string LevelCompleted = "LevelCompleted";
        public static string LevelStarted = "LevelStarted";
        public static string LevelQuit = "LevelQuit";

        // PARAMETERS
        public static string BuildVersionNumberParam = "BuildVersionNumber";
        public static string LevelIndexParam = "LevelIndex";
        public static string ChapterIndexParam = "ChapterIndex";
        public static string LevelCompletionTime = "LevelCompletionTime";
        public static string LevelPlayTime = "LevelPlayTime";

        private static Stopwatch _stopwatch;

        public static void StartTimer()
        {
            _stopwatch = Stopwatch.StartNew();
        }

        public static TimeSpan FinishTimer()
        {
            _stopwatch.Stop();
            return _stopwatch.Elapsed;
        }
    }
}
