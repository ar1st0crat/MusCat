using System.Collections.Generic;

namespace MusCat.Core.Interfaces
{
    /// <summary>
    /// ISonglistHelper interface defines methods for:
    /// - extracting the track number, name and duration from song files
    /// - retrieving and formatting song duration
    /// - correcting track names according to conventional rules
    /// </summary>
    public interface ISonglistHelper
    {
        List<SongEntry> Parse(string folder);
        void FixTitles(IList<SongEntry> songs);
        string FixDurations(IList<SongEntry> songs);
    }

    public class SongEntry
    {
        public byte No { get; set; }
        public string Title { get; set; }
        public string Duration { get; set; }
    }
}