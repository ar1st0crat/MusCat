using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MusCat.Core.Entities;
using MusCat.Core.Interfaces.Audio;

namespace MusCat.Core.Interfaces.Radio
{
    public interface IRadioService
    {
        IAudioPlayer Player { get; }

        List<Song> SongArchive { get; }
        List<Song> UpcomingSongs { get; }
        Song CurrentSong { get; }
        Song PrevSong { get; }
        Song NextSong { get; }
        
        void Start();
        void Stop();
        
        Action Update { get; set; }

        //void StartPlaying();
        //void PausePlaying();
        //void ResumePlaying();
        //void StopPlaying();
        //void SetVolume(float volume);
        //AudioPlayer.PlaybackState SongPlaybackState { get; }

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