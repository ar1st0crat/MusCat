using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media.Imaging;
using AutoMapper;
using Microsoft.Win32;
using MusCat.Core.Entities;
using MusCat.Core.Interfaces;
using MusCat.Core.Interfaces.Data;
using MusCat.Core.Interfaces.Domain;
using MusCat.Core.Util;
using MusCat.Infrastructure.Services;
using MusCat.Util;
using MusCat.ViewModels.Entities;
using MusCat.Views;
using MusCat.Core.Services;

namespace MusCat.ViewModels
{
    class EditAlbumViewModel : ViewModelBase, IDataErrorInfo
    {
        private readonly IAlbumService _albumService;
        private readonly ISongService _songService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISonglistHelper _songlist;
        
        public AlbumViewModel Album { get; set; }

        public string AlbumName
        {
            get { return Album.Name; }
            set
            {
                Album.Name = value;
                RaisePropertyChanged();
            }
        }

        public string AlbumTotalTime
        {
            get { return Album.TotalTime; }
            set
            {
                Album.TotalTime = value;
                RaisePropertyChanged();
            }
        }

        private ObservableCollection<SongViewModel> _songs;
        public ObservableCollection<SongViewModel> Songs
        {
            get { return _songs; }
            set
            {
                _songs = value;
                RaisePropertyChanged();
            }
        }

        public SongViewModel SelectedSong { get; set; }

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

        public EditAlbumViewModel(IAlbumService albumService,
                                  ISongService songService,
                                  IUnitOfWork unitOfWork,
                                  ISonglistHelper songlist)
        {
            Guard.AgainstNull(albumService);
            Guard.AgainstNull(songService);
            Guard.AgainstNull(unitOfWork);
            Guard.AgainstNull(songlist);

            _albumService = albumService;
            _songService = songService;
            _unitOfWork = unitOfWork;
            _songlist = songlist;

            // setting up commands

            ParseMp3Command = new RelayCommand(ParseMp3);
            FixNamesCommand = new RelayCommand(FixTitles);
            FixTimesCommand = new RelayCommand(FixDurations);
            AddSongCommand = new RelayCommand(AddSong);
            ClearAllSongsCommand = new RelayCommand(async () => await ClearAllAsync());
            SaveAllSongsCommand = new RelayCommand(async () => await SaveAllAsync());
            SaveSongCommand = new RelayCommand(async() => await SaveSongAsync());
            DeleteSongCommand = new RelayCommand(async() => await RemoveSongAsync());
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

        public async Task LoadSongsAsync()
        {
            Songs = Mapper.Map<ObservableCollection<SongViewModel>>(
                await _albumService.LoadAlbumSongsAsync(Album.Id));
        }

        public async Task SaveSongAsync()
        {
            if (SelectedSong == null)
            {
                return;
            }

            var selectedSong = Mapper.Map<Song>(SelectedSong);

            if (selectedSong.Id == -1)
            {
                await SaveAllAsync();
                return;
            }

            var result = await _songService.UpdateSongAsync(selectedSong);

            if (result.Type != ResultType.Ok)
            {
                MessageBox.Show(result.Error);
            }
        }

        public async Task SaveAllAsync()
        {
            // first, check validity of song data

            //if (Songs.Any(s => s.Error != ""))
            //{
            //    var message = Songs.Where(s => s.Error != "")
            //                       .Select(s => s.TrackNo.ToString())
            //                       .Aggregate((t, s) => t + ", " + s);
            //    message = "Errors in songs #" + message;
            //    MessageBox.Show(message);
            //    return;
            //}

            var errorMessage = string.Empty;

            foreach (var songViewModel in Songs)
            {
                var song = Mapper.Map<Song>(songViewModel);
                song.AlbumId = Album.Id;

                if (song.Id == -1)
                {
                    // we save changes after adding each song
                    // because manual autoincrementing always needs actual values of ID
                    var result = await _songService.AddSongAsync(song);

                    if (result.Type != ResultType.Ok)
                    {
                        errorMessage += result.Error + "\n";
                    }
                }
                else
                {
                    var result = await _songService.UpdateSongAsync(song);

                    if (result.Type != ResultType.Ok)
                    {
                        errorMessage += result.Error + "\n";
                    }
                }
            }

            if (errorMessage != string.Empty)
            {
                MessageBox.Show("Errors in songs: " + errorMessage);
            }
        }

        private async Task SaveAlbumInformationAsync()
        {
            await _albumService.UpdateAlbumAsync(Mapper.Map<Album>(Album));
        }

        public async Task RemoveSongAsync()
        {
            var message = $"Are you sure you want to delete the song\n" +
                          $"'{SelectedSong.Name}'\n" +
                          $"by '{Album.Performer.Name}'?";

            if (MessageBox.Show(message, "Confirmation", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
            {
                return;
            }

            if (SelectedSong.Id != -1)
            {
                await _songService.RemoveSongAsync(SelectedSong.Id);
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

            Songs.Add(new SongViewModel
            {
                Id = -1,
                TrackNo = newTrackNo
            });
        }

        public async Task ClearAllAsync()
        {
            var message = $"Are you sure you want to delete all songs in the album\n " +
                          $"'{Album.Name}' \n" +
                          $"by '{Album.Performer.Name}'?";

            if (MessageBox.Show(message, "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                foreach (var song in Songs.Where(song => song.Id != -1))
                {
                    await _songService.RemoveSongAsync(song.Id);
                }

                Songs.Clear();
            }
        }

        private void ParseMp3()
        {
            var fbd = new System.Windows.Forms.FolderBrowserDialog();
            if (fbd.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                return;
            }

            var songs = _songlist.Parse(fbd.SelectedPath);

            Songs = new ObservableCollection<SongViewModel>(
                songs.Select((s, i) => new SongViewModel
                {
                    Id = -1,
                    TrackNo = (byte)(i + 1),
                    Name = s.Title,
                    TimeLength = s.Duration
                }));

            AlbumTotalTime = _songlist.FixDurations(songs);
        }

        private void FixTitles()
        {
            var songs = Songs.Select(s => new SongEntry
            {
                No = s.TrackNo,
                Title = s.Name,
                Duration = s.TimeLength
            })
            .ToList();

            _songlist.FixTitles(songs);

            var i = 0;
            foreach (var song in Songs)
            {
                song.TrackNo = songs[i].No;
                song.Name = songs[i].Title;
                i++;
            }
        }

        private void FixDurations()
        {
            var songs = Songs.Select(s => new SongEntry { Duration = s.TimeLength }).ToList();

            AlbumTotalTime = _songlist.FixDurations(songs);

            var i = 0;
            foreach (var song in Songs)
            {
                song.TimeLength = songs[i++].Duration;
            }
        }

        #region working with images

        private string ChooseImageSavePath()
        {
            var filepaths = FileLocator.MakeAlbumImagePathlist(Mapper.Map<Album>(Album));

            if (filepaths.Count == 1)
            {
                return filepaths[0];
            }

            var choice = new ChoiceWindow();
            choice.SetChoiceList(filepaths);
            choice.ShowDialog();

            return choice.ChoiceResult;
        }

        private void PrepareFileForSaving(string filepath)
        {
            // ensure that target directory exists
            Directory.CreateDirectory(Path.GetDirectoryName(filepath) ?? filepath);

            // first check if file already exists
            if (File.Exists(filepath))
            {
                File.Delete(filepath);
            }
        }

        private void LoadAlbumImageFromClipboard()
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
        }

        private void LoadAlbumImageFromFile()
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
        }

        #endregion

        #region IDataErrorInfo methods

        public string Error
        {
            get
            {
                var error = string.Join("\n", this["AlbumName"], this["AlbumTotalTime"]);
                return error.Replace("\n", "") == string.Empty ? string.Empty : error;
            }
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
                        if (Album.Name.Length > Core.Entities.Album.MaxNameLength)
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
