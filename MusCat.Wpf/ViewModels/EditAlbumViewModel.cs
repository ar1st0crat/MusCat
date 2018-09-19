using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using AutoMapper;
using Microsoft.Win32;
using MusCat.Core.Entities;
using MusCat.Core.Services;
using MusCat.Core.Interfaces.Domain;
using MusCat.Core.Interfaces.Tracklist;
using MusCat.Core.Util;
using MusCat.Infrastructure.Services;
using MusCat.Util;
using MusCat.ViewModels.Entities;
using MusCat.Views;
using MusCat.Core.Interfaces.Networking;

namespace MusCat.ViewModels
{
    class EditAlbumViewModel : ViewModelBase
    {
        private readonly IAlbumService _albumService;
        private readonly ISongService _songService;
        private readonly ITracklistHelper _tracklist;
        private readonly IWebLoader _trackLoader;

        public AlbumViewModel Album { get; set; }

        private ObservableCollection<SongViewModel> _songs = new ObservableCollection<SongViewModel>();
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

        public RelayCommand LoadTracklistCommand { get; private set; }
        public RelayCommand ParseMp3Command { get; private set; }
        public RelayCommand FixTitlesCommand { get; private set; }
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
                                  ITracklistHelper tracklist,
                                  IWebLoader trackLoader)
        {
            Guard.AgainstNull(albumService);
            Guard.AgainstNull(songService);
            Guard.AgainstNull(tracklist);
            Guard.AgainstNull(trackLoader);

            _albumService = albumService;
            _songService = songService;
            _tracklist = tracklist;
            _trackLoader = trackLoader;

            // setting up commands

            LoadTracklistCommand = new RelayCommand(LoadTracklistAsync);
            ParseMp3Command = new RelayCommand(ParseMp3);
            FixTitlesCommand = new RelayCommand(FixTitles);
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
                MessageBox.Show("This song has not been added to database yet.\nPlease save all songs first.");
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
            await SaveAlbumInformationAsync();

            var songs = Mapper.Map<Song[]>(Songs);

            // first, check validity of song data

            if (songs.Any(s => !string.IsNullOrEmpty(s.Error)))
            {
                var message = songs.Where(s => s.Error != "")
                                   .Select(s => s.TrackNo.ToString())
                                   .Aggregate((t, s) => t + ", " + s);

                MessageBox.Show($"Errors in songs #{message}");
                return;
            }

            var i = 0;

            foreach (var song in songs)
            {
                song.AlbumId = Album.Id;

                if (song.Id == -1)
                {
                    await _songService.AddSongAsync(song);

                    // update song Id after adding it to database
                    Songs[i++].Id = song.Id;
                }
                else
                {
                    await _songService.UpdateSongAsync(song);
                }
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

            FixDurations();
        }

        public void AddSong()
        {
            byte trackNo = 1;

            if (Songs.Any())
            {
                trackNo = (byte)(Songs.Last().TrackNo + 1);
            }

            Songs.Add(new SongViewModel { Id = -1, TrackNo = trackNo });
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

        private async void LoadTracklistAsync()
        {
            try
            {
                var tracks = await _trackLoader.LoadTracksAsync(Album.Performer.Name, Album.Name);

                for (var i = 0; i < tracks.Length; i++)
                {
                    var song = Mapper.Map<SongViewModel>(tracks[i]);
                    song.AlbumId = Album.Id;

                    if (i < Songs.Count)
                    {
                        song.Id = Songs[i].Id;
                        Songs[i] = song;
                    }
                    else
                    {
                        Songs.Add(song);
                    }
                }

                FixTitles();
                FixDurations();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ParseMp3()
        {
            var fbd = new System.Windows.Forms.FolderBrowserDialog();
            if (fbd.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                return;
            }

            var songs = _tracklist.Parse(fbd.SelectedPath);

            Songs = Mapper.Map<ObservableCollection<SongViewModel>>(songs);

            Album.TotalTime = _tracklist.FixDurations(songs);
        }
        
        private void FixTitles()
        {
            var songs = Mapper.Map<Track[]>(Songs);

            _tracklist.FixTitles(songs);

            // update songs in-place

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
            var songs = Mapper.Map<Track[]>(Songs);

            Album.TotalTime = _tracklist.FixDurations(songs);

            // update songs in-place

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

            Album.ImagePath = filepath;

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

            Album.ImagePath = filepath;

            RaisePropertyChanged("Album");
        }

        #endregion
    }
}
