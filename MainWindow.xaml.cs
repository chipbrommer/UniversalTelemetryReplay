using System.IO;
using System.Windows;
using UniversalTelemetryReplay.Objects;

namespace UniversalTelemetryReplay
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string programDataPath = string.Empty;
        private readonly string companyFolder = "InnovativeConcepts";
        private readonly string applicationFolder = "UniversalTelemetryReplay";
        private readonly string settingsFileName = @"\settings.json";
        internal static SettingsFile<Settings>? settingsFile;
        internal static Settings? settings;

        public MainWindow()
        {
            InitializeComponent();

            programDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), companyFolder, applicationFolder);

            // Ensure the ProgramData folder exists, create it if it doesn't
            if (!Directory.Exists(programDataPath))
            {
                Directory.CreateDirectory(programDataPath);
            }

            // Handle settings file
            string settingsFilePath = programDataPath + settingsFileName;
            settingsFile = new SettingsFile<Settings>(settingsFilePath);

            // Attempt to load settings
            if (!settingsFile.Load() || settingsFile.data == null)
            {
                settingsFile.data = new();
            }
            settings = settingsFile.data;

            // Update things based on settings.
            ThemeController.SetTheme(settings.theme);
        }
    }
}