using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MusCat.Entities;

namespace MusCat.Services.Radio
{
    interface IRadioService
    {
        List<Song> SongArchive { get; }
        List<Song> UpcomingSongs { get; }
        Song CurrentSong { get; }
        Song PrevSong { get; }
        Song NextSong { get; }
        
        void Start();
        void Stop();

        void StartPlaying();
        void PausePlaying();
        void ResumePlaying();
        void StopPlaying();
        void SetVolume(float volume);
        
        Action Update { get; set; }
        AudioPlayer.PlaybackState SongPlaybackState { get; }

        void MakeSonglist();
        void MoveToNextSong();
        void MoveToPrevSong();
        void ChangeSong(long songId);
        void RemoveSong(long songId);

        Task MakeSonglistAsync();
        Task MoveToNextSongAsync();
        Task ChangeSongAsync(long songId);
        Task RemoveSongAsync(long songId);
    }
}