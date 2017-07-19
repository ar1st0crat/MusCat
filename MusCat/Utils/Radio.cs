using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using MusCat.Model;

namespace MusCat.Utils
{
    /// <summary>
    /// Radio station class
    /// 
    /// Class provides basic operations and their asynchronous analogs
    /// 
    /// </summary>
    class Radio
    {
        public const int MaxSongs = 10;

        // audio player
        public AudioPlayer Player { get; } = new AudioPlayer();

        // randomizer
        private readonly Random _songSelector = new Random();

        // collection of recently played songs
        public ObservableCollection<Song> SongArchive { get; } = new ObservableCollection<Song>();

        // collection of upcoming songs
        public ObservableCollection<Song> UpcomingSongs { get; } = new ObservableCollection<Song>();

        public Song CurrentSong { get; private set; } = new Song();

        public Song PrevSong => SongArchive.Any() ? SongArchive.Last() : null;

        public Song NextSong => UpcomingSongs.Any() ? UpcomingSongs.First() : null;

        public void AddToArchive() => SongArchive.Add(CurrentSong);

        public void AddRandomSong() => UpcomingSongs.Add(SelectRandomSong());


        private readonly object _lock = new object();
        
        public Radio()
        {
            BindingOperations.EnableCollectionSynchronization(SongArchive, _lock);
            BindingOperations.EnableCollectionSynchronization(UpcomingSongs, _lock);
        }

        #region synchronous operations

        /// <summary>
        /// Make initial playlist and select random song as the current one
        /// </summary>
        public void MakeSonglist()
        {
            CurrentSong = SelectRandomSong();

            for (var i = 0; i < MaxSongs; i++)
            {
                AddRandomSong();
            }
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
            AddRandomSong();
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

        public void StartPlaying()
        {
            var fileSong = FileLocator.FindSongPath(CurrentSong);

            try
            {
                Player.Play(fileSong);
            }
            catch (Exception)
            {
                AddRandomSong();
                MoveToNextSong();
                StartPlaying();
            }
        }

        /// <summary>
        /// Method just replaces song (addressed by its ID) with the new randomly selected song 
        /// </summary>
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

        /// <summary>
        /// Method removes song by its ID from the list of upcoming songs
        /// and adds new randonly selected song to the list (to keep its size constant)
        /// </summary>
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

        /// <summary>
        /// The method selects random song from a local database.
        /// The song is guaranteed to be present in user's file system
        /// </summary>
        /// <returns>Song object selected randomly from the database</returns>
        public Song SelectRandomSong()
        {
            Song song;

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
                    var songId = _songSelector.Next() % maxSid;

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

        #endregion


        #region asynchronous operations 

        /// <summary>
        /// Make initial playlist and select random song as the current one
        /// </summary>
        public async Task MakeSonglistAsync()
        {
            CurrentSong = await SelectRandomSongAsync().ConfigureAwait(false);

            // addding songs 1 by 1 prevents the situation when songs can be duplicated

            for (var i = 0; i < MaxSongs; i++)
            {
                await AddRandomSongAsync().ConfigureAwait(false);
            }

            // Alternative code (however, it allows duplicate songs (((:

            //var songAdders = new Task[MaxSongs];

            //// just fire them all at once (order doesn't matter)
            //for (var i = 0; i < MaxSongs; i++)
            //{
            //    songAdders[i] = AddRandomSongAsync();
            //}

            //await Task.WhenAll(songAdders).ConfigureAwait(false);


            // just was playing' with ))

            // Parallel.For(0, MaxSongs, i => AddRandomSong());
            // Parallel.For(0, MaxSongs, i => AddRandomSongAsync().RunSynchronously());
        }

        public async Task AddRandomSongAsync()
        {
            var song = await SelectRandomSongAsync().ConfigureAwait(false);

            UpcomingSongs.Add(song);
        }

        public async Task MoveToNextSongAsync()
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

            await AddRandomSongAsync().ConfigureAwait(false);
        }

        public async Task StartPlayingAsync()
        {
            var fileSong = FileLocator.FindSongPath(CurrentSong);

            try
            {
                Player.Play(fileSong);
            }
            catch (Exception)
            {
                await AddRandomSongAsync().ConfigureAwait(false);
                await MoveToNextSongAsync().ConfigureAwait(false);
                await StartPlayingAsync().ConfigureAwait(false);
            }
        }

        public async Task ChangeSongAsync(long songId)
        {
            for (var i = 0; i < MaxSongs; i++)
            {
                if (UpcomingSongs[i].ID != songId)
                {
                    continue;
                }

                var song = await SelectRandomSongAsync();

                UpcomingSongs[i] = song;
                return;
            }
        }

        public async Task RemoveSongAsync(long songId)
        {
            for (var i = 0; i < MaxSongs; i++)
            {
                if (UpcomingSongs[i].ID != songId)
                {
                    continue;
                }

                await AddRandomSongAsync();

                UpcomingSongs.RemoveAt(i);
                return;
            }
        }

        public async Task<Song> SelectRandomSongAsync()
        {
            return await Task.Run(() => SelectRandomSong()).ConfigureAwait(false);
        }

        #endregion
    }
}
