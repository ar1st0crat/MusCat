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
        public const int MaxSongs = 10;

        // audio player
        public AudioPlayer Player { get; } = new AudioPlayer();

        // collection of recently played songs
        public ObservableCollection<Song> SongArchive { get; } = new ObservableCollection<Song>();
        // collection of upcoming songs
        public ObservableCollection<Song> UpcomingSongs { get; } = new ObservableCollection<Song>();

        public Song CurrentSong { get; private set; } = new Song();

        public Song PrevSong => SongArchive.Any() ? SongArchive.Last() : null;

        public Song NextSong => UpcomingSongs.First();

        public void AddSong() => UpcomingSongs.Add(SelectRandomSong());

        public void AddToArchive() => SongArchive.Add(CurrentSong);


        /// <summary>
        /// Make initial playlist and select random song as the current one
        /// </summary>
        public void MakeSonglist()
        {
            CurrentSong = SelectRandomSong();

            for (var i = 0; i < MaxSongs; i++)
            {
                AddSong();
            }
        }

        public void MoveToPrevSong()
        {
            if (!SongArchive.Any())
            {
                return;
            }

            // shrink list of upcoming songs
            UpcomingSongs.Insert(0, CurrentSong);
            UpcomingSongs.Remove(UpcomingSongs.Last());

            // reassign current song (take last song from archive)
            CurrentSong = SongArchive.Last();

            // update archive
            SongArchive.Remove(SongArchive.Last());
        }

        public void MoveToNextSong()
        {
            // update archive
            if (SongArchive.Count >= MaxSongs)
            {
                SongArchive.RemoveAt(0);
            }
            SongArchive.Add(CurrentSong);

            // reassign current song (take first item from list of upcoming songs)
            CurrentSong = UpcomingSongs.First();

            // update the list of upcoming songs
            UpcomingSongs.RemoveAt(0);
            AddSong();
        }

        public void PlayCurrentSong()
        {
            var fileSong = FileLocator.FindSongPath(CurrentSong);

            try
            {
                Player.Play(fileSong);
            }
            catch (Exception)
            {
                AddSong();

                MoveToNextSong();
                PlayCurrentSong();
            }
        }

        /// <summary>
        /// The method selects random song from a local database.
        /// The song is guaranteed to be present in user's file system
        /// </summary>
        /// <returns>Songs object selected randomly from the database</returns>
        public Song SelectRandomSong()
        {
            Song song;
            var songSelector = new Random();

            // the only way to find out how many mp3 files are actually on user's drive is to try...
            const int maxAttempts = 50;
            var attempts = 0;

            using (var context = new MusCatEntities())
            {
                // find out the maximum song ID in the database
                var maxSid = context.Songs.Max(s => s.ID);

                // keep selecting song randomly until the song file is actually present in the file system...
                // ...and while it isn't present in archive of recently played songs and upcoming songs
                do
                {
                    var songId = songSelector.Next() % maxSid;

                    song = context.Songs.First(s => s.ID >= songId);

                    // include the corresponding album of our song
                    song.Album = context.Albums.First(a => a.ID == song.AlbumID);

                    // do the same thing with performer for included album
                    song.Album.Performer = context.Performers.First(p => p.ID == song.Album.PerformerID);

                    attempts++;
                    if (attempts > maxAttempts)
                    {
                        return null;    // no songs - no radio (((
                    }
                }
                while (SongArchive.Any(s => s.ID == song.ID)    // true, if the archive already contains this song
                    || UpcomingSongs.Any(s => s.ID == song.ID)  // true, if it is already in songlist
                    || song.ID == CurrentSong.ID                // true, if it's currently playing
                    || FileLocator.FindSongPath(song) == "");   // true, if the file with this song doesn't exist
            }

            return song;
        }

        public void ChangeSong(long songId)
        {
            for (var i = 0; i < MaxSongs; i++)
            {
                if (UpcomingSongs[i].ID != songId)
                {
                    continue;
                }
                UpcomingSongs[i] = SelectRandomSong();
                return;
            }
        }

        public void RemoveSong(long songId)
        {
            for (var i = 0; i < MaxSongs; i++)
            {
                if (UpcomingSongs[i].ID != songId)
                {
                    continue;
                }
                UpcomingSongs.RemoveAt(i);
                UpcomingSongs.Add(SelectRandomSong());
                return;
            }
        }
    }
}
