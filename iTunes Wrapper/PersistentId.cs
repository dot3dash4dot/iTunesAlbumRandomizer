using iTunesLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iTunes_Wrapper
{
    public class PersistentID : IEquatable<PersistentID>
    {
        public int High { get; }
        public int Low { get; }

        public PersistentID(int high, int low)
        {
            High = high;
            Low = low;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as PersistentID);
        }

        public bool Equals(PersistentID other)
        {
            return other != null && High == other.High && Low == other.Low;
        }

        public override int GetHashCode()
        {
            var hashCode = 773927110;
            hashCode = hashCode * -1521134295 + High.GetHashCode();
            hashCode = hashCode * -1521134295 + Low.GetHashCode();
            return hashCode;
        }
    }

    public static class Extensions
    {
        public static PersistentID GetPersistentID(this IITTrack track, iTunesApp app)
        {
            app.GetITObjectPersistentIDs(track, out int high, out int low);
            return new PersistentID(high, low);
        }
    }
}
