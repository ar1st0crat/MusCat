using MusCatalog.Model;
using System.Linq;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;

namespace MusCatalog
{
    /// <summary>
    /// Class Mp3Parser is responsible for:
    /// - extracting th etrack number and track nme from mp3 files
    /// - retrieving and formatting song duration
    /// </summary>
    class Mp3Parser
    {
        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="foldername"></param>
        /// <param name="album"></param>
        /// <param name="Songs"></param>
        public void ParseMp3Collection( string foldername, Album album, ObservableCollection<Song> Songs )
        {
            var i = 0;
            foreach (var filename in Directory.GetFiles(foldername, "*.mp3"))
            {
                if (i == Songs.Count)
                {
                    Songs.Add(new Song { ID = -1, AlbumID = album.ID });
                }

                var file = TagLib.File.Create(filename);
                var v2tag = (TagLib.Id3v2.Tag)file.GetTag(TagLib.TagTypes.Id3v2);

                if (v2tag != null)
                {
                    Songs.ElementAt(i).Name = v2tag.Title;
                }
                else
                {
                    TagLib.Id3v1.Tag v1tag;
                    v1tag = (TagLib.Id3v1.Tag)file.GetTag(TagLib.TagTypes.Id3v1);
                    if (v1tag != null)
                    {
                        Songs.ElementAt(i).Name = v1tag.Title;
                    }
                    else
                    {
                        Songs.ElementAt(i).Name = Path.GetFileNameWithoutExtension(filename);
                    }
                }

                Songs.ElementAt(i).TrackNo = (byte)(i + 1);
                Songs.ElementAt(i).TimeLength = file.Properties.Duration.ToString(@"m\:ss");

                file.Dispose();

                i++;
            }
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="Songs"></param>
        public void FixNames(ObservableCollection<Song> Songs)
        {
            foreach (var s in Songs)
            {
                s.Name = s.Name.Trim();
                s.Name = s.Name.Replace("_", " ");
                
                // additionally replace multiple whitespaces (if there are any) with one whitespace
                s.Name = Regex.Replace(s.Name, @"\s{2,}", " ");

                // "lower" each character
                s.Name = s.Name.ToLower();

                // and then "upper" each first letter of the word
                string oldLetter = s.Name.Substring(0, 1);
                s.Name = s.Name.Remove(0, 1).Insert(0, oldLetter.ToUpper());

                int spacePos = s.Name.IndexOf(' ');
                while (spacePos > -1)
                {
                    oldLetter = s.Name.Substring(spacePos + 1, 1);
                    s.Name = s.Name.Remove(spacePos + 1, 1).Insert(spacePos + 1, oldLetter.ToUpper());
                    spacePos = s.Name.IndexOf(' ', spacePos + 1);
                }
            }
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="Songs"></param>
        /// <returns></returns>
        public string FixTimes(ObservableCollection<Song> Songs)
        {
            int totalMinutes = 0, totalSeconds = 0;

            foreach (var s in Songs)
            {
                // fix each record if there's a need
                string clean = "";
                foreach (char c in s.TimeLength)
                {
                    if (char.IsDigit(c) || c == ':')
                        clean += c;
                }

                int colonPos = clean.IndexOf(':');

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
