using iTunesLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iTunes_Wrapper
{
    public class Album
    {
        public string ArtistName { get; }
        public string AlbumName { get; }
        public List<PersistentID> PersistentTrackIDs { get; } = new List<PersistentID>();
        public int Bytes { get; set; }

        public Album(string artistName, string albumName)
        {
            ArtistName = artistName;
            AlbumName = albumName;
        }

        public bool WouldContainTrack(IITTrack track)
        {
            return ArtistName == track.Artist && AlbumName == track.Album;
        }

        public override string ToString()
        {
            return $"{ArtistName} - {AlbumName}";
        }
    }
}
