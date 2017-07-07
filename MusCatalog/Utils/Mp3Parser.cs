using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using MusCatalog.Model;
using TagLib;

namespace MusCatalog.Utils
{
    /// <summary>
    /// Class Mp3Parser is responsible for:
    /// - extracting the track number and track name from mp3 files
    /// - retrieving and formatting song duration
    /// </summary>
    class Mp3Parser
    {
        /// <summary>
        /// Method iterates through all mp3 files in given directory,
        /// extracts ID3 tag info from each file
        /// and returns the collection of ready Song objects
        /// </summary>
        /// <param name="folder">The directory where to parse mp3 files</param>
        /// <param name="album">The album containing the songs we are parsing</param>
        /// <param name="songs">The songs filled with information extracted from mp3 files</param>
        public void ParseMp3Collection(string folder, Album album, ObservableCollection<Song> songs)
        {
            var i = 0;
            foreach (var filename in Directory.GetFiles(folder, "*.mp3"))
            {
                if (i == songs.Count)
                {
                    songs.Add(new Song { ID = -1, AlbumID = album.ID });
                }

                using (var file = TagLib.File.Create(filename))
                {
                    var v2Tag = file.GetTag(TagTypes.Id3v2) as TagLib.Id3v2.Tag;

                    if (v2Tag?.Title != null)
                    {
                        songs.ElementAt(i).Name = v2Tag.Title;
                    }
                    else
                    {
                        var v1Tag = file.GetTag(TagTypes.Id3v1) as TagLib.Id3v1.Tag;

                        if (v1Tag?.Title != null)
                        {
                            songs.ElementAt(i).Name = v1Tag.Title;
                        }
                        else
                        {
                            songs.ElementAt(i).Name = Path.GetFileNameWithoutExtension(filename);
                        }
                    }

                    songs.ElementAt(i).TrackNo = (byte) (i + 1);
                    songs.ElementAt(i).TimeLength = file.Properties.Duration.ToString(@"m\:ss");
                }

                i++;
            }
        }

        /// <summary>
        /// Method corrects song titles.
        /// Examples:
        ///     the_title_contains_underscores      =>      The Title Contains Underscores
        ///     Too   many   whitespaces            =>      Too Many Whitespaces
        ///     capitalize First  letter            =>      Capitalize First Letter
        /// </summary>
        /// <param name="songs">Collection of songs</param>
        public void FixNames(ObservableCollection<Song> songs)
        {
            foreach (var song in songs)
            {
                song.Name = song.Name.Trim().Replace("_", " ");
                
                // additionally replace multiple whitespaces (if there are any) with one whitespace
                song.Name = Regex.Replace(song.Name, @"\s{2,}", " ");

                // make each character lowercase
                song.Name = song.Name.ToLower();

                // and then capitalize each first letter of the word
                var oldLetter = song.Name.Substring(0, 1);
                song.Name = song.Name.Remove(0, 1).Insert(0, oldLetter.ToUpper());

                var spacePos = song.Name.IndexOf(' ');
                while (spacePos > -1)
                {
                    oldLetter = song.Name.Substring(spacePos + 1, 1);
                    song.Name = song.Name.Remove(spacePos + 1, 1).Insert(spacePos + 1, oldLetter.ToUpper());
                    spacePos = song.Name.IndexOf(' ', spacePos + 1);
                }
            }
        }

        /// <summary>
        /// Method corrects the duration time of each song 
        /// and computes the total duration time of all songs.
        /// Example:
        ///     song no.1       '3:5'       =>      '3:05'
        ///     song no.2       '2'         =>      '2:00'
        ///     song no.3       'a1:30'     =>      '1:30'
        ///                            and returns: '6:35' (total time)
        /// </summary>
        /// <param name="songs">Collection of songs</param>
        /// <returns>The total duration of songs in format 'm:ss'</returns>
        public string FixTimes(ObservableCollection<Song> songs)
        {
            var totalMinutes = 0;
            var totalSeconds = 0;

            foreach (var s in songs)
            {
                // fix each record if there's a need
                var clean = "";
                foreach (char c in s.TimeLength)
                {
                    if (char.IsDigit(c) || c == ':')
                    {
                        clean += c;
                    }
                }

                var colonPos = clean.IndexOf(':');

                if (colonPos == -1)
                {
                    clean = clean + ":00";
                }
                else if (clean.Length - colonPos <= 2)
                {
                    clean = clean.Insert(colonPos + 1, "0");
                }

                s.TimeLength = clean;
                colonPos = clean.IndexOf(':');

                totalMinutes += int.Parse(s.TimeLength.Substring(0, colonPos));
                totalSeconds += int.Parse(s.TimeLength.Substring(colonPos + 1));
            }

            // calculate total time
            totalMinutes += totalSeconds / 60;
            totalSeconds = totalSeconds % 60;

            return string.Format("{0}:{1:00}", totalMinutes, totalSeconds);
        }
    }
}
