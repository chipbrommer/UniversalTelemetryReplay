using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniversalTelemetryReplay.Objects
{
    public class MessageConfiguration
    {
        public int RowIndex { get; set; }
        public string Name { get; set; }
        public byte SyncByte1 { get; set; }
        public byte SyncByte2 { get; set; }
        public byte SyncByte3 { get; set; }
        public byte SyncByte4 { get; set; }
        public uint MessageSize { get; set; }
        public uint TimestampSize { get; set; }
        public uint TimestampByteOffset { get; set; }
        public byte EndByte1 { get; set; }
        public byte EndByte2 { get; set; }

        public override bool Equals(object? obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            MessageConfiguration other = (MessageConfiguration)obj;

            // Check if all properties are equal
            return Name == other.Name &&
                   SyncByte1 == other.SyncByte1 &&
                   SyncByte2 == other.SyncByte2 &&
                   SyncByte3 == other.SyncByte3 &&
                   SyncByte4 == other.SyncByte4 &&
                   MessageSize == other.MessageSize &&
                   TimestampSize == other.TimestampSize &&
                   TimestampByteOffset == other.TimestampByteOffset &&
                   EndByte1 == other.EndByte1 &&
                   EndByte2 == other.EndByte2;
        }

        public override int GetHashCode()
        {
            HashCode hash = new();
            hash.Add(Name);
            hash.Add(SyncByte1);
            hash.Add(SyncByte2);
            hash.Add(SyncByte3);
            hash.Add(SyncByte4);
            hash.Add(MessageSize);
            hash.Add(TimestampSize);
            hash.Add(TimestampByteOffset);
            hash.Add(EndByte1);
            hash.Add(EndByte2);
            return hash.ToHashCode();
        }
    }
}
