using System.Collections.Generic;

namespace MusCat.Core.Interfaces.Tracklist
{
    /// <summary>
    /// ITracklistHelper interface defines methods for:
    /// - extracting the track number, name and duration from song files in particular folder
    /// - correcting and formatting track durations
    /// - correcting track names according to conventional rules
    /// </summary>
    public interface ITracklistHelper
    {
        Track[] Parse(string folder);
        void FixTitles(IList<Track> tracks);
        string FixDurations(IList<Track> tracks);
    }
}