using System.ComponentModel;
using System.Windows.Media;

namespace UniversalTelemetryReplay.Objects
{
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

        public int Id
        {
            get { return _id; }
            set { _id = value; OnPropertyChanged(nameof(Id)); }
        }

        public string Configuration
        {
            get { return _configuration; }
            set { _configuration = value; OnPropertyChanged(nameof(Configuration)); }
        }
        public Brush ConfigBG
        {
            get { return _configBG; }
            set { _configBG = value; OnPropertyChanged(nameof(ConfigBG)); }
        }

        public Brush ConfigFG
        {
            get { return _configFG; }
            set { _configFG = value; OnPropertyChanged(nameof(ConfigFG)); }
        }

        public string Status
        {
            get { return _status; }
            set { _status = value; OnPropertyChanged(nameof(Status)); }
        }

        public Brush StatusBG
        {
            get { return _statusBG; }
            set { _statusBG = value; OnPropertyChanged(nameof(StatusBG)); }
        }

        public Brush StatusFG
        {
            get { return _statusFG; }
            set { _statusFG = value; OnPropertyChanged(nameof(StatusFG)); }
        }

        public string FilePath
        {
            get { return _filePath; }
            set { _filePath = value; OnPropertyChanged(nameof(FilePath)); }
        }

        public string IpAddress
        {
            get { return _ipAddress; }
            set { _ipAddress = value; OnPropertyChanged(nameof(IpAddress)); }
        }

        public int Port
        {
            get { return _port; }
            set { _port = value; OnPropertyChanged(nameof(Port)); }
        }

        public double StartTime
        {
            get { return _startTime; }
            set { _startTime = value; OnPropertyChanged(nameof(StartTime)); }
        }

        public double EndTime
        {
            get { return _endTime; }
            set { _endTime = value; OnPropertyChanged(nameof(EndTime)); }
        }

        public double PlaybackAmountComplete
        {
            get { return _playbackAmountComplete; }
            set { _playbackAmountComplete = value; OnPropertyChanged(nameof(PlaybackAmountComplete)); }
        }

        public bool PathSelected
        {
            get { return _pathSelected; }
            set { _pathSelected = value; OnPropertyChanged(nameof(PathSelected)); }
        }

        public int ConfigIndex
        {
            get { return _configIndex; }
            set { _configIndex = value; OnPropertyChanged(nameof(ConfigIndex)); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
