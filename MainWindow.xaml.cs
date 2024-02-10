using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using UniversalTelemetryReplay.Controls;
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
        private readonly string configurationsFileName = @"\configurations.json";
        internal static SettingsFile<Settings>? settingsFile;
        internal static ConfigurationManager<MessageConfiguration>? configManager;
        private double selectedPlaybackSpeed = 0;

        /// Views
        private View currentView;
        static private Pages.Configure  configureView;
        static private Pages.Replay     replayView;
        static private Pages.Settings   settingsView;

        public enum View
        {
            Configure,
            Replay,
            Settings,
        }

        public enum PlayBackStatus
        {
            Unloaded,
            Loaded,
            Started,
            Paused,
            Stopped,
        }

        List<string> SpeedOptions =
        [
            "0.25x",
            "0.5x",
            "1x",
            "1.5x",
            "2x",
            "3x",
            "5x"
        ];

        public MainWindow()
        {
            InitializeComponent();

            configureView = new();
            replayView = new();
            settingsView = new();

            // Get the version information of the assembly
            Assembly assembly = Assembly.GetExecutingAssembly();
            AssemblyName assemblyName = assembly.GetName();
            string version = assemblyName.Version == null ? "" : assemblyName.Version.ToString();

            // Set the title of the MainWindow with the assembly version
            Title = $"Universal Telemetry Replay v{version}";

            programDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), companyFolder, applicationFolder);

            // Ensure the ProgramData folder exists, create it if not
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

            // Update things based on settings.
            ThemeController.SetTheme(settingsFile.data.Theme);
            settingsView.SetThemeSelection(settingsFile.data.Theme);

            // Attempt to load configurations
            string configFilePath = programDataPath + configurationsFileName;
            configManager = new ConfigurationManager<MessageConfiguration>(configFilePath);
            configManager.Load();

            // Populate the ComboBox with playback speed options and set from settings.
            PlaybackSpeedComboBox.ItemsSource = SpeedOptions;
            SetPlaybackSpeed(settingsFile.data.PlaybackSpeed);

            // Update the playback style based on settings
            PlaybackStyleButton.IsChecked = settingsFile.data.ConcurrentPlaybackEnabled;

            // Set default view
            ChangeView(View.Replay);
        }

        /// <summary>Changes the view content</summary>
        /// <param name="view"></param>
        public void ChangeView(View view)
        {
            switch (view)
            {
                case View.Configure:    
                    ContentArea.Content = configureView;    
                    configureView.UpdateConfigurationsDataGrid();
                    ConfigurationsViewButton.Content = "Replay Home";
                    SettingsViewButton.Content = "Settings";
                    currentView = view; 
                    break;
                case View.Replay:       
                    ContentArea.Content = replayView;
                    ConfigurationsViewButton.Content = "Message Configurations";
                    SettingsViewButton.Content = "Settings";
                    currentView = view; 
                    break;
                case View.Settings:     
                    ContentArea.Content = settingsView;
                    SettingsViewButton.Content = "Replay Home";
                    ConfigurationsViewButton.Content = "Message Configurations";
                    currentView = view; 
                    break;
                default: 
                    break;
            }

            currentView = view;
        }

        private void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            Button clickedButton = sender as Button;

            if (clickedButton == ConfigurationsViewButton)
            {
                if (currentView == View.Configure) ChangeView(View.Replay);
                else ChangeView(View.Configure);
            }
            else if (clickedButton == SettingsViewButton)
            {
                if (currentView == View.Settings) ChangeView(View.Replay);
                else ChangeView(View.Settings);
            }
        }

        private void ActionButton_Click(object sender, RoutedEventArgs e)
        {
            Button clickedButton = sender as Button;

            if (clickedButton != null)
            {
                if (clickedButton == LoadButton)
                {
                    UpdatePlaybackControls(PlayBackStatus.Loaded);
                }
                else if (clickedButton == RestartButton)
                {
                    // Handle RestartButton click

                    UpdatePlaybackControls(PlayBackStatus.Started);
                }
                else if (clickedButton == PlayButton)
                {
                    // Handle PlayButton click

                    UpdatePlaybackControls(PlayBackStatus.Started);
                }
                else if (clickedButton == PauseButton)
                {
                    // Handle PauseButton click

                    UpdatePlaybackControls(PlayBackStatus.Paused);
                }
                else if (clickedButton == ResumeButton)
                {
                    // Handle PlayButton click

                    UpdatePlaybackControls(PlayBackStatus.Started);
                }
                else if (clickedButton == StopButton)
                {
                    // Handle StopButton click

                    UpdatePlaybackControls(PlayBackStatus.Stopped);
                }
                else if (clickedButton == ResetButton)
                {
                    // Handle ResetButton click

                    UpdatePlaybackControls(PlayBackStatus.Unloaded);
                }
            }
            else
            {
                ModernToggleButton toggleButton = sender as ModernToggleButton;

                if(toggleButton != null && settingsFile != null && settingsFile.data != null) 
                {
                    settingsFile.data.ConcurrentPlaybackEnabled = toggleButton.IsChecked ?? false;
                    settingsFile.Save();
                }
            }
        }

        private void UpdatePlaybackControls(PlayBackStatus status)
        {
            switch(status) 
            {
                case PlayBackStatus.Unloaded: 
                    LoadButton.Visibility = Visibility.Visible;
                    RestartButton.Visibility = Visibility.Collapsed; 
                    PlayButton.Visibility = Visibility.Collapsed; 
                    PauseButton.Visibility = Visibility.Collapsed;
                    ResumeButton.Visibility = Visibility.Collapsed;
                    StopButton.Visibility = Visibility.Collapsed;
                    ResetButton.Visibility = Visibility.Collapsed;
                    PlaybackStyleButton.IsEnabled = true;
                    break;
                case PlayBackStatus.Loaded:
                    LoadButton.Visibility = Visibility.Collapsed;
                    RestartButton.Visibility = Visibility.Collapsed;
                    PlayButton.Visibility = Visibility.Visible;
                    PauseButton.Visibility = Visibility.Collapsed;
                    ResumeButton.Visibility = Visibility.Collapsed;
                    StopButton.Visibility = Visibility.Collapsed;
                    ResetButton.Visibility = Visibility.Visible;
                    PlaybackStyleButton.IsEnabled = true;
                    break;
                case PlayBackStatus.Started:
                    LoadButton.Visibility = Visibility.Collapsed;
                    RestartButton.Visibility = Visibility.Collapsed;
                    PlayButton.Visibility = Visibility.Collapsed;
                    PauseButton.Visibility = Visibility.Visible;
                    ResumeButton.Visibility = Visibility.Collapsed;
                    StopButton.Visibility = Visibility.Visible;
                    ResetButton.Visibility = Visibility.Collapsed;
                    PlaybackStyleButton.IsEnabled = false;
                    break;
                case PlayBackStatus.Paused:
                    LoadButton.Visibility = Visibility.Collapsed;
                    RestartButton.Visibility = Visibility.Collapsed;
                    PlayButton.Visibility = Visibility.Collapsed;
                    PauseButton.Visibility = Visibility.Collapsed;
                    ResumeButton.Visibility = Visibility.Visible;
                    StopButton.Visibility = Visibility.Visible;
                    ResetButton.Visibility = Visibility.Collapsed;
                    PlaybackStyleButton.IsEnabled = false;
                    break;
                case PlayBackStatus.Stopped:
                    LoadButton.Visibility = Visibility.Collapsed;
                    RestartButton.Visibility = Visibility.Visible;
                    PlayButton.Visibility = Visibility.Collapsed;
                    PauseButton.Visibility = Visibility.Collapsed;
                    ResumeButton.Visibility = Visibility.Collapsed;
                    StopButton.Visibility = Visibility.Collapsed;
                    ResetButton.Visibility = Visibility.Visible;
                    PlaybackStyleButton.IsEnabled = false;
                    break;
            }
        }

        private void PlaybackSpeedComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Check if an item is selected
            if (PlaybackSpeedComboBox.SelectedItem != null)
            {

                SetPlaybackSpeed(PlaybackSpeedComboBox.SelectedItem.ToString());
            }
        }

        private void SetPlaybackSpeed(string speed)
        {
            // Check if the input speed exists in the list of available speeds
            if (SpeedOptions.Contains(speed))
            {
                // Set the ComboBox to display the selected speed
                PlaybackSpeedComboBox.SelectedItem = speed;
                if(settingsFile != null && settingsFile.data != null) 
                {
                    settingsFile.data.PlaybackSpeed = speed;
                    settingsFile.Save();
                }
            }
            else
            {
                // If the input speed is not found, revert to a default speed
                PlaybackSpeedComboBox.SelectedItem = "1x";
            }
        }

    }
}