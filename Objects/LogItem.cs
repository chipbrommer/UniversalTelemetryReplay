using System.ComponentModel;

namespace UniversalTelemetryReplay.Objects
{
    public class LogItem : INotifyPropertyChanged
    {
        public static int NEXT_ID = 0;

        private int _id;
        private string _configuration;
        private string _status;
        private string _filePath;
        private string _ipAddress;
        private int _port;

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

        public string Status
        {
            get { return _status; }
            set { _status = value; OnPropertyChanged(nameof(Status)); }
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

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
