using System.Collections.ObjectModel;
using MusCat.Entities;

namespace MusCat.Services.Tracklist
{
    /// <summary>
    /// ITracklistParser interface defines methods for:
    /// - extracting the track number and track name from mp3 files
    /// - retrieving and formatting song duration
    /// - correcting track names according to conventional rules
    /// </summary>
    interface ITracklistParser
    {
        /// <summary>
        /// Method iterates through all song files in given directory,
        /// parses each file
        /// and returns the collection of ready Song objects
        /// </summary>
        /// <param name="folder">The directory where to parse song files</param>
        /// <param name="album">The album containing the songs to parse</param>
        /// <param name="songs">The songs filled with information extracted from song files</param>
        void ParseMp3Collection(string folder, Album album, ObservableCollection<Song> songs);

        /// <summary>
        /// Method corrects song titles.
        /// </summary>
        /// <param name="songs">Collection of songs</param>
        void FixNames(ObservableCollection<Song> songs);

        /// <summary>
        /// Method corrects the duration time of each song 
        /// and computes the total duration time of all songs.
        /// </summary>
        /// <param name="songs">Collection of songs</param>
        /// <returns>The total duration of songs in format 'm:ss'</returns>
        string FixTimes(ObservableCollection<Song> songs);
    }
}