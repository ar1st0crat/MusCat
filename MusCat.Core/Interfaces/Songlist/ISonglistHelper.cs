using System.Collections.Generic;

namespace MusCat.Core.Interfaces.Songlist
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
}