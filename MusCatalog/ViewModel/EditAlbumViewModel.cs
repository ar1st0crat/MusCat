using Microsoft.Win32;
using MusCatalog.Model;
using MusCatalog.View;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;

namespace MusCatalog.ViewModel
{
    class EditAlbumViewModel : INotifyPropertyChanged
    {
        public AlbumViewModel AlbumView { get; set; }
        public Album Album
        {
            get { return AlbumView.Album; }
            set
            {
                AlbumView.Album = value;
                RaisePropertyChanged("Album");
            }
        }
        public string AlbumTotalTime
        {
            get { return Album.TotalTime; }
            set
            {
                Album.TotalTime = value;
                RaisePropertyChanged("AlbumTotalTime");
            }
        }

        public ObservableCollection<Song> Songs
        {
            get { return AlbumView.Songs; }
        }
        public Song SelectedSong { get; set; }

        // bitmaps for stars
        private static BitmapImage imageStar = App.Current.TryFindResource("ImageStar") as BitmapImage;
        private static BitmapImage imageHalfStar = App.Current.TryFindResource("ImageHalfStar") as BitmapImage;
        private static BitmapImage imageEmptyStar = App.Current.TryFindResource("ImageEmptyStar") as BitmapImage;


        public EditAlbumViewModel( AlbumViewModel viewmodel )
        {
            AlbumView = viewmodel;
        }

        public void ParseMp3()
        {
            var fbd = new System.Windows.Forms.FolderBrowserDialog();
            if (fbd.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                return;
            }

            Mp3Parser parser = new Mp3Parser();
            parser.ParseMp3Collection( fbd.SelectedPath, Album, Songs );
            AlbumTotalTime = parser.FixTimes( Songs );
        }

        public void SaveSong()
        {
            if (SelectedSong != null)
            {
                using (var context = new MusCatEntities())
                {
                    context.Entry(context.Songs.Find(SelectedSong.ID)).CurrentValues.SetValues(SelectedSong);
                    context.SaveChanges();
                }
            }
        }

        public void DeleteSong()
        {
            if (MessageBox.Show(string.Format("Are you sure you want to delete the song\n'{0}'\nby '{1}'?",
                                            SelectedSong.Name, SelectedSong.Album.Performer.Name),
                                            "Confirmation",
                                            MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                using (var context = new MusCatEntities())
                {
                    context.Songs.Remove(SelectedSong);
                    context.SaveChanges();
                }
            }
        }

        public void AddSong()
        {
            byte newTrackNo = (byte)(Songs.Last().TrackNo + 1);
            Songs.Add(new Song { ID = -1, TrackNo = newTrackNo, AlbumID = Album.ID });
        }

        public void SaveAlbumInformation()
        {
            using (var context = new MusCatEntities())
            {
                context.Entry(Album).State = System.Data.EntityState.Modified;
                context.SaveChanges();
            }
        }

        public void FixNames()
        {
            Mp3Parser parser = new Mp3Parser();
            parser.FixNames(Songs);
        }

        public void FixTimes()
        {
            Mp3Parser parser = new Mp3Parser();
            AlbumTotalTime = parser.FixTimes( Songs );
        }

        public void ClearAll()
        {
            if (MessageBox.Show(string.Format("Are you sure you want to delete all songs in the album\n '{0}' \nby '{1}'?",
                                            Album.Name, Album.Performer.Name),
                                            "Confirmation",
                                            MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                Songs.Clear();
            }
        }

        public void SaveAll()
        {
            using (var context = new MusCatEntities())
            {
                foreach (var song in Songs)
                {
                    if (song.ID == -1)
                    {
                        song.ID = context.Songs.Max(s => s.ID) + 1;
                        context.Songs.Add(song);
                    }
                    else
                    {
                        context.Entry(context.Songs.Find(song.ID)).CurrentValues.SetValues(song);
                    }
                    context.SaveChanges();
                }
            }
        }

        public string ChooseImageSavePath()
        {
            var filepaths = FileLocator.MakePathImageAlbum(Album);

            if (filepaths.Count > 1)
            {
                ChoiceWindow choice = new ChoiceWindow();
                choice.SetChoiceList(filepaths);
                choice.ShowDialog();

                return choice.ChoiceResult;
            }

            return filepaths[0];
        }

        public void PrepareFileForSaving(string filepath)
        {
            // ensure that target directory exists
            Directory.CreateDirectory(Path.GetDirectoryName(filepath));

            // first check if file already exists
            if (File.Exists(filepath))
            {
                File.Delete(filepath);
            }
        }

        public void LoadAlbumImageFromClipboard()
        {
            if (!Clipboard.ContainsImage())
            {
                MessageBox.Show("No image in clipboard!");
                return;
            }

            string filepath = ChooseImageSavePath();
            if (filepath == null)
            {
                return;
            }

            var image = Clipboard.GetImage();
            try
            {
                PrepareFileForSaving(filepath);

                using (var fileStream = new FileStream(filepath, FileMode.CreateNew))
                {
                    BitmapEncoder encoder = new JpegBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(image));
                    encoder.Save(fileStream);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            RaisePropertyChanged("Album");
            AlbumView.RaisePropertyChanged("Album");
        }

        public void LoadAlbumImageFromFile()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            var result = ofd.ShowDialog();
            if (result.HasValue && result.Value == true)
            {
                string filepath = ChooseImageSavePath();
                if (filepath == null)
                {
                    return;
                }

                try
                {
                    PrepareFileForSaving(filepath);
                    File.Copy(ofd.FileName, filepath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

                RaisePropertyChanged("Album");
                AlbumView.RaisePropertyChanged("Album");
            }
        }

        #region INotifyPropertyChanged event and method

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }
}
