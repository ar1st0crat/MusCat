using MusCatalog.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;


namespace MusCatalog
{
    /// <summary>
    /// Class providing a static function for locating a song file in file system
    /// </summary>
    class MusCatFileLocator
    {
        public static List<string> pathlist = new List<string>();

        public static void Initialize()
        {
            pathlist.Add( @"F:\" );
            pathlist.Add( @"G:\" );
            pathlist.Add( @"G:\Other\" );
        }

        /// <summary>
        /// Normalize string: remove all spaces and punctuations, convert each letter to uppercase
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private static string postProcess(string s)
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
        public static string FindSongPath(Songs song)
        {
            foreach (var rootpath in pathlist)
            {
                string pathDir = rootpath +
                                    song.Albums.Performers.Performer[0] +
                                    System.IO.Path.DirectorySeparatorChar +
                                    song.Albums.Performers.Performer +
                                    System.IO.Path.DirectorySeparatorChar;

                if (Directory.Exists(pathDir))
                {
                    string[] dirs = Directory.GetDirectories(pathDir);
                    var neededDirs = dirs.Where( d => d.Contains(song.Albums.AYear.ToString()) );

                    if (neededDirs.Count() > 0)
                    {
                        string songDir = neededDirs.First();
                                                
                        // Check if song name contains punctuation and eliminate it!
                        // Normalize string - remove spaces and punctuation!
                        foreach (string dir in neededDirs)
                        {
                            if ( postProcess(dir).Contains(postProcess(song.Albums.Album)) )
                            {
                                songDir = dir;
                                break;
                            }
                        }

                        // TODO:
                        // Check if the album is double (currently we simply return ""
                        // if the album is broken into two folders (CD1 and CD2) and the song is in the second part of the album)
                        // 
                        if (Directory.GetFiles(songDir).Length < song.SN)
                        {
                            return "";
                        }
                        else
                        {
                            return Directory.GetFiles(songDir)[song.SN - 1];
                        }
                    }
                }
            }

            return "";
        }
    }
}
