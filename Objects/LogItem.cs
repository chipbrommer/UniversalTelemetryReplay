namespace UniversalTelemetryReplay.Objects
{
    public class LogItem
    {
        public static int NEXT_ID = 0;

        public int Id { get; set; }
        public string Configuration { get; set; }
        public string Status { get; set; }
        public string FilePath { get; set; }
        public string IpAddress { get; set; }
        public int Port { get; set; }
    }
}
