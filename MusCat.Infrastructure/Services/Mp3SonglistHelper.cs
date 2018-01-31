using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MusCat.Core.Interfaces;
using MusCat.Infrastructure.Services.Util;
using TagLib;

namespace MusCat.Infrastructure.Services
{
    /// <summary>
    /// Class Mp3SonglistHelper is responsible for:
    /// - extracting the track number and track name from mp3 files
    /// - retrieving and formatting song duration
    /// - correcting track names according to conventional rules
    /// </summary>
    public class Mp3SonglistHelper : ISonglistHelper
    {
        /// <summary>
        /// Method iterates through all mp3 files in given directory,
        /// extracts ID3 tag info from each file
        /// and returns the collection of SongEntry objects
        /// </summary>
        /// <param name="folder">The directory where to parse mp3 files</param>
        /// <returns>List of song titles and durations extracted from song files</returns>
        public List<SongEntry> Parse(string folder)
        {
            var songs = new List<SongEntry>();

            var i = 1;
            foreach (var filename in Directory.GetFiles(folder, "*.mp3"))
            {
                using (var file = TagLib.File.Create(filename))
                {
                    var song = new SongEntry();

                    var v2Tag = file.GetTag(TagTypes.Id3v2) as TagLib.Id3v2.Tag;

                    if (v2Tag?.Title != null)
                    {
                        song.Title = v2Tag.Title;
                    }
                    else
                    {
                        var v1Tag = file.GetTag(TagTypes.Id3v1) as TagLib.Id3v1.Tag;

                        if (v1Tag?.Title != null)
                        {
                            song.Title = v1Tag.Title;
                        }
                        else
                        {
                            song.Title = Path.GetFileNameWithoutExtension(filename);
                        }
                    }

                    song.No = (byte)(i++);
                    song.Duration = file.Properties.Duration.ToString(@"m\:ss");

                    songs.Add(song);
                }
            }

            return songs;
        }

        /// <summary>
        /// Method corrects song titles.
        /// Algorithm was simpler in the first place
        /// however it did not take all nuances into account.
        /// 
        /// Currently, it preserves all punctuations and reorganizes them
        /// so that the resulting text looks nice.
        /// 
        /// PS. Don't ask me why I didn't want to use regexes here )))
        /// 
        /// Examples:
        ///     the_title_contains_underscores      =>      The Title Contains Underscores
        ///     Too   many   whitespaces            =>      Too Many Whitespaces
        ///     capitalize First  letter            =>      Capitalize First Letter
        /// 
        /// More:
        ///     "  Well   how     aRE  you?  "          =>      "Well How Are You?"
        ///     "  Well ,  song(yeAh ?yeah!)  "         =>      "Well, Song (Yeah? Yeah!)"
        ///     "wow...  it(is so cool...)  "           =>      "Wow... It (Is So Cool...)"
        ///     "   (There's   been)  some, right?))  " =>      "(There's Been) Some, Right?))"
        ///     "Hush! it's    ( \"an\" ( ! ) ) experiment.. ."    
        ///                                              =>      "Hush! It's (\"An\" (!)) Experiment..."
        /// 
        /// </summary>
        /// <param name="songs">Collection of songs</param>
        public void FixTitles(IList<SongEntry> songs)
        {
            var punctuation = new[] { '.', ',', '?', '!', ':', ';', '(', ')', '/', '\\'};//, '"' };

            byte trackNo = 1;

            foreach (var song in songs)
            {
                song.No = trackNo++;

                // first, trim string

                var title = song.Title.Replace("_", " ").Trim();

                if (title.Length == 0)
                {
                    continue;
                }
                
                // remove all odd spaces but leave spaces between letters! (including '\'', '-' and '&')
                
                title = string.Join(" ", title.Split(new[] {"  "}, StringSplitOptions.RemoveEmptyEntries));

                for (var i = 1; i < title.Length - 1; i++)
                {
                    if (title.SpaceSurroundedByLetters(i))
                    {
                        title = title.Remove(i--, 1);
                    }
                }

                title = punctuation.Aggregate(title, (current, p) => current.Replace(p.ToString(), p + " "));

                // capitalize each word

                var parts = title.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
                for (var i = 0; i < parts.Length; i++)
                {
                    parts[i] = parts[i].Capitalize();
                }

                title = string.Join(" ", parts);

                // subtle postprocessing of parentheses and slashes    %-o
                title = title.Replace("( ", " (")
                             .Replace("/", " /")
                             .Replace("\\", " \\")
                             .Trim();

                // final cleanup
                for (var i = 1; i < title.Length - 1; i++)
                {
                    if (title[i] == ' ' &&
                        punctuation.Contains(title[i - 1]) && 
                        punctuation.Contains(title[i + 1]))
                    {
                        title = title.Remove(i--, 1);
                    }
                }

                song.Title = title;
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
        public string FixDurations(IList<SongEntry> songs)
        {
            var totalMinutes = 0;
            var totalSeconds = 0;

            foreach (var song in songs)
            {
                // fix each record if there's a need

                var parts = song.Duration.Split(':')
                                .Select(s => "0" + s)
                                .ToArray();

                var minutes = int.Parse(parts[0].Digits());
                var seconds = 0;

                if (parts.Count() > 1)
                {
                    seconds = int.Parse(parts[1].Digits());
                }
                
                song.Duration = string.Format("{0}:{1:00}", minutes, seconds);

                totalMinutes += minutes;
                totalSeconds += seconds;
            }

            // calculate total time
            totalMinutes += totalSeconds / 60;
            totalSeconds = totalSeconds % 60;

            return string.Format("{0}:{1:00}", totalMinutes, totalSeconds);
        }
    }
}
