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
        
        void MoveUpcomingSong(int from, int to);

        Task MakeSonglistAsync();
        Task MoveToNextSongAsync();
        Task MoveToPrevSongAsync();
        Task ChangeSongAsync(int songId);
        Task RemoveSongAsync(int songId);
    }
}
