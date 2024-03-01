using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
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
        private readonly string windowFileName = @"\window.json";
        internal static SettingsFile<WindowInformation>? windowSettings;
        internal static SettingsFile<Objects.Settings>? settingsFile;
        internal static ConfigurationManager<MessageConfiguration>? configManager;
        private double playbackSpeed = 0;
        private double startTime = 0;
        private double endTime = 0;
        private double replayTime = 0;
        public PlayBackStatus currentStatus = PlayBackStatus.Unloaded;
        private List<List<TelemetryMessage>> tmMessages;
        private HashSet<string> copiedFiles = [];
        private bool sliderMoved = false;

        /// Views
        private View currentView;
        static private Pages.Configure  configureView;
        static private Pages.Replay     replayView;
        static private Pages.Settings   settingsView;

        /// Threads
        Thread replayThread;
        private ManualResetEvent stopSignal = new(false);
        private ManualResetEvent pauseSignal = new(false);

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

        public static List<Brush> ItemMappingBrushColors =
        [
            Brushes.Red,
            Brushes.Blue,
            Brushes.Green,
            Brushes.Purple,
            Brushes.Cyan,
            Brushes.Magenta,
            Brushes.Brown,
            Brushes.Goldenrod,
            Brushes.Black,
            Brushes.DarkOrange,
            Brushes.Coral,
            Brushes.DarkOrchid,
            Brushes.DarkBlue, 
            Brushes.DarkKhaki,
            Brushes.DarkOliveGreen,
            Brushes.Maroon,
            Brushes.LightGreen,
            Brushes.LightBlue,
            Brushes.Yellow,
            Brushes.Teal,
        ];

        public MainWindow()
        {
            InitializeComponent();
            tmMessages = [];

            configureView = new();
            replayView = new(this);
            settingsView = new();

            // Get the version information of the assembly
            Assembly assembly = Assembly.GetExecutingAssembly();
            AssemblyName assemblyName = assembly.GetName();
            string version = assemblyName.Version == null ? "" : assemblyName.Version.ToString();

            // Set the title of the MainWindow with the assembly version
            Title = $"Universal Telemetry Replay v{version}";

            programDataPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), companyFolder, applicationFolder);

            // Ensure the ProgramData folder exists, create it if not
            if (!Directory.Exists(programDataPath))
            {
                Directory.CreateDirectory(programDataPath);
            }

            // Handle settings file
            string settingsFilePath = programDataPath + settingsFileName;
            settingsFile = new SettingsFile<Objects.Settings>(settingsFilePath);

            // Attempt to load settings
            if (!settingsFile.Load() || settingsFile.data == null)
            {
                settingsFile.data = new();
            }

            // Handle window file
            string windowFilePath = programDataPath + windowFileName;
            windowSettings = new SettingsFile<WindowInformation>(windowFilePath);
            windowSettings?.Load();

            if(windowSettings?.data != null) 
            {
                Height = windowSettings.data.Height;
                Width = windowSettings.data.Width;
                Top = windowSettings.data.Top;
                Left = windowSettings.data.Left;
                WindowState = windowSettings.data.IsFullscreen ? WindowState.Maximized : WindowState.Normal;
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

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateSliderLogAccents();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            // Save window settings before closing
            if (windowSettings != null && windowSettings.data != null)
            {
                windowSettings.data.Width = Application.Current.MainWindow.Width;
                windowSettings.data.Height = Application.Current.MainWindow.Height;
                windowSettings.data.Top = Application.Current.MainWindow.Top;
                windowSettings.data.Left = Application.Current.MainWindow.Left;
                windowSettings.data.IsFullscreen = Application.Current.MainWindow.WindowState == WindowState.Maximized;

                windowSettings.Save();
            }

            // Make sure any temporary files are deleted
            foreach (string filePath in copiedFiles)
            {
                if (File.Exists(filePath)) File.Delete(filePath);
            }
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
            if (sender is not Button clickedButton) return;

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
                    if(configManager != null && configManager.GetData().Count < 1)
                    {
                        MessageBox.Show("Please add a configuration before attempting to load.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    Task.Run(() =>
                    {
                        if (ParseSelectedLogs())
                        {
                            Dispatcher.Invoke(() =>
                            {
                                UpdatePlaybackControls(PlayBackStatus.Loaded);
                                UpdateReplayContent();

                                replayView.UpdateAddButton(true);

                                foreach (LogItem log in replayView.logItems)
                                {
                                    if(log.ReadyForReplay)
                                    {
                                        log.Locked = true;
                                    }
                                }
                            });
                        }
                        else if(replayView.logItems.Count == 0)
                        {
                            Dispatcher.Invoke(() => MessageBox.Show("Please add a log before loading.", "Error", MessageBoxButton.OK, MessageBoxImage.Error));
                        }
                        else
                        {
                            Dispatcher.Invoke(() => MessageBox.Show("Failed to match log file(s) to a configuration.", "Error", MessageBoxButton.OK, MessageBoxImage.Error));
                        }
                    });
                }
                else if (clickedButton == RestartButton)
                {
                    // Reset the signals
                    stopSignal.Reset();
                    pauseSignal.Reset();

                    // Reset the logs completion info
                    foreach (LogItem log in replayView.logItems)
                    {
                        log.ReplayedPackets = 0;
                        log.PlaybackAmountComplete = 0;
                    }

                    // Start the replay
                    StartReplay();

                    // Update UI
                    UpdatePlaybackControls(PlayBackStatus.Started);
                }
                else if (clickedButton == PlayButton)
                {
                    // Start the replay
                    StartReplay();

                    // Update UI
                    UpdatePlaybackControls(PlayBackStatus.Started);
                }
                else if (clickedButton == PauseButton)
                {
                    // Set the pauseSignal
                    pauseSignal.Set();

                    // Update the UI
                    UpdatePlaybackControls(PlayBackStatus.Paused);
                }
                else if (clickedButton == ResumeButton)
                {
                    // Reset the pauseSignal
                    pauseSignal.Reset();

                    // Update the UI
                    UpdatePlaybackControls(PlayBackStatus.Started);
                }
                else if (clickedButton == StopButton)
                {
                    // Signal the threads to stop.
                    stopSignal.Set();

                    // Wait for threads to finish
                    replayThread?.Join();

                    // Update the UI
                    UpdatePlaybackControls(PlayBackStatus.Stopped);
                }
                else if (clickedButton == ResetButton)
                {
                    // Reset the signals
                    stopSignal.Reset();
                    pauseSignal.Reset();
                    startTime = 0;
                    endTime = 0;
                    ReplaySlider.Value = 0;
                    LogLinesCanvas.Children.Clear();

                    if(settingsFile != null && settingsFile.data != null && !settingsFile.data.ConcurrentPlaybackEnabled) 
                    {
                        ReplayStartTime.Text = "0.0";
                        ReplayEndTime.Text = "0.0";
                    }

                    foreach (LogItem log in replayView.logItems)
                    {
                        log.Locked = false;
                        log.Notify = false;
                        log.Notification = "";
                        log.ReplayedPackets = 0;
                        log.PlaybackAmountComplete = 0;
                    }

                    // Unlock the add button
                    replayView.UpdateAddButton(false);

                    // Update the UI
                    UpdatePlaybackControls(PlayBackStatus.Unloaded);
                }
            }
            else
            {
                if (sender is ModernToggleButton toggleButton && settingsFile != null && settingsFile.data != null)
                {
                    settingsFile.data.ConcurrentPlaybackEnabled = toggleButton.IsChecked ?? false;
                    settingsFile.Save();
                    UpdateReplayContent();
                }
            }
        }

        private void UpdatePlaybackControls(PlayBackStatus status)
        {
            // Capture the status update
            currentStatus = status;

            // Perform work. 
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
                    ReplaySlider.Value = 0;

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
                    PlaybackStyleButton.IsEnabled = true;
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

                    // Try to parse the speed string to a double else set default. 
                    string speedWithoutSuffix = speed.Replace("x", "");
                    if (double.TryParse(speedWithoutSuffix, out double outSpeed)) playbackSpeed = outSpeed;
                    else playbackSpeed = 1.0;
                }
            }
            else
            {
                // If the input speed is not found, revert to a default speed
                PlaybackSpeedComboBox.SelectedItem = "1x";
            }
        }

        private bool ParseSelectedLogs()
        {
            // Preventive check
            if (configManager == null || configManager.GetData() == null) return false;

            // Success counter
            int success = 0;

            // Attempt the parse the logs
            foreach(LogItem log in replayView.logItems) 
            {
                // If the log is already parsed, no need to re-parse. 
                if (log.ReadyForReplay) 
                { 
                    success++;
                    UpdateLogStatus(LogStatus.Found, log);
                    continue; 
                }

                // Initialize the inner list if it hasn't been initialized yet
                int logIndex = replayView.logItems.IndexOf(log);

                // Ensure tmMessages has enough elements to accommodate logIndex
                while (tmMessages.Count <= logIndex)
                {
                    tmMessages.Add([]);
                }
                
                // preventive check against null
                if (tmMessages[logIndex] == null)
                {
                    tmMessages.Add([]);
                }

                // Set status to parsing
                UpdateLogStatus(LogStatus.Parsing, log);

                // If no path was selected, skip this log. 
                if (log.PathSelected == false)
                {
                    UpdateLogStatus(LogStatus.Skipped, log, "No File Selected");
                    continue;
                }

                // Attempt to find a configuration for the log
                if (!ParseConfigurations(log))
                {
                    // If here, no matching config was found for this log
                    UpdateLogStatus(LogStatus.NotFound, log, "No Matching Configuration Found Within Specified Limit");
                }
                else success++;
            }

            // return true if any logs successfully parsed
            if(success <= 0) { return false; }
            else return true;
        }

        private bool ParseConfigurations(LogItem log)
        {
            // Preventive Check
            if (configManager == null) return false;

            // For each configuration in the data, check if the log matches
            foreach (MessageConfiguration config in configManager.GetData())
            {
                // Make sure the config has a valid time stamp. 
                if (config.TimestampByteOffset == 0 || config.TimestampSize == 0) continue;

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

                    // Capture what index this log is. 
                    int logIndex = replayView.logItems.IndexOf(log);

                    while ((numRead = fileStream.Read(buffer, bytesInBuffer, buffer.Length - bytesInBuffer)) > 0)
                    {
                        bytesInBuffer += numRead;

                        // Make sure we have enough bytes for a full message
                        if (bytesInBuffer < config.MessageSize) continue;

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

                                    // Capture the start time and memory location to the list
                                    tmMessages[logIndex].Add(new TelemetryMessage
                                    {
                                        MemoryLocation = totalRead+i,
                                        Timestamp = log.StartTime,
                                    });

                                    foundStart = true;
                                }
                                else
                                {
                                    // Get the end times
                                    log.EndTime = ParseTimestamp(buffer, i + (int)config.TimestampByteOffset, (int)config.TimestampSize, config.TimestampScaling);

                                    // Capture the new log memory location and add to the list
                                    tmMessages[logIndex].Add(new TelemetryMessage
                                    {
                                        MemoryLocation = totalRead + i,
                                        Timestamp = log.EndTime,
                                    });
                                }

                            }
                            //else
                            //{
                            //    bytesInBuffer--;
                            //}
                        }

                        // Update the total bytes Read
                        totalRead += numRead;

                        // If we havent found a message, and
                        // if the parse limit has been reached - break out
                        if (settingsFile != null && settingsFile.data != null)
                        {
                            if (foundStart == false && IsParseLimitReached(settingsFile.data.ParseLimit, config, totalRead, fileSize)) break;
                        }

                        //// If here and bytesInBuffer isnt 0, move the remaining data to the front of the buffer
                        //if (bytesInBuffer != 0)
                        //{
                        //    Array.Copy(buffer, buffer.Length - bytesInBuffer, buffer, 0, bytesInBuffer);
                        //}
                    }

                    if(log.StartTime != 0 && log.EndTime != 0)
                    {
                        UpdateLogStatus(LogStatus.Found, log);
                        log.ReadyForReplay = true;
                        log.TotalPackets = tmMessages[logIndex].Count;
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
                int time = BitConverter.ToInt32(buffer, offset);
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

        public static void UpdateLogStatus(LogStatus pStatus, LogItem log, string error = "")
        {
            switch(pStatus) 
            {
                case LogStatus.Unparsed:
                    {
                        log.Status = "Not Parsed";
                        log.StatusBG = (Brush)Application.Current.Resources["PrimaryGrayColor"];
                        log.Configuration = "Unknown";
                        log.ConfigBG = (Brush)Application.Current.Resources["PrimaryGrayColor"];
                        log.Notify = false;
                        log.Notification = "";
                        log.IsReplayComplete = false;
                    }
                    break;
                case LogStatus.Parsing:
                    {
                        log.Status = "Parsing";
                        log.StatusBG = (Brush)Application.Current.Resources["PrimaryYellowColor"];
                        log.Configuration = "Parsing";
                        log.ConfigBG = (Brush)Application.Current.Resources["PrimaryYellowColor"];
                        log.Notify = false;
                        log.Notification = "";
                        log.IsReplayComplete = false;
                    }
                    break;
                case LogStatus.Found:
                    {
                        log.Status = "Parsed";
                        log.StatusBG = (Brush)Application.Current.Resources["PrimaryGreenColor"];

                        if (log.ConfigIndex != -1 && configManager != null)
                            log.Configuration = configManager.GetData()[log.ConfigIndex].Name;

                        log.ConfigBG = (Brush)Application.Current.Resources["PrimaryGreenColor"];

                        if(error != string.Empty)
                        {
                            log.Notification = error;
                            log.Notify = true;
                        }

                        log.IsReplayComplete = false;
                    }
                    break;
                case LogStatus.NotFound:
                    {
                        log.Status = "Parsed";
                        log.StatusBG = (Brush)Application.Current.Resources["PrimaryRedColor"];
                        log.Configuration = "Not Found";
                        log.ConfigBG = (Brush)Application.Current.Resources["PrimaryRedColor"];

                        if (error != string.Empty)
                        {
                            log.Notification = error;
                            log.Notify = true;
                        }
                    }
                    break;
                case LogStatus.Skipped:
                    {
                        log.Status = "Skipped";
                        log.StatusBG = (Brush)Application.Current.Resources["PrimaryYellowColor"];
                        log.Configuration = "Unknown";
                        log.ConfigBG = (Brush)Application.Current.Resources["PrimaryYellowColor"];

                        if (error != string.Empty)
                        {
                            log.Notification = error;
                            log.Notify = true;
                        }
                    }
                    break;
                case LogStatus.Playing:
                    {
                        log.Status = "Playing";
                        log.StatusBG = (Brush)Application.Current.Resources["PrimaryGreenColor"];
                        log.Notify = false;
                        log.Notification = "";
                        log.IsReplayComplete = false;
                    }
                    break;
                case LogStatus.Finished:
                    {
                        log.Status = "Finished";
                        log.StatusBG = (Brush)Application.Current.Resources["PrimaryBlueColor"];
                        log.Notify = false;
                        log.Notification = "";
                        log.IsReplayComplete = true;
                    }
                    break;
            }
        }

        private void StartReplay()
        {
            // Start the replay thread
            replayThread = new(StartReplayWorker);
            replayThread.Start();
        }

        private void StartReplayWorker()
        {
            UdpClient udp = new() { EnableBroadcast = true };
            List<IPEndPoint> logEndpoints = [];
            List<FileStream> logFileStreams = [];
            List<MonotonicTimestamp> logLastSentTime = [];
            List<double> logReplayTime = [];

            // Keep track of the longest log for concurrent replay type
            LogItem longestLog = new();

            try
            {
                // Create the needed items for each log
                foreach (LogItem log in replayView.logItems)
                {
                    logEndpoints.Add(new IPEndPoint(IPAddress.Parse(log.IpAddress), log.Port));

                    // Create a temp copy of the file at the filepath so we dont block the file usage
                    string tempPath = System.IO.Path.Combine(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), companyFolder, applicationFolder), System.IO.Path.GetFileName(log.FilePath));

                    // Check if the file has already been copied, if so, give it a copy number
                    if (copiedFiles.Contains(tempPath))
                    {
                        tempPath = AppendCountToTempPath(tempPath, copiedFiles.Count);
                    }

                    // Add the file to the set of copied files
                    copiedFiles.Add(tempPath);

                    // Copy the content of the original log file to the temporary file
                    File.Copy(log.FilePath, tempPath, true);

                    logFileStreams.Add(File.Open(tempPath, FileMode.Open));
                    logLastSentTime.Add(MonotonicTimestamp.Now());
                    logReplayTime.Add(log.StartTime);

                    // Capture the longestLog 
                    if (log.TotalPackets > longestLog.TotalPackets) longestLog = log;

                    // Update status to playing
                    UpdateLogStatus(LogStatus.Playing, log);
                }

                // Log the start time
                replayTime = startTime;

                // Do some real good work
                bool replayComplete = false;
                while (!replayComplete)
                {
                    // Check if a signal is set - Indicating stop or pause
                    if (stopSignal.WaitOne(0))
                    {
                        break;
                    }
                    else if(pauseSignal.WaitOne(0))
                    {
                        Thread.Sleep(1);
                        continue;
                    }

                    // if slider was moved, update the items appropriately
                    if(sliderMoved)
                    {
                        MonotonicTimestamp n = MonotonicTimestamp.Now();

                        foreach (LogItem log in replayView.logItems)
                        {
                            int logIndex = replayView.logItems.IndexOf(log);

                            Dispatcher.Invoke(() =>
                            {
                                // Based on the replay type, update the data
                                if (settingsFile != null && settingsFile.data != null && settingsFile.data.ConcurrentPlaybackEnabled)
                                {
                                    if (log.ReplayedPackets >= ReplaySlider.Value) log.ReplayedPackets = (int)ReplaySlider.Value;
                                    else if (log.TotalPackets < ReplaySlider.Value) log.ReplayedPackets = log.TotalPackets;
                                    else log.ReplayedPackets = (int)ReplaySlider.Value;
                                    
                                    logReplayTime[logIndex] = tmMessages[logIndex][log.ReplayedPackets-1].Timestamp;
                                }
                                else
                                {
                                    replayTime = ReplaySlider.Value;
                                    log.ReplayedPackets = 0;
                                    foreach(TelemetryMessage msg in tmMessages[logIndex])
                                    {
                                        if(msg.Timestamp < replayTime) log.ReplayedPackets++;
                                    }
                                }

                                if (log.ReplayedPackets == log.TotalPackets) UpdateLogStatus(LogStatus.Finished, log);
                                else UpdateLogStatus(LogStatus.Playing, log);
                            });

                            logLastSentTime[logIndex] = n;
                        }

                        sliderMoved = false;
                    }

                    // For each log in the list, check if time to send
                    foreach (LogItem log in replayView.logItems)
                    {
                        if (!log.IsReplayComplete)
                        {
                            // Capture what index this log is. 
                            int logIndex = replayView.logItems.IndexOf(log);

                            // Preventive check
                            if (configManager == null || configManager.GetData() == null) return;

                            // Calculate adjusted time
                            double adjustedTime = 0;
                            MonotonicTimestamp now = MonotonicTimestamp.Now();

                            // Based on the replay type, handle the replay time. 
                            if (settingsFile != null && settingsFile.data != null && settingsFile.data.ConcurrentPlaybackEnabled)
                            {
                                logReplayTime[logIndex] += (now - logLastSentTime[logIndex]).TotalSeconds * playbackSpeed;
                                adjustedTime = logReplayTime[logIndex];
                            }
                            else 
                            {
                                replayTime += (now - logLastSentTime[logIndex]).TotalSeconds * playbackSpeed;
                                adjustedTime = replayTime;
                            }
                            logLastSentTime[logIndex] = now;

                            // Get the config and make sure it's not null
                            MessageConfiguration config = configManager.GetData()[replayView.logItems[logIndex].ConfigIndex];
                            if (config == null) return;

                            // Create a buffer to hold the message
                            byte[] data = new byte[config.MessageSize];

                            // Check if it's time to send any of the messages for this log
                            while (log.ReplayedPackets < log.TotalPackets && tmMessages[logIndex][log.ReplayedPackets].Timestamp <= adjustedTime)
                            {
                                // Get the message from the memory location
                                logFileStreams[logIndex].Seek(tmMessages[logIndex][log.ReplayedPackets].MemoryLocation, SeekOrigin.Begin);
                                logFileStreams[logIndex].Read(data, 0, data.Length);

                                // Attempt to send the message
                                if (udp.Send(data, data.Length, logEndpoints[logIndex]) > 0)
                                {
                                    // Update the send count
                                    log.ReplayedPackets++;
                                    log.PlaybackAmountComplete = log.ReplayedPackets / log.TotalPackets;
                                }

                                // Check if the file is complete
                                if (log.ReplayedPackets == log.TotalPackets) UpdateLogStatus(LogStatus.Finished, log);
                            }

                            // Update the longest log
                            if (longestLog.TotalPackets == log.TotalPackets) longestLog = log;
                        }
                    }

                    // Check if all logs are complete
                    if (replayView.logItems.All(log => log.IsReplayComplete))
                    {
                        replayComplete = true;
                    }

                    // Update ReplaySlider - outside loop to prevent overloading UI
                    if (settingsFile != null && settingsFile.data != null && settingsFile.data.ConcurrentPlaybackEnabled)
                    {
                        Dispatcher.Invoke(() => ReplaySlider.Value = longestLog.ReplayedPackets);
                    }
                    else
                    {
                        Dispatcher.Invoke(() => ReplaySlider.Value = replayTime);
                    }

                    // Take a breather
                    Thread.Sleep(1);
                }
            }
            finally
            {
                // Close UDP client
                udp.Close();

                // Close file streams
                foreach (var fileStream in logFileStreams)
                {
                    fileStream.Close();
                }

                // Delete all temporary files permanently
                foreach (string filePath in copiedFiles)
                {
                    if (File.Exists(filePath)) File.Delete(filePath);
                }
            }
        }

        public void UpdateReplayContent()
        {
            if (settingsFile != null && settingsFile.data != null && settingsFile.data.ConcurrentPlaybackEnabled)
            {
                LogItem? longestLog = null;
                double longestDuration = 0;

                // Find the longest file
                foreach (LogItem log in replayView.logItems)
                {
                    if (log != null && log.ReadyForReplay)
                    {
                        double duration = log.EndTime - log.StartTime;
                        if (duration > longestDuration)
                        {
                            longestDuration = duration;
                            longestLog = log;
                        }
                    }
                }

                // Update start and end to be a percentage
                ReplayStartTime.Text = "0 %";
                ReplayEndTime.Text = "100 %";
                ReplaySlider.Minimum = 0;
                ReplaySlider.Maximum = 100;

                // Update the start and end with the longest files total filecount
                if (longestLog != null)
                {
                    startTime = longestLog.StartTime;
                    endTime = longestLog.EndTime;
                    ReplaySlider.Minimum = 0;
                    ReplaySlider.Maximum = longestLog.TotalPackets;
                }
            }
            else
            {
                // Get our starting and ending time points
                foreach (LogItem log in replayView.logItems)
                {
                    if (log != null && log.ReadyForReplay)
                    {
                        if (startTime == 0 || log.StartTime < startTime) startTime = log.StartTime;
                        if (endTime == 0 || log.EndTime > endTime) endTime = log.EndTime;
                    }
                }

                // Update UI items with 2 decimal places precision
                ReplayStartTime.Text = startTime.ToString("F2");
                ReplayEndTime.Text = endTime.ToString("F2");
                ReplaySlider.Minimum = startTime;
                ReplaySlider.Maximum = endTime;
            }

            // Update the tick frequency to every 1% and set value to 0
            ReplaySlider.TickFrequency = (ReplaySlider.Maximum - ReplaySlider.Minimum) / 100;
            ReplaySlider.Value = 0;

            // Update the accents for the slider
            UpdateSliderLogAccents();
        }

        private void UpdateSliderLogAccents()
        {
            // if we arent loaded or if the count is 0, return
            if(currentStatus == PlayBackStatus.Unloaded || tmMessages.Count == 0) { return; }

            // Clear any existing lines from the canvas
            LogLinesCanvas.Children.Clear();

            // Calculate the width of the canvas
            double canvasWidth = LogLinesCanvas.ActualWidth;

            // Define a variable to keep track of the vertical position of each line
            double currentY = 0;

            // Draw lines for each log item
            foreach (LogItem log in replayView.logItems)
            {
                if (log != null)
                {
                    // Perform calculations for a time based replay, starting with the logs start and end time. 
                    double logStartTime = log.StartTime;
                    double logEndTime = log.EndTime;

                    // Calculate positions for the start and end points of the line
                    double startX = (logStartTime - startTime) / (endTime - startTime) * canvasWidth;
                    double endX = (logEndTime - startTime) / (endTime - startTime) * canvasWidth;

                    // If its a concurrent style replay, show all lines on the far left. 
                    if (settingsFile != null && settingsFile.data != null && settingsFile.data.ConcurrentPlaybackEnabled)
                    {
                        double logLength = logEndTime - logStartTime;
                        double totalDuration = endTime - startTime;

                        // Calculate the scaling factor
                        double scaleFactor = canvasWidth / totalDuration;

                        // Adjust startX and endX based on the scaling factor
                        startX = 0;
                        endX = logLength * scaleFactor;
                    }

                    // Create and configure a Line shape
                    Line line = new()
                    {
                        X1 = startX,
                        Y1 = currentY, // Set Y1 to current vertical position
                        X2 = endX,
                        Y2 = currentY, // Set Y2 to current vertical position
                        Stroke = log.LogColor,
                        StrokeThickness = 2
                    };

                    // Increment the vertical position for the next line
                    currentY += 3;

                    // Add the line to the Canvas
                    LogLinesCanvas.Children.Add(line);
                }
            }
        }

        private static string AppendCountToTempPath(string tempPath, int count)
        {
            // Remove the file extension
            string fileNameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(tempPath);

            // Add the count to the filename
            string newFileName = $"{fileNameWithoutExtension}_{count}";

            // Re-add the file extension
            string fileExtension = System.IO.Path.GetExtension(tempPath);

            // Combine the new file name with the file extension
            string newTempPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(tempPath), newFileName + fileExtension);

            return newTempPath;
        }

        private void ReplaySlider_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            // Signal pause when mouse is clicked
            pauseSignal.Set();
        }

        private void ReplaySlider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            // Update replayTime when mouse is released
            replayTime = ReplaySlider.Value;
            sliderMoved = true;

            // Reset pause signal to resume replay
            pauseSignal.Reset();
        }
    }
}