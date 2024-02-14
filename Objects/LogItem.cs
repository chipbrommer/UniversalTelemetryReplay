using System.ComponentModel;
using System.Windows.Media;

namespace UniversalTelemetryReplay.Objects
{
    /// <summary> A class to represent a log item</summary>
    public class LogItem : INotifyPropertyChanged
    {
        public static int NEXT_ID = 0;

        private int _id = 0;
        private string _configuration = "";
        private Brush _configBG = Brushes.Gray;
        private Brush _configFG = Brushes.Gray;
        private string _status = "";
        private Brush _statusBG = Brushes.Gray;
        private Brush _statusFG = Brushes.Gray;
        private string _filePath = "";
        private string _ipAddress = "";
        private int _port = 0;
        private double _startTime = 0;
        private double _endTime = 0;
        private double _playbackAmountComplete = 0;
        public bool _pathSelected = false;
        public int _configIndex = -1;
        private Brush _logColor = Brushes.White;
        private bool _readyForReplay = false;
        private int _totalPackets = 0;
        private int _replayedPackets = 0;
        private bool _locked = false;
        private bool _notify = false;
        private string _notification = "";
        private bool _replayComplete = false;

        /// <summary>Get or Set the ID of this log</summary>
        public int Id
        {
            get { return _id; }
            set { _id = value; OnPropertyChanged(nameof(Id)); }
        }

        /// <summary>Get or Set the name of the configuration</summary>
        public string Configuration
        {
            get { return _configuration; }
            set { _configuration = value; OnPropertyChanged(nameof(Configuration)); }
        }

        /// <summary>Get or Set the background color of the config name indicator</summary>
        public Brush ConfigBG
        {
            get { return _configBG; }
            set { _configBG = value; OnPropertyChanged(nameof(ConfigBG)); }
        }

        /// <summary>Get or Set the foreground color of the config name indicator</summary>
        public Brush ConfigFG
        {
            get { return _configFG; }
            set { _configFG = value; OnPropertyChanged(nameof(ConfigFG)); }
        }

        /// <summary>Get or Set the status of the log</summary>
        public string Status
        {
            get { return _status; }
            set { _status = value; OnPropertyChanged(nameof(Status)); }
        }

        /// <summary>Get or Set the background color for the Status indicator</summary>
        public Brush StatusBG
        {
            get { return _statusBG; }
            set { _statusBG = value; OnPropertyChanged(nameof(StatusBG)); }
        }

        /// <summary>Get or Set the foreground color for the Status indicator</summary>
        public Brush StatusFG
        {
            get { return _statusFG; }
            set { _statusFG = value; OnPropertyChanged(nameof(StatusFG)); }
        }

        /// <summary>Get or Set the filepath for the log file</summary>
        public string FilePath
        {
            get { return _filePath; }
            set { _filePath = value; OnPropertyChanged(nameof(FilePath)); }
        }

        /// <summary>Get or Set the ip address for the log replay</summary>
        public string IpAddress
        {
            get { return _ipAddress; }
            set { _ipAddress = value; OnPropertyChanged(nameof(IpAddress)); }
        }

        /// <summary>Get or Set the port number for the log replay</summary>
        public int Port
        {
            get { return _port; }
            set { _port = value; OnPropertyChanged(nameof(Port)); }
        }

        /// <summary>Get or Set the starting time of the log</summary>
        public double StartTime
        {
            get { return _startTime; }
            set { _startTime = value; OnPropertyChanged(nameof(StartTime)); }
        }

        /// <summary>Get or Set the ending time of the log</summary>
        public double EndTime
        {
            get { return _endTime; }
            set { _endTime = value; OnPropertyChanged(nameof(EndTime)); }
        }

        /// <summary>Get or Set the percentage of the playback being complete</summary>
        public double PlaybackAmountComplete
        {
            get { return _playbackAmountComplete; }
            set { _playbackAmountComplete = value; OnPropertyChanged(nameof(PlaybackAmountComplete)); }
        }

        /// <summary>Get or Set the status of the filepath being selected</summary>
        public bool PathSelected
        {
            get { return _pathSelected; }
            set { _pathSelected = value; OnPropertyChanged(nameof(PathSelected)); }
        }

        /// <summary>Get or Set the index of the logs matching config in the config manager</summary>
        public int ConfigIndex
        {
            get { return _configIndex; }
            set { _configIndex = value; OnPropertyChanged(nameof(ConfigIndex)); }
        }

        /// <summary>Get or Set the logs displayed color</summary>
        public Brush LogColor
        {
            get { return _logColor; }
            set { _logColor = value; OnPropertyChanged(nameof(LogColor)); }
        }

        /// <summary>Get or Set the log into a ready for replay state</summary>
        public bool ReadyForReplay
        {
            get { return _readyForReplay; }
            set { _readyForReplay = value; OnPropertyChanged(nameof(ReadyForReplay)); }
        }

        /// <summary>Get or Set the total nuber of packets</summary>
        public int TotalPackets
        {
            get { return _totalPackets; }
            set { _totalPackets = value; OnPropertyChanged(nameof(TotalPackets)); }
        }

        /// <summary>Get or Set the number of replayed packets</summary>
        public int ReplayedPackets
        {
            get { return _replayedPackets; }
            set { _replayedPackets = value; OnPropertyChanged(nameof(ReplayedPackets)); }
        }

        /// <summary>Get or Set the lock into a locked state</summary>
        public bool Locked
        {
            get { return _locked; }
            set { _locked = value; OnPropertyChanged(nameof(Locked)); }
        }

        /// <summary>Enable or disable the notification for the log</summary>
        public bool Notify
        {
            get { return _notify; }
            set { _notify = value; OnPropertyChanged(nameof(Notify)); }
        }

        /// <summary>Get or Set the notification text for the log</summary>
        public string Notification
        {
            get { return _notification; }
            set { _notification = value; OnPropertyChanged(nameof(Notification)); }
        }

        /// <summary>Get or Set if the log has finished replaying</summary>
        public bool IsReplayComplete
        {
            get { return _replayComplete; }
            set { _replayComplete = value; OnPropertyChanged(nameof(IsReplayComplete)); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>Invoker for the PropertyChanged event</summary>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
