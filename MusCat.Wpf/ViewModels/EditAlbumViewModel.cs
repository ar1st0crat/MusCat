using AutoMapper;
using Microsoft.Win32;
using MusCat.Application.Interfaces;
using MusCat.Core.Entities;
using MusCat.Core.Interfaces.Networking;
using MusCat.Core.Interfaces.Tracklist;
using MusCat.Core.Util;
using MusCat.Infrastructure.Services;
using MusCat.ViewModels.Entities;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace MusCat.ViewModels
{
    class EditAlbumViewModel : BindableBase, IDialogAware
    {
        private readonly IAlbumService _albumService;
        private readonly ISongService _songService;
        private readonly ITracklistHelper _tracklist;
        private readonly ITracklistWebLoader _tracklistWebLoader;
        private readonly IDialogService _dialogService;

        public AlbumViewModel Album { get; set; }

        private ObservableCollection<SongViewModel> _songs = new ObservableCollection<SongViewModel>();
        public ObservableCollection<SongViewModel> Songs
        {
            get { return _songs; }
            set { SetProperty(ref _songs, value); }
        }

        public SongViewModel SelectedSong { get; set; }
                

        #region Commands

        public DelegateCommand LoadTracklistCommand { get; }
        public DelegateCommand ParseMp3Command { get; }
        public DelegateCommand FixTitlesCommand { get; }
        public DelegateCommand FixTimesCommand { get; }
        public DelegateCommand ClearAllSongsCommand { get; }
        public DelegateCommand SaveAlbumCommand { get; }
        public DelegateCommand AddSongCommand { get; }
        public DelegateCommand SaveSongCommand { get; }
        public DelegateCommand DeleteSongCommand { get; }
        public DelegateCommand SaveAlbumInformationCommand { get; }
        public DelegateCommand LoadAlbumImageFromFileCommand { get; }
        public DelegateCommand LoadAlbumImageFromClipboardCommand { get; }

        #endregion

        public EditAlbumViewModel(IAlbumService albumService,
                                  ISongService songService,
                                  ITracklistHelper tracklist,
                                  ITracklistWebLoader tracklistWebLoader,
                                  IDialogService dialogService)
        {
            Guard.AgainstNull(albumService);
            Guard.AgainstNull(songService);
            Guard.AgainstNull(tracklist);
            Guard.AgainstNull(tracklistWebLoader);
            Guard.AgainstNull(dialogService);

            _albumService = albumService;
            _songService = songService;
            _tracklist = tracklist;
            _tracklistWebLoader = tracklistWebLoader;
            _dialogService = dialogService;

            // setting up commands

            LoadTracklistCommand = new DelegateCommand(LoadTracklistAsync);
            ParseMp3Command = new DelegateCommand(ParseMp3);
            FixTitlesCommand = new DelegateCommand(FixTitles);
            FixTimesCommand = new DelegateCommand(FixDurations);
            AddSongCommand = new DelegateCommand(AddSong);
            ClearAllSongsCommand = new DelegateCommand(async () => await ClearAllAsync());
            SaveAlbumCommand = new DelegateCommand(async () => await SaveAllAsync());
            SaveSongCommand = new DelegateCommand(async() => await SaveSongAsync());
            DeleteSongCommand = new DelegateCommand(async() => await RemoveSongAsync());
            LoadAlbumImageFromFileCommand = new DelegateCommand(LoadAlbumImageFromFile);
            LoadAlbumImageFromClipboardCommand = new DelegateCommand(LoadAlbumImageFromClipboard);
        }

        public async Task LoadSongsAsync()
        {
            Songs = Mapper.Map<ObservableCollection<SongViewModel>>(
                await _albumService.GetAlbumSongsAsync(Album.Id));
        }

        public async Task SaveSongAsync()
        {
            if (SelectedSong is null)
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

            if (Songs.Any(s => !string.IsNullOrEmpty(s.Error)))
            {
                var message = Songs.Where(s => s.Error != "")
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
            await _albumService.UpdateAlbumAsync(Album.Id, Mapper.Map<Album>(Album));
        }

        public async Task RemoveSongAsync()
        {
            var message = $"Are you sure you want to delete the song " +
                          $"'{SelectedSong.Name}' " +
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
            var message = $"Are you sure you want to delete all songs in the album " +
                          $"'{Album.Name}' " +
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
                var tracks = await _tracklistWebLoader.LoadTracksAsync(Album.Performer.Name, Album.Name);

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

            var parameters = new DialogParameters
            {
                { "options", filepaths }
            };

            string path = null;

            _dialogService.ShowDialog("ChoiceWindow", parameters, r =>
            {
                path = r.Parameters.GetValue<string>("choice");
            });

            return path;
        }

        private void PrepareFileForSaving(string filepath)
        {
            // ensure that target directory exists
            Directory.CreateDirectory(Path.GetDirectoryName(filepath) ?? filepath);

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
            if (filepath is null)
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
            if (filepath is null)
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


        #region IDialogAware implementation

        public string Title => $"Edit album: {Album.Performer.Name} - {Album.Name}";

        public event Action<IDialogResult> RequestClose;

        public bool CanCloseDialog() => true;

        public void OnDialogOpened(IDialogParameters parameters)
        {
            Album = parameters.GetValue<AlbumViewModel>("album");

            LoadSongsAsync();
        }

        public void OnDialogClosed()
        {
        }

        #endregion
    }
}
