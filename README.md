# iTunesAlbumRandomizer

Can't fit all your music on your iPhone/iPod? This program adds random albums to a playlist of a user-defined maximum size (in GB) which can then be synced to your device.

## Background

If you have too much music to fit on your iPhone or iPod, you can either manually choose what to include, or you can create a smart playlist and get iTunes to choose random songs for you up to a certain size. The former option takes effort, and the big problem with the latter is there is no way to get iTunes to re-choose which tracks to include - reducing the playlist's size and increasing it again just leads to the same songs being selected rather than a new randomized selection.

It's also nice to choose complete albums to sync to your device rather than random songs. iTunes in theory allows this by using the "Limit to... selected by album" option on the smart playlist, though I've found this doesn't always work for me.

## Solution

This program will scan all the tracks in your library (or an existing playlist if you'd rather select from a particular set of tracks), then randomly adds complete albums to a user-defined playlist until it reaches a particular filesize (also user-defined). It will choose a different selection of albums each time it is run.

In order to use it, simply fill in the three settings at the top of Program.cs.

## Implementation Notes

This program uses the iTunes COM SDK which I've found works in some pretty unexpected ways sometimes, namely it doesn't seem to like you storing references to tracks and then using them later on - this leads to regular "track has been deleted" errors. Therefore after getting the size of each track in an initial scan, the program re-finds each track later when it wants to remove/add it from/to the destination playlist.

Despite this workaround, I've found it still fails occasionally with a "playlist has been deleted" error message. This isn't too common, however, and generally the error doesn't re-occur if you just run the program again.