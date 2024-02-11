namespace UniversalTelemetryReplay.Objects
{
    /// <summary>Class to hold all settings for the application</summary>
    public class Settings
    {
        public ThemeController.ThemeTypes Theme = ThemeController.ThemeTypes.Modern;
        public MainWindow.ParseLimit ParseLimit = MainWindow.ParseLimit.Percent10;
        public MainWindow.ReplayLimit ReplayLimit = MainWindow.ReplayLimit.Five;
        public string PlaybackSpeed = "";
        public bool ConcurrentPlaybackEnabled = false;
        public bool DebugMode = true;
    }
}
