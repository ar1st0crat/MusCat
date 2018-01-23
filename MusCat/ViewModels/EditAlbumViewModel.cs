using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using MusCat.Core.Entities;
using MusCat.Core.Interfaces.Data;
using MusCat.Infrastructure.Services;
using MusCat.Infrastructure.Services.Songlist;
using MusCat.Utils;
using MusCat.Views;

namespace MusCat.ViewModels
{
    class EditAlbumViewModel : ViewModelBase, IDataErrorInfo
    {
        public IUnitOfWork _unitOfWork;

        public AlbumViewModel AlbumView { get; set; }
        public Album Album
        {
            get { return AlbumView.Album; }
            set
            {
                AlbumView.Album = value;
                RaisePropertyChanged();
            }
        }

        public string AlbumName
        {
            get { return Album.Name; }
            set
            {
                Album.Name = value;
                RaisePropertyChanged();
                //AlbumView.RaisePropertyChanged("Album");
            }
        }

        public string AlbumTotalTime
        {
            get { return Album.TotalTime; }
            set
            {
                Album.TotalTime = value;
                RaisePropertyChanged();
                //AlbumView.RaisePropertyChanged("Album");
            }
        }

        public ObservableCollection<Song> Songs => AlbumView.Songs;
        public Song SelectedSong { get; set; }

        public ObservableCollection<string> ReleaseYearsCollection { get; set; }

        // starting year is 1900
        private const int StartingYear = 1900;
        // ending year will be defined at run-time as current year + 1
        
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

        public EditAlbumViewModel(AlbumViewModel viewmodel, IUnitOfWork unitOfWork)
        {
            AlbumView = viewmodel;
            _unitOfWork = unitOfWork;

            // setting up commands

            ParseMp3Command = new RelayCommand(ParseMp3);
            FixNamesCommand = new RelayCommand(FixNames);
            FixTimesCommand = new RelayCommand(FixTimes);
            AddSongCommand = new RelayCommand(AddSong);
            ClearAllSongsCommand = new RelayCommand(async () => await ClearAllAsync());
            SaveAllSongsCommand = new RelayCommand(async () => await SaveAllAsync());
            SaveSongCommand = new RelayCommand(async() => await SaveSongAsync());
            DeleteSongCommand = new RelayCommand(async() => await DeleteSongAsync());
            SaveAlbumInformationCommand = new RelayCommand(async () => await SaveAlbumInformationAsync());
            LoadAlbumImageFromFileCommand = new RelayCommand(LoadAlbumImageFromFile);
            LoadAlbumImageFromClipboardCommand = new RelayCommand(LoadAlbumImageFromClipboard);

            // fill combobox with release years from given range

            var endingYear = DateTime.Now.Year + 1;

            ReleaseYearsCollection = new ObservableCollection<string>();
            for (var i = StartingYear; i < endingYear; i++)
            {
                ReleaseYearsCollection.Add(i.ToString());
            }
        }

        public async Task SaveSongAsync()
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

            if (SelectedSong.ID == -1)
            {
                await SaveAllAsync();
                return;
            }

            _unitOfWork.SongRepository.Edit(SelectedSong);
            await _unitOfWork.SaveAsync();
        }

        public async Task DeleteSongAsync()
        {
            if (MessageBox.Show(string.Format("Are you sure you want to delete the song\n'{0}'\nby '{1}'?",
                                    SelectedSong.Name, Album.Performer.Name),
                                    "Confirmation",
                                    MessageBoxButton.YesNo) != MessageBoxResult.Yes)
            {
                return;
            }

            if (SelectedSong.ID != -1)
            {
                _unitOfWork.SongRepository.Delete(SelectedSong);
                await _unitOfWork.SaveAsync();
            }

            Songs.Remove(SelectedSong);
        }

        public void AddSong()
        {
            byte newTrackNo = 1;

            if (Songs.Any())
            {
                newTrackNo = (byte)(Songs.Last().TrackNo + 1);
            }

            Songs.Add(new Song
            {
                ID = -1,
                AlbumID = Album.ID,
                TrackNo = newTrackNo
            });
        }

        public async Task ClearAllAsync()
        {
            if (MessageBox.Show(string.Format("Are you sure you want to delete all songs in the album\n '{0}' \nby '{1}'?",
                                    Album.Name, Album.Performer.Name),
                                    "Confirmation",
                                    MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                foreach (var song in Songs.Where(song => song.ID != -1))
                {
                    _unitOfWork.SongRepository.Delete(song);
                }

                await _unitOfWork.SaveAsync();

                Songs.Clear();
            }
        }

        public async Task SaveAllAsync()
        {
            // first, check validity of song data

            if (Songs.Any(s => s.Error != ""))
            {
                var message = Songs.Where(s => s.Error != "")
                                   .Select(s => s.TrackNo.ToString())
                                   .Aggregate((t, s) => t + ", " + s);
                message = "Errors in songs #" + message;
                MessageBox.Show(message);
                return;
            }

            foreach (var song in Songs)
            {
                if (song.ID == -1)
                {
                    await _unitOfWork.SongRepository.AddAsync(song);
                    // we save changes after adding each song
                    // because manual autoincrementing always needs actual values of ID
                    await _unitOfWork.SaveAsync();

                }
                else
                {
                    _unitOfWork.SongRepository.Edit(song);
                }
            }

            await _unitOfWork.SaveAsync();
        }

        public async Task SaveAlbumInformationAsync()
        {
            _unitOfWork.AlbumRepository.Edit(Album);
            await _unitOfWork.SaveAsync();
        }

        public void ParseMp3()
        {
            var fbd = new System.Windows.Forms.FolderBrowserDialog();
            if (fbd.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                return;
            }

            var parser = new Mp3SonglistHelper();
            parser.Parse(fbd.SelectedPath, Album, Songs);
            AlbumTotalTime = parser.FixTimes(Songs);
        }

        public void FixNames()
        {
            var parser = new Mp3SonglistHelper();
            parser.FixNames(Songs);
        }

        public void FixTimes()
        {
            var parser = new Mp3SonglistHelper();
            AlbumTotalTime = parser.FixTimes(Songs);
        }

        #region working with images

        public string ChooseImageSavePath()
        {
            var filepaths = FileLocator.MakeAlbumImagePathlist(Album);

            if (filepaths.Count == 1)
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
            Directory.CreateDirectory(Path.GetDirectoryName(filepath) ?? filepath);

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
            //AlbumView.RaisePropertyChanged("Album");
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
            //AlbumView.RaisePropertyChanged("Album");
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
                    {
                        var regex = new Regex(@"^\d+:\d{2}$");
                        if (!regex.IsMatch(Album.TotalTime))
                        {
                            error = "Total time should be in the format mm:ss";
                        }
                        break;
                    }
                    case "AlbumName":
                    {
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
                }
                return error;
            }
        }

        #endregion
    }
}
