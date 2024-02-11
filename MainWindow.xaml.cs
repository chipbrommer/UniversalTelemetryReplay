using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
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
        private readonly double selectedPlaybackSpeed = 0;

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

        public enum LogStatus
        {
            Unparsed,
            Parsing,
            Found,
            NotFound,
            Skipped,
            Playing,
            Finished,
        }

        public enum ErrorReason
        {
            None,
            NoFileSelected,
            NoMessageSize,
            NotEnoughSyncBytes,
            NotEnoughEndBytes,
            NoTimestampLocation,
            NoTimestampSize,
        }

        public enum ParseLimit
        {
            [Description("None")]
            None,
            [Description("10%")]
            Percent10,
            [Description("25%")]
            Percent25,
            [Description("1 Message")]
            OneMessage,
            [Description("5 Messages")]
            FiveMessages,
            [Description("10 Messages")]
            TenMessages,
        }

        public enum ReplayLimit
        {
            [Description("1")]
            One = 1,
            [Description("5")]
            Five = 5,
            [Description("10")]
            Ten = 10,
            [Description("20")]
            Twenty = 20,
        }

        readonly List<string> SpeedOptions =
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
            replayView = new(this);
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
            settingsView.SetParseLimitSelection(settingsFile.data.ParseLimit);
            settingsView.SetReplayLimitSelection(settingsFile.data.ReplayLimit);

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
            if (sender is Button clickedButton)
            {
                if (clickedButton == LoadButton)
                {
                    replayView.UpdateAddButton(true);

                    Task.Run(() =>
                    {
                        

                        if (ParseSelectedLogs())
                        {
                            Dispatcher.Invoke(() => UpdatePlaybackControls(PlayBackStatus.Loaded));
                        }
                        else
                        {
                            Dispatcher.Invoke(() => MessageBox.Show("Failed to match log file(s) to a configuration.", "Error", MessageBoxButton.OK, MessageBoxImage.Error));
                        }
                    });
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
                    replayView.UpdateAddButton(false);
                    UpdatePlaybackControls(PlayBackStatus.Unloaded);
                }
            }
            else
            {
                if (sender is ModernToggleButton toggleButton && settingsFile != null && settingsFile.data != null)
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

        private static bool ParseSelectedLogs()
        {
            // Preventive check
            if (configManager == null || configManager.GetData() == null) return false;

            // Success counter
            int success = 0;

            // Attempt the parse the logs
            foreach(LogItem log in replayView.logItems) 
            {
                // Set status to parsing
                UpdateLogStatus(LogStatus.Parsing, log, null);

                // If no path was selected, skip this log. 
                if (log.PathSelected == false)
                {
                    UpdateLogStatus(LogStatus.Skipped, log, null);
                    continue;
                }

                // Attempt to find a configuration for the log
                if (!ParseConfigurations(log))
                {
                    // If here, no matching config was found for this log
                    UpdateLogStatus(LogStatus.NotFound, log, null);
                }
                else success++;
            }

            // return true if any logs successfully parsed
            if(success <= 0) { return false; }
            else return true;
        }

        private static bool ParseConfigurations(LogItem log)
        {
            // Preventive Check
            if (configManager == null) return false;

            // For each configuration in the data, check if the log matches
            foreach (MessageConfiguration config in configManager.GetData())
            {
                // load entries from the telemetry file
                if (File.Exists(log.FilePath))
                {
                    using FileStream fileStream = File.Open(log.FilePath, FileMode.Open);
                    long fileSize = fileStream.Length;
                    int numRead = 0;
                    long totalRead = 0;
                    int bytesInBuffer = 0;
                    byte[] buffer = new byte[config.MessageSize];
                    bool foundStart = false;

                    while ((numRead = fileStream.Read(buffer, bytesInBuffer, buffer.Length - bytesInBuffer)) > 0)
                    {
                        bytesInBuffer += numRead;
                        totalRead += numRead;

                        // Make sure we have enough bytes for a full message
                        if (bytesInBuffer < config.MessageSize) continue;

                        // If we havent found a message, and
                        // if the parse limit has been reached - break out
                        if (settingsFile != null && settingsFile.data != null)
                        {
                            if (foundStart == false && IsParseLimitReached(settingsFile.data.ParseLimit, config, totalRead, fileSize)) break;
                        }

                        // Loop through data to find a message that matches a configuration
                        for (int i = 0; i <= bytesInBuffer - config.MessageSize; i++)
                        {
                            if (buffer[i + 0] == config.SyncByte1 &&
                                buffer[i + 1] == config.SyncByte2 &&
                                (config.SyncByte3 == 0 || buffer[i + 2] == config.SyncByte3) &&
                                (config.SyncByte4 == 0 || buffer[i + 3] == config.SyncByte4) &&
                                buffer[i + config.MessageSize - 2] == config.EndByte1 &&
                                (config.EndByte2 == 0 || buffer[i + config.MessageSize - 1] == config.EndByte2))
                            {
                                bytesInBuffer -= (int)config.MessageSize;

                                if (!foundStart)
                                {
                                    // Set the config index and then update the status
                                    log.ConfigIndex = config.RowIndex;

                                    // Get the start times
                                    log.StartTime = ParseTimestamp(buffer, i + (int)config.TimestampByteOffset, (int)config.TimestampSize, config.TimestampScaling);

                                    foundStart = true;
                                }
                                else
                                {
                                    // Get the end times
                                    log.EndTime = ParseTimestamp(buffer, i + (int)config.TimestampByteOffset, (int)config.TimestampSize, config.TimestampScaling);
                                }

                            }
                            //else
                            //{
                            //    bytesInBuffer--;
                            //}
                        }
                        //// If here and bytesInBuffer isnt 0, move the remaining data to the front of the buffer
                        //if (bytesInBuffer != 0)
                        //{
                        //    Array.Copy(buffer, buffer.Length - bytesInBuffer, buffer, 0, bytesInBuffer);
                        //}
                    }

                    if(log.StartTime != 0 && log.EndTime != 0)
                    {
                        UpdateLogStatus(LogStatus.Found, log, config);
                        return true;
                    }
                }
            
            }

            return false;
        }

        private static bool IsParseLimitReached(ParseLimit limit, MessageConfiguration config, long totalRead, long fileSize)
        {
            double percentageParsed = totalRead / fileSize * 100;

            return limit switch
            {
                ParseLimit.None => false,
                ParseLimit.Percent10 => percentageParsed >= 10,
                ParseLimit.Percent25 => percentageParsed >= 25,
                ParseLimit.OneMessage => totalRead >= config.MessageSize,
                ParseLimit.FiveMessages => totalRead >= (config.MessageSize * 5),
                ParseLimit.TenMessages => totalRead >= (config.MessageSize * 10),
                _ => false,
            };
        }

        private static double ParseTimestamp(byte[] buffer, int offset, int timestampSize, double timestampScaling)
        {
            if (timestampSize == 4)
            {
                uint time = BitConverter.ToUInt32(buffer, offset);
                return time / timestampScaling;
            }
            else if (timestampSize == 8)
            {
                double time = BitConverter.ToDouble(buffer, offset);
                return time / timestampScaling;
            }
            else
            {
                return 0.0;
            }
        }

        public void UpdateControls(bool visible)
        {
            if(visible)
                ControlsGrid.Visibility = Visibility.Visible;
            else
                ControlsGrid.Visibility = Visibility.Hidden;
        }

        public static void UpdateLogStatus(LogStatus pStatus, LogItem log, MessageConfiguration config,ErrorReason error = ErrorReason.None)
        {
            switch(pStatus) 
            {
                case LogStatus.Unparsed:
                    {
                        log.Status = "Not Parsed";
                        log.StatusBG = (Brush)Application.Current.Resources["PrimaryGrayColor"];
                        log.Configuration = "Unknown";
                        log.ConfigBG = (Brush)Application.Current.Resources["PrimaryGrayColor"];
                    }
                    break;
                case LogStatus.Parsing:
                    {
                        log.Status = "Parsing";
                        log.StatusBG = (Brush)Application.Current.Resources["PrimaryYellowColor"];
                        log.Configuration = "Parsing";
                        log.ConfigBG = (Brush)Application.Current.Resources["PrimaryYellowColor"];
                    }
                    break;
                case LogStatus.Found:
                    {
                        log.Status = "Parsed";
                        log.StatusBG = (Brush)Application.Current.Resources["PrimaryGreenColor"];

                        if (log.ConfigIndex != -1 && configManager != null)
                            log.Configuration = configManager.GetData()[log.ConfigIndex].Name;

                        log.ConfigBG = (Brush)Application.Current.Resources["PrimaryGreenColor"];
                    }
                    break;
                case LogStatus.NotFound:
                    {
                        log.Status = "Parsed";
                        log.StatusBG = (Brush)Application.Current.Resources["PrimaryRedColor"];
                        log.Configuration = "Not Found";
                        log.ConfigBG = (Brush)Application.Current.Resources["PrimaryRedColor"];
                    }
                    break;
                case LogStatus.Skipped:
                    {
                        log.Status = "Skipped";
                        log.StatusBG = (Brush)Application.Current.Resources["PrimaryYellowColor"];
                        log.Configuration = "Unknown";
                        log.ConfigBG = (Brush)Application.Current.Resources["PrimaryYellowColor"];
                    }
                    break;
                case LogStatus.Playing:
                    {
                        log.Status = "Playing";
                        log.StatusBG = (Brush)Application.Current.Resources["PrimaryGreenColor"];
                    }
                    break;
                case LogStatus.Finished:
                    {
                        log.Status = "Finished";
                        log.StatusBG = (Brush)Application.Current.Resources["PrimaryBlueColor"];
                    }
                    break;
            }
        }
    }
}