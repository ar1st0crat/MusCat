using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using MusCat.Entities;

namespace MusCat.Services
{
    /// <summary>
    /// Radio station class
    /// 
    /// Class provides basic methods for making and playing tracklists 
    /// and their asynchronous analogs
    /// 
    /// </summary>
    class Radio
    {
        public const int MaxSongs = 10;

        // Audio player
        private readonly AudioPlayer _player = new AudioPlayer();

        // Background worker - player
        private readonly BackgroundWorker _worker = new BackgroundWorker();
        private bool _isStopped;

        // Delegate that will be invoked when a new song starts playing
        public Action Update { get; set; }

        // Randomizer
        private readonly Random _songSelector = new Random();

        // Collection of recently played songs
        public List<Song> SongArchive { get; } = new List<Song>();

        // Collection of upcoming songs
        public List<Song> UpcomingSongs { get; } = new List<Song>();

        public Song CurrentSong { get; private set; } = new Song();
        public Song PrevSong => SongArchive.LastOrDefault();
        public Song NextSong => UpcomingSongs.FirstOrDefault();

        /// <summary>
        /// There's one general background worker associated with the radio
        /// whise purpose is to play whatever active song in the background thread
        /// </summary>
        public Radio()
        {
            _worker.DoWork += async (o, e) =>
            {
                while (!_isStopped)
                {
                    Task.Delay(1000).Wait();

                    if (_player.IsStopped() && !_player.IsStoppedManually)
                    {
                        await MoveToNextSongAsync();
                    }
                }
            };
        }

        #region playback functions

        public void Start()
        {
            StartPlaying();
            
            _isStopped = false;
            _worker.RunWorkerAsync();
        }

        public void Stop()
        {
            _isStopped = true;
            _player.StopAndDispose();
        }

        public void StartPlaying()
        {
            if (_player.SongPlaybackState != PlaybackState.Stop)
            {
                _player.Stop();
            }

            var fileSong = FileLocator.FindSongPath(CurrentSong);

            try
            {
                _player.Play(fileSong);
            }
            catch (Exception)
            {
                MoveToNextSong();
            }
        }

        public void PausePlaying()
        {
            _player.Pause();
        }

        public void ResumePlaying()
        {
            _player.Resume();
        }

        public void StopPlaying()
        {
            _player.Stop();
        }

        public void SetVolume(float volume)
        {
            _player.SetVolume(volume);
        }

        public PlaybackState SongPlaybackState => _player.SongPlaybackState;

        #endregion


        #region synchronous operations

        public void AddToArchive() => SongArchive.Add(CurrentSong);

        public void AddRandomSong() => UpcomingSongs.Add(SelectRandomSong());

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
            _player.Stop();

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

            Update?.Invoke();

            StartPlaying();
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

            Update?.Invoke();

            StartPlaying();
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
                }
                while (SongArchive.Any(s => s.ID == song.ID)    // true, if the archive already contains this song
                    || UpcomingSongs.Any(s => s.ID == song.ID)  // true, if it is already in songlist
                    || song.ID == CurrentSong.ID                // true, if it's currently playing
                    || FileLocator.FindSongPath(song) == "");   // true, if the file with this song doesn't exist
            }

            return song;
        }

        public bool CheckSongFiles()
        {
            Song song;

            // the only way to find out how many mp3 files are actually on user's drive is to try...
            const int maxAttempts = 50;
            var attempts = 0;

            using (var context = new MusCatEntities())
            {
                var maxSid = context.Songs.Max(s => s.ID);

                // keep selecting song randomly until the song file is actually present in the file system...
                // ...and while it isn't present in archive of recently played songs and upcoming songs
                do
                {
                    var songId = _songSelector.Next() % maxSid;

                    song = context.Songs.First(s => s.ID >= songId);
                    song.Album = context.Albums.First(a => a.ID == song.AlbumID);
                    song.Album.Performer = context.Performers.First(p => p.ID == song.Album.PerformerID);

                    attempts++;
                    if (attempts > maxAttempts)
                    {
                        return false;    // no songs - no radio (((
                    }
                }
                while (UpcomingSongs.Any(s => s.ID == song.ID) ||  // true, if it is already in songlist
                       FileLocator.FindSongPath(song) == "");      // true, if the file with this song doesn't exist
            }

            return true;
        }

        #endregion


        #region asynchronous operations 

        /// <summary>
        /// Make initial playlist and select random song as the current one
        /// </summary>
        public async Task MakeSonglistAsync()
        {
            CurrentSong = await SelectRandomSongAsync().ConfigureAwait(false);

            // adding songs 1 by 1 prevents the situation when songs can be duplicated

            for (var i = 0; i < MaxSongs; i++)
            {
                await AddRandomSongAsync().ConfigureAwait(false);
            }


            // ====== Alternative code (however, it allows duplicate songs (((: ======

            //var songAdders = new Task[MaxSongs];

            //// just fire them all at once (order doesn't matter)
            //for (var i = 0; i < MaxSongs; i++)
            //{
            //    songAdders[i] = AddRandomSongAsync();
            //}

            //await Task.WhenAll(songAdders).ConfigureAwait(false);


            // ====================== just was playing' with )) ======================

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
            if (SongArchive.Count >= MaxSongs)
            {
                SongArchive.RemoveAt(0);
            }
            SongArchive.Add(CurrentSong);

            CurrentSong = UpcomingSongs.First();

            UpcomingSongs.RemoveAt(0);

            await AddRandomSongAsync().ConfigureAwait(false);

            Update?.Invoke();

            StartPlaying();
        }

        public async Task ChangeSongAsync(long songId)
        {
            await SelectRandomSongAsync();

            for (var i = 0; i < MaxSongs; i++)
            {
                if (UpcomingSongs[i].ID != songId)
                {
                    continue;
                }

                var song = await SelectRandomSongAsync().ConfigureAwait(false);

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

                await AddRandomSongAsync().ConfigureAwait(false);

                UpcomingSongs.RemoveAt(i);
                return;
            }
        }

        /// <summary>
        /// Same as synchronous version but with async calls to EF
        /// </summary>
        public async Task<Song> SelectRandomSongAsync()
        {
            Song song;

            using (var context = new MusCatEntities())
            {
                // find out the maximum song ID in the database
                var maxSid = context.Songs.Max(s => s.ID);

                // keep selecting song randomly until the song file is actually present in the file system...
                // ...and while it isn't present in archive of recently played songs and upcoming songs
                do
                {
                    var songId = _songSelector.Next() % maxSid;

                    song = await context.Songs
                                        .FirstAsync(s => s.ID >= songId)
                                        .ConfigureAwait(false);

                    // include the corresponding album of our song
                    song.Album = await context.Albums
                                              .FirstAsync(a => a.ID == song.AlbumID)
                                              .ConfigureAwait(false);

                    // do the same thing with performer for included album
                    song.Album.Performer = await context.Performers
                                                        .FirstAsync(p => p.ID == song.Album.PerformerID)
                                                        .ConfigureAwait(false);
                }
                while (SongArchive.Any(s => s.ID == song.ID)    // true, if the archive already contains this song
                    || UpcomingSongs.Any(s => s.ID == song.ID)  // true, if it is already in songlist
                    || song.ID == CurrentSong.ID                // true, if it's currently playing
                    || FileLocator.FindSongPath(song) == "");   // true, if the file with this song doesn't exist
            }

            return song;
        }

        #endregion
    }
}
