﻿namespace UniversalTelemetryReplay.Objects
{
    /// <summary>Class to hold all settings for the application</summary>
    public class Settings
    {
        public ThemeController.ThemeTypes Theme = ThemeController.ThemeTypes.Modern;
        public MainWindow.ParseLimit ParseLimit = MainWindow.ParseLimit.OneMessage;
        public MainWindow.ReplayLimit ReplayLimit = MainWindow.ReplayLimit.Five;
        public MainWindow.TimestampDeltaLimit TimestampDeltaLimit = MainWindow.TimestampDeltaLimit.Minutes5;
        public string PlaybackSpeed = "";
        public bool ConcurrentPlaybackEnabled = false;
        public bool DebugMode = true;
    }
}
