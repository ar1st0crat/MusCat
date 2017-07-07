using Microsoft.Win32;
using MusCatalog.Model;
using MusCatalog.Utils;
using MusCatalog.View;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Data.Entity;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media.Imaging;

namespace MusCatalog.ViewModel
{
    class EditAlbumViewModel : INotifyPropertyChanged, IDataErrorInfo
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
        public string AlbumName
        {
            get { return Album.Name; }
            set
            {
                Album.Name = value;
                RaisePropertyChanged("AlbumName");
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

        public ObservableCollection<Song> Songs => AlbumView.Songs;
        public Song SelectedSong { get; set; }

        public ObservableCollection<string> ReleaseYearsCollection { get; set; }

        private const int StartingYear = 1900;
        private const int EndingYear = 2050;

        #region Commands

        public RelayCommand ParseMp3Command { get; private set; }
        public RelayCommand FixNamesCommand { get; private set; }
        public RelayCommand FixTimesCommand { get; private set; }
        public RelayCommand ClearAllSongsCommand { get; private set; }
        public RelayCommand SaveAllSongsCommand { get; private set; }
        public RelayCommand AddSongCommand { get; private set; }
        public RelayCommand SaveSongCommand { get; private set; }
        public RelayCommand DeleteSongCommand { get; private set; }
        public RelayCommand SaveAlbumInformationCommand { get; private set; }
        public RelayCommand LoadAlbumImageFromFileCommand { get; private set; }
        public RelayCommand LoadAlbumImageFromClipboardCommand { get; private set; }

        #endregion

        public EditAlbumViewModel(AlbumViewModel viewmodel)
        {
            AlbumView = viewmodel;

            // setting up commands
            ParseMp3Command = new RelayCommand(ParseMp3);
            FixNamesCommand = new RelayCommand(FixNames);
            FixTimesCommand = new RelayCommand(FixTimes);
            ClearAllSongsCommand = new RelayCommand(ClearAll);
            SaveAllSongsCommand = new RelayCommand(SaveAll);
            AddSongCommand = new RelayCommand(AddSong);
            SaveSongCommand = new RelayCommand(SaveSong);
            DeleteSongCommand = new RelayCommand(DeleteSong);
            SaveAlbumInformationCommand = new RelayCommand(SaveAlbumInformation);
            LoadAlbumImageFromFileCommand = new RelayCommand(LoadAlbumImageFromFile);
            LoadAlbumImageFromClipboardCommand = new RelayCommand(LoadAlbumImageFromClipboard);

            // fill combobox with release years
            ReleaseYearsCollection = new ObservableCollection<string>();
            for (var i = StartingYear; i < EndingYear; i++)
            {
                ReleaseYearsCollection.Add(i.ToString());
            }
        }

        public void ParseMp3()
        {
            var fbd = new System.Windows.Forms.FolderBrowserDialog();
            if (fbd.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                return;
            }

            var parser = new Mp3Parser();
            parser.ParseMp3Collection(fbd.SelectedPath, Album, Songs);
            AlbumTotalTime = parser.FixTimes(Songs);
        }

        public void SaveSong()
        {
            if (SelectedSong == null)
            {
                return;
            }

            if (SelectedSong.Error != "")
            {
                MessageBox.Show("Invalid data!");
                return;
            }

            using (var context = new MusCatEntities())
            {
                context.Entry(context.Songs.Find(SelectedSong.ID))
                       .CurrentValues
                       .SetValues(SelectedSong);
                context.SaveChanges();
            }
        }

        public void DeleteSong()
        {
            if (MessageBox.Show(string.Format("Are you sure you want to delete the song\n'{0}'\nby '{1}'?",
                                    SelectedSong.Name, SelectedSong.Album.Performer.Name),
                                    "Confirmation",
                                    MessageBoxButton.YesNo) != MessageBoxResult.Yes)
            {
                return;
            }

            using (var context = new MusCatEntities())
            {
                context.Songs.Remove(context.Songs.SingleOrDefault(x => x.ID == SelectedSong.ID));
                context.SaveChanges();

                Songs.Remove(SelectedSong);
            }
        }

        public void AddSong()
        {
            byte newTrackNo = 1;

            if (Songs.Count > 0)
            {
                newTrackNo = (byte)(Songs.LastOrDefault().TrackNo + 1);
            }

            Songs.Add(new Song
            {
                ID = -1, TrackNo = newTrackNo, AlbumID = Album.ID
            });
        }

        public void SaveAlbumInformation()
        {
            using (var context = new MusCatEntities())
            {
                context.Entry(Album).State = EntityState.Modified;
                context.SaveChanges();
            }
        }

        public void FixNames()
        {
            var parser = new Mp3Parser();
            parser.FixNames(Songs);
        }

        public void FixTimes()
        {
            var parser = new Mp3Parser();
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
                        // check validity of song data
                        if (song.Error != "")
                        {
                            MessageBox.Show(string.Format("Invalid data in song {0}!", song.TrackNo));
                            continue;
                        }
                        context.Entry(context.Songs.Find(song.ID)).CurrentValues.SetValues(song);
                    }
                    context.SaveChanges();
                }
            }
        }

        public string ChooseImageSavePath()
        {
            var filepaths = FileLocator.MakeAlbumImagePathlist(Album);

            if (filepaths.Count == 0)
            {
                return filepaths[0];
            }

            var choice = new ChoiceWindow();
            choice.SetChoiceList(filepaths);
            choice.ShowDialog();

            return choice.ChoiceResult;
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

            var filepath = ChooseImageSavePath();
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
                    var encoder = new JpegBitmapEncoder();
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
            var ofd = new OpenFileDialog();
            var result = ofd.ShowDialog();
            if (!result.HasValue || result.Value != true)
            {
                return;
            }

            var filepath = ChooseImageSavePath();
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

        #region IDataErrorInfo methods

        public string Error
        {
            get { throw new NotImplementedException(); }
        }

        public string this[string columnName]
        {
            get 
            {
                var error = string.Empty;

                switch (columnName)
                {
                    case "AlbumTotalTime":
                        var regex = new Regex(@"^\d+:\d{2}$");
                        if (!regex.IsMatch(AlbumTotalTime))
                        {
                            error = "Total time should be in the format mm:ss";
                        }
                        break;

                    case "AlbumName":
                        if (Album.Name.Length > 40)
                        {
                            error = "Album name should contain not more than 40 symbols";
                        }
                        else if (Album.Name == "")
                        {
                            error = "Album name can't be empty";
                        }
                        break;
                }
                return error;
            }
        }

        #endregion
    }
}
