using MusCatalog.Model;
using NAudio.Wave;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Data;

namespace MusCatalog
{
    class Radio
    {
        // the list of recently played songs
        private ObservableCollection<Song> songlist = new ObservableCollection<Song>();
        private int nCurrentSong = 0;

        // the number of recently played songs which we're tracking
        public const int MAX_SONGS_IN_ARCHIVE = 25;

        // audio player
        private AudioPlayer player = new AudioPlayer();
        public AudioPlayer Player
        {
            get { return player; }
        }

        public ObservableCollection<Song> SongArchive
        {
            get { return songlist; }
        }

        public Song CurrentSong()
        {
            if (nCurrentSong >= 0 && nCurrentSong < songlist.Count)
            {
                return songlist[nCurrentSong];
            }
            else
            {
                return null;
            }
        }

        public Song PrevSong()
        {
            if (nCurrentSong > 0)
            {
                return songlist[nCurrentSong - 1];
            }
            else
            {
                return null;
            }
        }

        public Song NextSong()
        {
            if (nCurrentSong < songlist.Count - 1)
            {
                return songlist[nCurrentSong + 1];
            }
            else
            {
                return null;
            }
        }

        private readonly object songlistLock = new object();

        public Radio()
        {
            BindingOperations.EnableCollectionSynchronization(songlist, songlistLock);
        }

        public void AddSong()
        {
            songlist.Add( SelectRandomSong() );
        }

        /// <summary>
        /// if the current song isn't the first one in a songlist
        /// we can safely decrease the current position in the songlist
        /// </summary>
        public void MoveToPrevSong()
        {
            if (nCurrentSong > 0)
            {
                nCurrentSong--;
            }
        }

        public void MoveToNextSong()
        {
            // check if the archive ("songlist story") is full 
            if ( songlist.Count >= MAX_SONGS_IN_ARCHIVE)
            {
                songlist.RemoveAt(0);       // if we remove the first element then we don't have to increase nCurrentSong
            }
            else
            {
                nCurrentSong++;             // otherwise - increase nCurrentSong
            }

            // if the next song is the last one in the playlist
            // then we should select new upcoming song and add it to the playlist
            if (nCurrentSong == songlist.Count - 1)
            {
                AddSong();
            }
        }

        public void PlayCurrentSong( EventHandler<StoppedEventArgs> SongStoppedHandler )
        {
            string fileSong = FileLocator.FindSongPath( songlist[nCurrentSong] );

            try
            {
                player.Play(fileSong, SongStoppedHandler);
            }
            catch (Exception)
            {
                songlist.RemoveAt(nCurrentSong--);

                // if the next song is the last one in the playlist
                // then we should select new upcoming song and add it to the playlist
                if (nCurrentSong == songlist.Count - 1)
                {
                    AddSong();
                }

                MoveToNextSong();
                PlayCurrentSong( SongStoppedHandler );
            }
        }

        /// <summary>
        /// The method selects random song from a local database.
        /// The song is guaranteed to be present in user's file system
        /// </summary>
        /// <returns>Songs object selected randomly from the database</returns>
        public Song SelectRandomSong( bool bOnlyShortSongs=false )
        {
            Song song = null;

            using (var context = new MusCatEntities())
            {
                Random songSelector = new Random();

                // find out the maximum song ID in the database
                var maxSID = context.Songs.Max(s => s.ID);

                // we keep select song randomly until the song file is actually present in our file system...
                // ...and while it isn't present in our archive of recently played songs
                do
                {
                    IQueryable<Song> selectedsongs;
                    do
                    {
                        // generate random song ID
                        var songNo = songSelector.Next() % maxSID;

                        // the problem here is that our generated ID isn't necessarily present in the database
                        // however there will be at least one song with songID that is greater or equal than this ID
                        selectedsongs = (from s in context.Songs
                                         where s.ID >= songNo
                                         select s).Take(1);

                        // if the filter "Short songs is 'on'" we do additional filtering
                        if ( bOnlyShortSongs )
                        //if (this.ShortSongs.IsChecked.Value)
                            selectedsongs = selectedsongs.Where(
                                    s => s.TimeLength.Length <= 4 && s.TimeLength.CompareTo("1:30") < 0);
                    }
                    while (selectedsongs.Count() < 1);


                    // select the first song from the set of selected songs
                    song = selectedsongs.First();

                    // include the corresponding album of our song
                    song.Album = (from a in context.Albums
                                  where a.ID == song.AlbumID
                                  select a).First();

                    // do the same thing with performer for included album
                    song.Album.Performer = (from p in context.Performers
                                            where p.ID == song.Album.PerformerID
                                            select p).First();
                }
                while (songlist.Where(s => s.ID == song.ID).Count() > 0      // true, if the archive already contains this song
                    || FileLocator.FindSongPath(song) == "");             // true, if the file with this song doesn't exist
            }

            return song;
        }
    }
}
