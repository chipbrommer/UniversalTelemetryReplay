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
        public List<byte> SyncBytes { get; set; }
        public uint MessageSize { get; set; }
        public uint TimestampSize { get; set; }
        public uint TimestampByteOffset { get; set; }
        public List<byte> EndBytes { get; set; }

        public override bool Equals(object? obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            MessageConfiguration other = (MessageConfiguration)obj;

            // Check if all properties are equal
            return Name == other.Name &&
                   SyncBytes.SequenceEqual(other.SyncBytes) &&
                   MessageSize == other.MessageSize &&
                   TimestampSize == other.TimestampSize &&
                   TimestampLocation == other.TimestampLocation &&
                   EndBytes.SequenceEqual(other.EndBytes);
        }

        public override int GetHashCode()
        {
            unchecked 
            {
                int hash = 17;
                hash = hash * 23 + (Name != null ? Name.GetHashCode() : 0);
                hash = hash * 23 + (SyncBytes != null ? SyncBytes.GetHashCode() : 0);
                hash = hash * 23 + MessageSize.GetHashCode();
                hash = hash * 23 + TimestampSize.GetHashCode();
                hash = hash * 23 + TimestampLocation.GetHashCode();
                hash = hash * 23 + (EndBytes != null ? EndBytes.GetHashCode() : 0);
                return hash;
            }
        }
    }
}
