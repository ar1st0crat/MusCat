using MusCatalog.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;


namespace MusCatalog
{
    class MusCatFileLocator
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        string postProcess(string s)
        {
            string res = "";
            foreach (char c in s)
                if (!Char.IsWhiteSpace(c) && !Char.IsPunctuation(c))
                    res += c;

            return res;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="song"></param>
        /// <returns>the actual path of the file with the song if it was found, an empty string otherwise</returns>
        public static string FindSongPath(Songs song)
        {
            List<string> pathlist = new List<string>();
            pathlist.Add(@"F:\");
            pathlist.Add(@"G:\");
            pathlist.Add(@"G:\Other\");

            foreach (var rootpath in pathlist)
            {
                string pathDir = rootpath +
                                    song.Albums.Performers.Performer[0] +
                                    System.IO.Path.DirectorySeparatorChar +
                                    song.Albums.Performers.Performer +
                                    System.IO.Path.DirectorySeparatorChar;

                if (Directory.Exists(pathDir))
                {
                    // TODO:
                    // 1) check if song name contains punctuation and eliminate it! Normalize string - remove spaces and punctuation!
                    // 2) check if the album is double!

                    string[] dirs = Directory.GetDirectories(pathDir);
                    var neededDirs = dirs.Where(d => d.Contains(song.Albums.AYear.ToString()));// && postProcess(song.Albums.Album) == postProcess(d));

                    //if (neededDirs.Count() > 0)
                    //{
                    //    if (song.Albums.Songs.Count(s => s.SID == song.SID) > 1)
                    //        return neededDirs.AsEnumerable().ElementAt(1);
                    //    else
                    //        return neededDirs.First();
                    //}

                    if (Directory.GetFiles(neededDirs.First()).Length < song.SN)
                        return "";
                    else
                        return Directory.GetFiles(neededDirs.First())[song.SN - 1];
                }
            }

            return "";
        }
    }
}
