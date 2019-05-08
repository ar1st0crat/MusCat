using System.Collections.Generic;
using System.Threading.Tasks;
using MusCat.Core.Entities;

namespace MusCat.Core.Interfaces.Radio
{
    public interface IRadioService
    {
        List<Song> SongArchive { get; }
        List<Song> UpcomingSongs { get; }
        Song CurrentSong { get; }
        Song PrevSong { get; }
        Song NextSong { get; }

        void MakeSonglist();
        void MoveToNextSong();
        void MoveToPrevSong();
        void ChangeSong(int songId);
        void RemoveSong(int songId);

        Task MakeSonglistAsync();
        Task MoveToNextSongAsync();
        Task MoveToPrevSongAsync();
        Task ChangeSongAsync(int songId);
        Task RemoveSongAsync(int songId);

        void MoveUpcomingSong(int from, int to);
    }
}