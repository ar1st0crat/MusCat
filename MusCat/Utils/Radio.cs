using System;
using System.Collections.ObjectModel;
using System.Linq;
using MusCat.Model;

namespace MusCat.Utils
{
    /// <summary>
    /// Radio station class
    /// </summary>
    class Radio
    {
        // the list of recently played songs
        private int _curSong;

        // the number of recently played songs which we're tracking
        public const int MaxSongsInArchive = 25;

        // audio player
        public AudioPlayer Player { get; } = new AudioPlayer();

        // collection of recently played songs
        public ObservableCollection<Song> SongArchive { get; } = new ObservableCollection<Song>();


        public Song CurrentSong()
        {
            return _curSong >= 0 && _curSong < SongArchive.Count ? SongArchive[_curSong] : null;
        }

        public Song PrevSong()
        {
            return _curSong > 0 ? SongArchive[_curSong - 1] : null;
        }

        public Song NextSong()
        {
            return _curSong < SongArchive.Count - 1 ? SongArchive[_curSong + 1] : null;
        }

        public void AddSong() => SongArchive.Add(SelectRandomSong());

        /// <summary>
        /// if the current song isn't the first one in a songlist
        /// we can safely decrease the current position in the songlist
        /// </summary>
        public void MoveToPrevSong()
        {
            if (_curSong > 0)
            {
                _curSong--;
            }
        }

        public void MoveToNextSong()
        {
            // check if the archive ("songlist story") is full 
            if (SongArchive.Count >= MaxSongsInArchive)
            {
                SongArchive.RemoveAt(0);       // if we remove the first element then we don't have to increase currentSongNo
            }
            else
            {
                _curSong++;             // otherwise - increase currentSongNo
            }

            // if the next song is the last one in the playlist
            // then we should select new upcoming song and add it to the playlist
            if (_curSong == SongArchive.Count - 1)
            {
                AddSong();
            }
        }

        public void PlayCurrentSong()
        {
            var fileSong = FileLocator.FindSongPath(SongArchive[_curSong]);

            try
            {
                Player.Play(fileSong);
            }
            catch (Exception)
            {
                SongArchive.RemoveAt(_curSong--);

                // if the next song is the last one in the playlist
                // then we should select new upcoming song and add it to the playlist
                if (_curSong == SongArchive.Count - 1)
                {
                    AddSong();
                }

                MoveToNextSong();
                PlayCurrentSong();
            }
        }

        /// <summary>
        /// The method selects random song from a local database.
        /// The song is guaranteed to be present in user's file system
        /// </summary>
        /// <returns>Songs object selected randomly from the database</returns>
        public Song SelectRandomSong(bool onlyShortSongs = false)
        {
            Song song;
            var songSelector = new Random();

            const int MaxAttempts = 50;
            var attempts = 0;

            using (var context = new MusCatEntities())
            {
                // find out the maximum song ID in the database
                var maxSid = context.Songs.Max(s => s.ID);

                // we keep select song randomly until the song file is actually present in our file system...
                // ...and while it isn't present in our archive of recently played songs
                do
                {
                    IQueryable<Song> selectedsongs;
                    do
                    {
                        // generate random song ID
                        var songNo = songSelector.Next() % maxSid;

                        // the problem here is that our generated ID isn't necessarily present in the database
                        // however there will be at least one song with songID that is greater or equal than this ID
                        selectedsongs = (from s in context.Songs
                                         where s.ID >= songNo
                                         select s)
                                         .Take(1);

                        // if the filter "Short songs is 'on'" we do additional filtering
                        if (onlyShortSongs)
                        {
                            selectedsongs = selectedsongs.Where(
                                    s => s.TimeLength.Length <= 4 && 
                                    string.Compare(s.TimeLength, "1:30", StringComparison.Ordinal) < 0);
                        }
                    }
                    while (!selectedsongs.Any());

                    // select the first song from the set of selected songs
                    song = selectedsongs.First();

                    // include the corresponding album of our song
                    song.Album = (from a in context.Albums
                                  where a.ID == song.AlbumID
                                  select a).First();

                    // do the same thing with performer for included album
                    song.Album.Performer = (from p in context.Performers
                                            where p.ID == song.Album.PerformerID
                                            select p).First();

                    attempts++;
                    if (attempts > MaxAttempts)
                    {
                        return null;
                    }
                }
                while (SongArchive.Any(s => s.ID == song.ID)      // true, if the archive already contains this song
                    || FileLocator.FindSongPath(song) == "");     // true, if the file with this song doesn't exist
            }

            return song;
        }
    }
}
