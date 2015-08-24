using MusCatalog.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MusCatalog.ViewModel
{
    class EditAlbumViewModel
    {
        public Album Album { get; set; }

        public EditAlbumViewModel( Album a )
        {
            Album = a;

            // load and prepare all songs from the album for further actions
            using (var context = new MusCatEntities())
            {
                var albumSongs = context.Songs.Where(s => s.Album.ID == a.ID).ToList();

                foreach (var song in albumSongs)
                {
                    //song.Album = album;
                    Album.Songs.Add( song );
                }
            }
        }
    }
}
