using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using MusCat.Core.Entities;

namespace MusCat.Infrastructure.Services
{
    /// <summary>
    /// Class providing static functions for locating paths in file system:
    /// 
    /// 1) song files (e.g. "|root|\F\FooPerformer\1975 - BarAlbum\01 - Intro.mp3")
    /// 2) performer photos (e.g. "|root|\F\FooPerformer\Picture\photo.jpeg")
    /// 3) album images (e.g. "|root|\F\FooPerformer\Picture\1762.jpg")
    /// 
    /// </summary>
    public static class FileLocator
    {
        // Pathlist contains root paths where to look for media files
        // Pathlist is taken from file paths.xml
        public static List<string> Pathlist = 
            Directory.GetLogicalDrives().Select(p => p.TrimEnd('\\')).ToList();

        /// <summary>
        /// During initialization FileLocator loads data from file "config\paths.xml".
        /// If MusCat app is launched for the first time or the config file is corrupted,
        /// user is asked to specify paths
        /// </summary>
        public static void LoadConfiguration()
        {
            using (var reader = XmlReader.Create(@"config\paths.xml"))
            {
                Pathlist.Clear();

                while (reader.Read())
                {
                    if (!reader.IsStartElement() || reader.Name != "path")
                    {
                        continue;
                    }

                    if (!reader.Read())
                    {
                        break;
                    }

                    Pathlist.Add(reader.Value);
                }
            }
        }

        /// <summary>
        /// Save pathlist to config file paths.xml
        /// </summary>
        public static void SaveConfiguration()
        {
            Directory.CreateDirectory("config");

            using (var writer = XmlWriter.Create(@"config\paths.xml"))
            {
                writer.WriteStartElement("pathlist");

                foreach (var path in Pathlist)
                {
                    writer.WriteElementString("path", path);
                }

                writer.WriteEndElement();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static bool MustBeConfigured()
        {
            if (File.Exists(@"config\paths.xml"))
            {
                return false;
            }

            Directory.CreateDirectory("config");
            return true;
        }

        /// <summary>
        /// Look for image file with performer's photo
        /// </summary>
        /// <param name="performer">Performer object</param>
        /// <returns>Path to performer's photo file, or null if the photo file could not be found</returns>
        public static string GetPerformerImagePath(Performer performer)
        {
            if (performer == null)
            {
                return "";
            }

            var regex = new Regex(@"(?i)\b(ph|f)oto.(png|jpe?g|gif|bmp)");

            foreach (var rootPath in Pathlist)
            {
                var path = string.Format(@"{0}\{1}\{2}\Picture", 
                                                    rootPath,
                                                    performer.Name[0],
                                                    performer.Name);
                if (!Directory.Exists(path))
                {
                    continue;
                }

                var files = Directory.GetFiles(path, "*", SearchOption.TopDirectoryOnly)
                                     .Where(filename => regex.IsMatch(filename))
                                     .ToList();

                if (files.Count > 0)
                {
                    return files[0];
                }
            }

            return "";
        }

        /// <summary>
        /// Generate the list of possible paths for saving performer's photo, according to FileLocator's pathlist and naming conventions
        /// </summary>
        /// <param name="performer">Performer object</param>
        /// <param name="ext">Performer's photo file extension</param>
        /// <returns>List of possible paths for saving performer's photo</returns>
        public static List<string> MakePerformerImagePathlist(Performer performer, string ext = "jpg")
        {
            var pathlist = new List<string>();

            foreach (var rootPath in Pathlist)
            {
                var path = string.Format(@"{0}\{1}\{2}",
                                                rootPath,
                                                performer.Name[0],
                                                performer.Name);
                if (Directory.Exists(path))
                {
                    pathlist.Add(string.Format(@"{0}\Picture\photo.{1}",
                                                path,
                                                ext));
                    return pathlist;
                }
            }

            pathlist.AddRange(Pathlist.Select(
                rootPath => string.Format(@"{0}\{1}\{2}\Picture\photo.{3}",
                                                rootPath,
                                                performer.Name[0],
                                                performer.Name,
                                                ext)));
            return pathlist;
        }
                
        /// <summary>
        /// Look for image file with album cover image
        /// </summary>
        /// <param name="album">Album object</param>
        /// <returns>Path to album image file, or null if the file could not be found</returns>
        public static string GetAlbumImagePath(Album album)
        {
            if (album == null)
            {
                return "";
            }

            var regex = new Regex(@"\b(?i)" + album.Id + ".(png|jpe?g|gif|bmp)");

            foreach (var rootPath in Pathlist)
            {
                var path = string.Format(@"{0}\{1}\{2}\Picture", 
                                                rootPath, 
                                                album.Performer.Name[0],
                                                album.Performer.Name);
                if (!Directory.Exists(path))
                {
                    continue;
                }

                var files = Directory.GetFiles(path, "*", SearchOption.TopDirectoryOnly)
                                     .Where(filename => regex.IsMatch(filename))
                                     .ToList();

                if (files.Count > 0)
                {
                    return files[0];
                }
            }

            return "";
        }

        /// <summary>
        /// Generate the list of possible paths for saving album cover image, according to FileLocator's pathlist and naming conventions
        /// </summary>
        /// <param name="album">Album object</param>
        /// <param name="ext">Album image file extension</param>
        /// <returns>List of possible paths for saving album cover image</returns>
        public static List<string> MakeAlbumImagePathlist(Album album, string ext = "jpg")
        {
            var pathlist = new List<string>();

            foreach (var rootPath in Pathlist)
            {
                var path = string.Format(@"{0}\{1}\{2}", 
                                                rootPath,
                                                album.Performer.Name[0],
                                                album.Performer.Name);
                if (Directory.Exists(path))
                {
                    pathlist.Add(string.Format(@"{0}\Picture\{1}.{2}",
                                                path, 
                                                album.Id,
                                                ext));
                    return pathlist;
                }
            }

            pathlist.AddRange(Pathlist.Select(
                rootPath => string.Format(@"{0}\{1}\{2}\Picture\{3}.{4}",
                                                rootPath,
                                                album.Performer.Name[0],
                                                album.Performer.Name,
                                                album.Id,
                                                ext)));
            return pathlist;
        }
        
        /// <summary>
        /// A static function for locating a song file in file system
        /// </summary>
        /// <param name="song"></param>
        /// <returns>the actual path of the file with the song if it was found, an empty string otherwise</returns>
        public static string FindSongPath(Song song)
        {
            foreach (var rootpath in Pathlist)
            {
                var performerDirectory = string.Format(@"{0}\{1}\{2}", 
                                                        rootpath,
                                                        song.Album.Performer.Name[0],
                                                        song.Album.Performer.Name);

                if (!Directory.Exists(performerDirectory))
                {
                    continue;
                }

                var albumDirectories = Directory.GetDirectories(performerDirectory)
                                                .Where(d => d.Contains(song.Album.ReleaseYear.ToString()))
                                                .ToList();

                if (!albumDirectories.Any())
                {
                    continue;
                }

                foreach (var dir in albumDirectories)
                {
                    var album = Path.GetFileName(dir);
                    var files = Directory.GetFiles(dir);

                    // Check if album name contains punctuation and eliminate it!
                    // Normalize string - remove punctuation!

                    if (album.NormalizePath().Contains(song.Album.Name.NormalizePath()) &&
                        // !!!check this as well:
                        song.TrackNo > 0 && song.TrackNo <= files.Length)
                    {
                        return files[song.TrackNo - 1];
                    }
                }
            }

            return "";
        }

        /// <summary>
        /// Extension method that normalizes file path strings:
        /// remove all spaces and punctuations, convert each letter to lowercase
        /// 
        /// Note.
        /// Method Path.GetInvalidFileNameChars() returns '.', ':', etc.
        /// Although those are legal path characters often folders do not contain them
        /// so the char.IsPunctuation() method is used here.
        /// 
        /// </summary>
        /// <param name="s">File path string</param>
        /// <returns>Normalized file path string</returns>
        public static string NormalizePath(this string s)
        {
            return s.Where(c => !char.IsPunctuation(c))
                    .Aggregate("", (current, c) => current + char.ToLowerInvariant(c));
        }
    }
}
