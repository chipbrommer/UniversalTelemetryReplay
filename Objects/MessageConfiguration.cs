using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniversalTelemetryReplay.Objects
{
    public class MessageConfiguration
    {
        public string Name { get; set; }
        public List<byte> SyncBytes { get; set; }
        public uint MessageSize { get; set; }
        public uint TimestampLocation { get; set; }
        public List<byte> EndBytes { get; set; }
    }
}
