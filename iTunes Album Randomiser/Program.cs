using iTunes_Wrapper;
using iTunesLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iTunes_Album_Randomiser
{
    class Program
    {
        //===========================================Settings===========================================

        //The playlist that albums should be selected from. Leave this blank to select albums from the 
        //whole iTunes library
        private static string _sourcePlaylistName = null;

        //The playlist that should be filled with the selected albums. This should already exist.
        private static string _destinationPlaylistName = "Playlist for Syncing to Device";

        //The maximum desired size of the destination playlist
        private static double _maxDestinationPlaylistGB = 20;

        //==============================================================================================

        private static void Main()
        {
            iTunes iTunes = new iTunes();
            iTunes.LogMessage += ITunes_LogMessage;

            IITPlaylist sourcePlaylist;
            if (string.IsNullOrEmpty(_sourcePlaylistName))
            {
                sourcePlaylist = iTunes.GetLibrary();
            }
            else
            {
                sourcePlaylist = iTunes.GetPlaylist(_sourcePlaylistName);
            }

            IITUserPlaylist destinationPlaylist = iTunes.GetPlaylistEditable(_destinationPlaylistName);

            List<Album> albums = iTunes.GetAlbumsInPlaylist(sourcePlaylist);

            //Select which albums to add
            Output("Selecting albums to add...");
            Random rnd = new Random();
            List<Album> albumsToAdd = new List<Album>();
            double cumulativeBytes = 0;
            while (Helpers.BytesToGB(cumulativeBytes) < _maxDestinationPlaylistGB)
            {
                int r = rnd.Next(albums.Count);
                Album albumToAdd = albums[r];

                albumsToAdd.Add(albumToAdd);
                cumulativeBytes += albumToAdd.Bytes;

                //Make sure we don't choose this album again
                albums.Remove(albumToAdd);
            }
            PersistentID[] persistentTrackIDsForPlaylist = albumsToAdd.SelectMany(x => x.PersistentTrackIDs).ToArray();
            Output($"Selected {albumsToAdd.Count} albums ({persistentTrackIDsForPlaylist.Length} tracks) to add, giving a playlist size of {Helpers.BytesToGBString(cumulativeBytes)}");
            Output("");

            //Clear destination playlist of the tracks we no longer want to include
            iTunes.ClearPlaylist(destinationPlaylist, persistentTrackIDsForPlaylist);

            //Add desired songs to destination playlist if they're not already included
            iTunes.AddTracksToPlaylistIfNeeded(sourcePlaylist, destinationPlaylist, persistentTrackIDsForPlaylist);

            if (destinationPlaylist.Tracks.Count != persistentTrackIDsForPlaylist.Length)
            {
                Output($"Hmmm, expected {persistentTrackIDsForPlaylist.Length} tracks in playlist but only found {destinationPlaylist.Tracks.Count} :(", true);
            }

            Output($"DONE! Added {albumsToAdd.Count} albums ({destinationPlaylist.Tracks.Count} tracks). Playlist size = {Helpers.BytesToGBString(destinationPlaylist.Size)}", true);
        }

        private static void ITunes_LogMessage(object sender, LogMessageEventArgs e)
        {
            Output(e.Message, e.ShouldExit);
        }

        private static void Output(string text, bool shouldExit = false)
        {
            Console.WriteLine(text);

            if (shouldExit)
            {
                Console.ReadLine();
                Environment.Exit(0);
            }
        }
    }
}
