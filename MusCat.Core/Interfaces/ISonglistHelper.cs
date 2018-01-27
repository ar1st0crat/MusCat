using System.Collections.Generic;

namespace MusCat.Core.Interfaces
{
    /// <summary>
    /// ISonglistHelper interface defines methods for:
    /// - extracting the track number and track name from song files
    /// - retrieving and formatting song duration
    /// - correcting track names according to conventional rules
    /// </summary>
    public interface ISonglistHelper
    {
        /// <summary>
        /// Method iterates through all song files in given directory,
        /// parses each file
        /// and returns the collection of ready SongEntry objects
        /// </summary>
        /// <param name="folder">The directory where to parse song files</param>
        /// <returns>Songs with titles and durations extracted from song files</returns>
        List<SongEntry> Parse(string folder);

        /// <summary>
        /// Method corrects song titles.
        /// </summary>
        /// <param name="songs">Collection of song titles</param>
        void FixTitles(IList<SongEntry> songs);

        /// <summary>
        /// Method corrects the duration time of each song 
        /// and computes the total duration time of all songs.
        /// </summary>
        /// <param name="songs">Collection of song durations</param>
        /// <returns>The total duration of songs in format 'm:ss'</returns>
        string FixDurations(IList<SongEntry> songs);
    }

    public class SongEntry
    {
        public byte No { get; set; }
        public string Title { get; set; }
        public string Duration { get; set; }
    }
}