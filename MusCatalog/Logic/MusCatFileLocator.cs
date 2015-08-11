using MusCatalog.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;


namespace MusCatalog
{
    /// <summary>
    /// Class providing static functions for locating paths in file system:
    /// 
    /// 1) song files (e.g. "C:\F\FooPerformer\1975 - BarAlbum\01 - Intro.mp3")
    /// 2) performer photos (e.g. "C:\F\FooPerformer\Picture\photo.jpeg")
    /// 3) album images (e.g. "C:\F\FooPerformer\Picture\1762.jpg")
    /// 
    /// </summary>
    class MusCatFileLocator
    {
        // pathlist contains starting paths where to look for media files
        // pathlist is stored in paths.xml
        public static List<string> pathlist = new List<string>();

        /// <summary>
        /// During initialization MusCatLocator loads data from file "config\paths.xml".
        /// If MusCat app is launched for the first time or the config file is corrupted, user is asked to specify paths
        /// </summary>
        public static void Initialize()
        {
            // open config file paths.xml
            if (!File.Exists(@"config\paths.xml"))
            {
                System.Windows.MessageBox.Show( "Please specify folders for MusCat to look for media files" );

                Directory.CreateDirectory( "config" );
                using (XmlWriter writer = XmlWriter.Create(@"config\paths.xml"))
                {
                    writer.WriteStartElement( "pathlist" );
                    writer.WriteElementString( "path", @"F:" );
                    writer.WriteElementString( "path", @"G:" );
                    writer.WriteElementString( "path", @"G:\Other" );
                    writer.WriteEndElement();
                }
            }

            using (XmlReader reader = XmlReader.Create(@"config\paths.xml"))
            {
                while ( reader.Read() )
                {
                    if ( reader.IsStartElement() && reader.Name == "path" )
                    {
                        if ( !reader.Read() )
                            break;

                        pathlist.Add( reader.Value );
                    }
                }
            }
        }

        /// <summary>
        /// Save pathlist to config file paths.xml
        /// </summary>
        public static void SaveConfigFile()
        {
            // save config file paths.xml
            Directory.CreateDirectory("config");
            using (XmlWriter writer = XmlWriter.Create(@"config\paths.xml"))
            {
                writer.WriteStartElement("pathlist");
                foreach (var path in pathlist)
                {
                    writer.WriteElementString("path", path);
                }
                writer.WriteEndElement();
            }
        }
                
        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public static string GetPathImagePerformer( Performer p )
        {
            if (p == null)
            {
                return "";
            }

            Regex reg = new Regex(@"(?i)(ph|f)oto.(png|jpe?g|gif|bmp)");

            foreach (var startingPath in pathlist)
            {
                string path = string.Format(@"{0}\{1}\{2}\Picture", startingPath, p.Name[0], p.Name);

                if (Directory.Exists(path))
                {
                    var files = Directory.GetFiles(path, "*", SearchOption.TopDirectoryOnly)
                                            .Where(filename => reg.IsMatch(filename))
                                            .ToList();
                    if (files.Count > 0)
                    {
                        return files[0];
                    }
                }
            }

            return "";
        }
                
        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public static string GetPathImageAlbum(Album a)
        {
            if (a == null)
            {
                return "";
            }

            Regex reg = new Regex(@"(?i)" + a.ID + ".(png|jpe?g|gif|bmp)");

            foreach (var startingPath in pathlist)
            {
                string path = string.Format(@"{0}\{1}\{2}\Picture", startingPath, a.Performer.Name[0], a.Performer.Name);

                if (Directory.Exists(path))
                {
                    var files = Directory.GetFiles(path, "*", SearchOption.TopDirectoryOnly)
                                            .Where(filename => reg.IsMatch(filename))
                                            .ToList();
                    if (files.Count > 0)
                    {
                        return files[0];
                    }
                }
            }

            return "";
        }

        /// <summary>
        /// Normalize string: remove all spaces and punctuations, convert each letter to uppercase
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private static string LocatorNormalize(string s)
        {
            string res = "";
            foreach (char c in s)
            {
                if (!Char.IsWhiteSpace(c) && !Char.IsPunctuation(c))
                {
                    res += Char.ToUpperInvariant(c);
                }
            }
            return res;
        }


        /// <summary>
        /// A static function for locating a song file in file system
        /// </summary>
        /// <param name="song"></param>
        /// <returns>the actual path of the file with the song if it was found, an empty string otherwise</returns>
        public static string FindSongPath(Song song)
        {
            foreach (var rootpath in pathlist)
            {
                string pathDir = string.Format( @"{0}\{1}\{2}", rootpath, song.Album.Performer.Name[0], song.Album.Performer.Name );

                if (Directory.Exists(pathDir))
                {
                    string[] dirs = Directory.GetDirectories(pathDir);
                    var neededDirs = dirs.Where( d => d.Contains(song.Album.ReleaseYear.ToString()) );

                    if (neededDirs.Count() > 0)
                    {
                        string songDir = neededDirs.First();
                                                
                        // Check if song name contains punctuation and eliminate it!
                        // Normalize string - remove spaces and punctuation!
                        foreach (string dir in neededDirs)
                        {
                            if ( LocatorNormalize(dir).Contains( LocatorNormalize(song.Album.Name) ) )
                            {
                                songDir = dir;
                                break;
                            }
                        }

                        // TODO:
                        // Check if the album is double (currently we simply return ""
                        // if the album is broken into two folders (CD1 and CD2) and the song is in the second part of the album)
                        // 
                        if (Directory.GetFiles(songDir).Length < song.TrackNo)
                        {
                            return "";
                        }
                        else
                        {
                            return Directory.GetFiles(songDir)[song.TrackNo - 1];
                        }
                    }
                }
            }

            return "";
        }
    }
}
