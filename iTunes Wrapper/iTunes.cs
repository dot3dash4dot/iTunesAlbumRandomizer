using iTunesLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iTunes_Wrapper
{
    public class iTunes
    {
        //Help: http://www.joshkunz.com/iTunesControl/

        private iTunesApp _app;

        public event EventHandler<LogMessageEventArgs> LogMessage;

        public iTunes()
        {
            _app = new iTunesApp();
        }

        public IITLibraryPlaylist GetLibrary()
        {
            return _app.LibraryPlaylist;
        }

        public IITPlaylist GetPlaylist(string playlistName)
        {
            IITPlaylist playlist = _app.LibrarySource.Playlists.ItemByName[playlistName];
            if (playlist == null)
            {
                Log($"Failed to find playlist '{playlistName}'", true);
            }

            return playlist;
        }

        public IITUserPlaylist GetPlaylistEditable(string playlistName)
        {
            IITUserPlaylist playlist = _app.LibrarySource.Playlists.ItemByName[playlistName] as IITUserPlaylist;
            if (playlist == null)
            {
                Log($"Failed to find playlist '{playlistName}'", true);
            }

            return playlist;
        }

        public List<Album> GetAlbumsInPlaylist(IITPlaylist sourcePlaylist)
        {
            List<Album> albums = new List<Album>();
            int trackCount = 0;
            foreach (IITTrack track in sourcePlaylist.Tracks)
            {
                //Ignore any podcasts or videos
                if (track is IITFileOrCDTrack trackFile
                    && !trackFile.Podcast
                    && trackFile.VideoKind == ITVideoKind.ITVideoKindNone)
                {
                    Album album = albums.FirstOrDefault(x => x.WouldContainTrack(track));
                    if (album == null)
                    {
                        album = new Album(track.Artist, track.Album);
                        albums.Add(album);
                    }

                    album.Bytes += track.Size;

                    //I tried storing the direct reference to the tracks here but that just led 
                    //to "track has been deleted" errors when trying to add/remove them to/from 
                    //a playlist later. Instead, store the tracks' IDs and use this to find the
                    //track again just before it needs adding/removing
                    album.PersistentTrackIDs.Add(track.GetPersistentID(_app));
                }
                
                if (trackCount % 250 == 0)
                {
                    Log($"Getting information about track {trackCount} of {sourcePlaylist.Tracks.Count}");
                }

                trackCount++;
            }

            Log("Finished processing tracks to get albums");
            Log("");

            return albums;
        }

        public void ClearPlaylist(IITUserPlaylist playlist, IEnumerable<PersistentID> exceptTrackIDs)
        {
            Log($"Removing unneeded tracks from playlist {playlist.Name}...");

            //iTunes doesn't like you deleting tracks out of the collection you are iterating over which
            //is understandable. But it also doesn't seem to like iterating over Tracks, storing the tracks 
            //in a separate list and then iterating over that and deleting each one. Maybe the tracks'
            //references are dependent on their playlist position? Anyway, that leads to this slightly
            //weird way of deleting everything:
            int playlistIndexToCheck = 1;
            int originalTrackCount = playlist.Tracks.Count;
            int processedTrackCount = 0;
            int removedTrackCount = 0;
            while (playlistIndexToCheck <= playlist.Tracks.Count)
            {
                processedTrackCount++;

                IITTrack currentTrack = playlist.Tracks[playlistIndexToCheck];
                if (exceptTrackIDs.Contains(currentTrack.GetPersistentID(_app)))
                {
                    //Keep this track in the playlist. Add one to the index so we check the next track
                    //in the next loop
                    playlistIndexToCheck++;
                }
                else
                {
                    currentTrack.Delete(); //This just deletes the track from the playlist, not the library!
                    removedTrackCount++;
                }

                if (processedTrackCount % 250 == 0)
                {
                    Log($"{processedTrackCount} of {originalTrackCount} tracks processed: {removedTrackCount} removed");
                }
            }

            Log("Unneeded tracks removed from playlist");
            Log("");
        }

        public void AddTracksToPlaylistIfNeeded(IITPlaylist sourcePlaylist, IITUserPlaylist destinationPlaylist, IEnumerable<PersistentID> tracksToAdd)
        {
            Log("Adding songs to playlist...");

            int processedTrackCount = 0;
            int addedTrackCount = 0;
            foreach (PersistentID id in tracksToAdd)
            {
                IITTrack currentTrack = sourcePlaylist.Tracks.ItemByPersistentID[id.High, id.Low];

                var matchingPlaylist = ((IITFileOrCDTrack)currentTrack).Playlists.ItemByName[destinationPlaylist.Name];
                if (matchingPlaylist == null)
                {
                    //Track is not already in the playlist, so add it
                    destinationPlaylist.AddTrack(currentTrack);
                    addedTrackCount++;
                }

                processedTrackCount++;

                if (processedTrackCount % 250 == 0)
                {
                    Log($"{processedTrackCount} of {tracksToAdd.Count()} tracks processed: {addedTrackCount} added");
                }
            }

            Log("Songs added to the playlist");
            Log("");
        }

        public void Log(string message, bool shouldExit = false)
        {
            LogMessage?.Invoke(this, new LogMessageEventArgs(message, shouldExit));
        }
    }

    public class LogMessageEventArgs : EventArgs
    {
        public string Message { get; set; }
        public bool ShouldExit { get; set; }

        public LogMessageEventArgs(string message, bool shouldExit)
        {
            Message = message;
            ShouldExit = shouldExit;
        }
    }
}
