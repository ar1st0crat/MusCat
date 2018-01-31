using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MusCat.Core.Entities;
using MusCat.Core.Interfaces;
using MusCat.Core.Util;

namespace MusCat.Infrastructure.Services
{
    /// <summary>
    /// Radio station service
    /// 
    /// Class provides basic methods for making and playing tracklists 
    /// and their asynchronous analogs
    /// 
    /// </summary>
    public class RadioService : IRadioService
    {
        public const int MaxSongs = 10;

        // Audio player
        public IAudioPlayer Player { get; }
        private bool _isStopped;

        // Delegate that will be invoked when a new song starts playing
        public Action Update { get; set; }

        // Random song selector
        private readonly ISongSelector _songSelector;

        // Collection of recently played songs
        public List<Song> SongArchive { get; } = new List<Song>();

        // Collection of upcoming songs
        public List<Song> UpcomingSongs { get; } = new List<Song>();

        public Song CurrentSong { get; private set; } = new Song();
        public Song PrevSong => SongArchive.LastOrDefault();
        public Song NextSong => UpcomingSongs.FirstOrDefault();


        public RadioService(IAudioPlayer player, ISongSelector songSelector)
        {
            Guard.AgainstNull(player);
            Guard.AgainstNull(songSelector);

            Player = player;
            _songSelector = songSelector;
        }

        #region playback functions

        public void Start()
        {
            PlayCurrentSong();

            // There's one general task associated with the radio
            // whose purpose is to play whatever active song in the background thread
            Task.Run(async () =>
            {
                while (!_isStopped)
                {
                    await Task.Delay(1000);

                    if (Player.IsStopped && !Player.IsStoppedManually)
                    {
                        await MoveToNextSongAsync();
                    }
                }
            });
        }

        public void Stop()
        {
            _isStopped = true;
            Player.Close();
        }

        public void PlayCurrentSong()
        {
            if (Player.SongPlaybackState != PlaybackState.Stop)
            {
                Player.Stop();
            }

            var fileSong = FileLocator.FindSongPath(CurrentSong);

            try
            {
                Player.Play(fileSong);
            }
            catch (InvalidOperationException)
            {
                // some multi-threading issue in debug mode
            }
            catch (Exception)
            {
                MoveToNextSong();
            }
        }

        #endregion


        #region synchronous operations

        public void AddToArchive() => SongArchive.Add(CurrentSong);

        public void AddRandomSong() => UpcomingSongs.Add(SelectRandomSong() ?? CurrentSong);

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
            Player.Stop();

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

            PlayCurrentSong();
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

            PlayCurrentSong();
        }
        
        /// <summary>
        /// Method just replaces song (addressed by its ID) with the new randomly selected song 
        /// </summary>
        public void ChangeSong(long songId)
        {
            for (var i = 0; i < MaxSongs; i++)
            {
                if (UpcomingSongs[i].Id != songId)
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
                if (UpcomingSongs[i].Id != songId)
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
        private Song SelectRandomSong(int maxAttempts = 15)
        {
            return SelectRandomSongAsync(maxAttempts).Result;
        }

        #endregion


        #region asynchronous operations 

        public async Task AddRandomSongAsync()
        {
            var song = await SelectRandomSongAsync().ConfigureAwait(false);

            // if for some reason could not find new song to play
            // then just add currently playing track to upcoming songs
            UpcomingSongs.Add(song ?? CurrentSong);
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
            PlayCurrentSong();
        }

        public async Task ChangeSongAsync(long songId)
        {
            await SelectRandomSongAsync();

            for (var i = 0; i < MaxSongs; i++)
            {
                if (UpcomingSongs[i].Id != songId)
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
                if (UpcomingSongs[i].Id != songId)
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
        private async Task<Song> SelectRandomSongAsync(int maxAttempts = 15)
        {
            var attempts = 0;

            Song song;

            // keep selecting song randomly until the song file is actually present in the file system...
            // ...and while it isn't present in archive of recently played songs and upcoming songs
            do
            {
                if (attempts++ == maxAttempts) return null;
                song = await _songSelector.SelectSongAsync();
            }
            while (SongArchive.Any(s => s.Id == song.Id)    // true, if the archive already contains this song
                || UpcomingSongs.Any(s => s.Id == song.Id)  // true, if it is already in songlist
                || song.Id == CurrentSong.Id                // true, if it's currently playing
                || FileLocator.FindSongPath(song) == "");   // true, if the file with this song doesn't exist

            return song;
        }

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
        }

        /* Alternative code
         * 
         * ================= (however, it allows duplicate songs) ===================
         *
         * //var songAdders = new Task[MaxSongs];
         *
         * //// just fire them all at once (order doesn't matter)
         * //for (var i = 0; i < MaxSongs; i++)
         * //{
         * //    songAdders[i] = AddRandomSongAsync();
         * //}
         *
         * //await Task.WhenAll(songAdders).ConfigureAwait(false);
         *
         * // ====================== just was playing' with )) ======================
         *
         * // Parallel.For(0, MaxSongs, i => AddRandomSong());
         * // Parallel.For(0, MaxSongs, i => AddRandomSongAsync().RunSynchronously());
         */

        #endregion
    }
}
