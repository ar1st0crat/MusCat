using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Autofac;
using AutoMapper;
using MusCat.Core.Entities;
using MusCat.Core.Interfaces;
using MusCat.Core.Interfaces.Audio;
using MusCat.Core.Interfaces.Data;
using MusCat.Core.Interfaces.Domain;
using MusCat.Core.Interfaces.Radio;
using MusCat.Core.Util;
using MusCat.Infrastructure.Services;
using MusCat.Util;
using MusCat.ViewModels.Entities;
using MusCat.Views;

namespace MusCat.ViewModels
{
    /// <summary>
    ///  MainViewModel is responsible for CRUD operations with performers and albums
    /// (and other stuff from main menu such as Radio, Stats, Settings, Help)
    /// 
    /// NOTE. Since this is a WPF app, it's impossible to resolve
    ///       ALL dependencies in app entrypoint (near the composition root).
    ///       MainViewModel is the "closest object" to composition root
    ///       so some classes are resolved here ad-hoc and nowhere else
    ///       (hence we don't have here a Service Locator antipattern).
    /// 
    /// </summary>
    class MainViewModel : ViewModelBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPerformerService _performerService;
        private readonly IAlbumService _albumService;
        private readonly IRateCalculator _rateCalculator;

        public ObservableCollection<PerformerViewModel> Performers { get; } = 
            new ObservableCollection<PerformerViewModel>();

        private PerformerViewModel _selectedPerformer;
        public PerformerViewModel SelectedPerformer
        {
            get { return _selectedPerformer; }
            set
            {
                _selectedPerformer = value;
                RaisePropertyChanged();

                if (_selectedPerformer != null)
                {
                    UpdateSongCountAsync();
                }
            }
        }

        private AlbumViewModel _selectedAlbum;
        public AlbumViewModel SelectedAlbum
        {
            get { return _selectedAlbum; }
            set
            {
                _selectedAlbum = value;
                RaisePropertyChanged();
            }
        }

        public int AlbumCount => SelectedPerformer.Albums.Count;

        private int _songCount;
        public int SongCount
        {
            get { return _songCount; }
            set
            {
                _songCount = value;
                RaisePropertyChanged();
            }
        }

        private string _performerPattern;
        public string PerformerPattern
        {
            get { return _performerPattern; }
            set
            {
                _performerPattern = value;
                RaisePropertyChanged();
            }
        }

        private string _albumPattern;
        public string AlbumPattern
        {
            get { return _albumPattern; }
            set
            {
                _albumPattern = value;
                RaisePropertyChanged();
            }
        }

        private PerformerFilters _filter = PerformerFilters.FilterByFirstLetter;
        private string _filterCriterion;

        private AlbumViewModel _albumToMove;
        private PerformerViewModel _albumToMovePerformer;

        #region Commands

        public RelayCommand GeneralViewCommand { get; private set; }
        public RelayCommand GeneralDeleteCommand { get; private set; }
        public RelayCommand GeneralEditCommand { get; private set; }
        public RelayCommand ViewPerformerCommand { get; private set; }
        public RelayCommand ViewAlbumCommand { get; private set; }
        public RelayCommand AddPerformerCommand { get; private set; }
        public RelayCommand AddAlbumCommand { get; private set; }
        public RelayCommand EditPerformerCommand { get; private set; }
        public RelayCommand EditMusiciansCommand { get; private set; }
        public RelayCommand EditCountriesCommand { get; private set; }
        public RelayCommand EditAlbumCommand { get; private set; }
        public RelayCommand DeletePerformerCommand { get; private set; }
        public RelayCommand DeleteAlbumCommand { get; private set; }
        public RelayCommand BeginMoveAlbumCommand { get; private set; }
        public RelayCommand MoveAlbumCommand { get; private set; }
        public RelayCommand PerformerSearchCommand { get; private set; }
        public RelayCommand IndexLetterCommand { get; private set; }
        public RelayCommand IndexPageCommand { get; private set; }
        public RelayCommand AlbumSearchCommand { get; private set; }
        public RelayCommand StartRadioCommand { get; private set; }
        public RelayCommand StatsCommand { get; private set; }
        public RelayCommand SettingsCommand { get; private set; }
        public RelayCommand HelpCommand { get; private set; }

        #endregion

        public MainViewModel(IUnitOfWork unitOfWork,
                             IPerformerService performerService,
                             IAlbumService albumService,
                             IRateCalculator rateCalculator)
        {
            Guard.AgainstNull(unitOfWork);
            Guard.AgainstNull(performerService);
            Guard.AgainstNull(albumService);
            Guard.AgainstNull(rateCalculator);

            _unitOfWork = unitOfWork;
            _performerService = performerService;
            _albumService = albumService;
            _rateCalculator = rateCalculator;

            // setting up all commands (quite a lot of them)

            GeneralViewCommand = new RelayCommand(() =>
            {
                if (SelectedAlbum != null)
                {
                    ViewSelectedAlbum();
                }
                else
                {
                    ViewSelectedPerformer();
                }
            });

            GeneralDeleteCommand = new RelayCommand(() =>
            {
                if (SelectedAlbum != null)
                {
                    RemoveSelectedAlbumAsync();
                }
                else
                {
                    RemoveSelectedPerformerAsync();
                }
            });

            GeneralEditCommand = new RelayCommand(() =>
            {
                if (SelectedAlbum != null)
                {
                    EditAlbum();
                }
                else
                {
                    EditPerformer();
                }
            });

            IndexLetterCommand = new RelayCommand(param =>
            {
                _filter = PerformerFilters.FilterByFirstLetter;
                ActivateUpperPanel(false);
                IndexLetter = param.ToString();
                ActivateUpperPanel(true);
            });

            IndexPageCommand = new RelayCommand(NavigatePage);

            ViewPerformerCommand = new RelayCommand(ViewSelectedPerformer);
            EditPerformerCommand = new RelayCommand(EditPerformer);
            EditCountriesCommand = new RelayCommand(EditCountries);
            EditMusiciansCommand = new RelayCommand(() => { });
            ViewAlbumCommand = new RelayCommand(ViewSelectedAlbum);
            EditAlbumCommand = new RelayCommand(EditAlbum);
            AddPerformerCommand = new RelayCommand(AddPerformerAsync);
            AddAlbumCommand = new RelayCommand(AddAlbumAsync);
            DeletePerformerCommand = new RelayCommand(RemoveSelectedPerformerAsync);
            DeleteAlbumCommand = new RelayCommand(RemoveSelectedAlbumAsync);
            BeginMoveAlbumCommand = new RelayCommand(BeginMoveAlbum);
            MoveAlbumCommand = new RelayCommand(MoveAlbum);
            PerformerSearchCommand = new RelayCommand(async () => await SelectPerformersByPatternAsync());
            AlbumSearchCommand = new RelayCommand(async () => await SelectPerformersByAlbumPatternAsync());
            StartRadioCommand = new RelayCommand(async() => await StartRadioAsync());
            StatsCommand = new RelayCommand(ShowStats);
            SettingsCommand = new RelayCommand(ShowSettings);
            HelpCommand = new RelayCommand(ShowHelp);
            
            // create navigation panel
            CreateUpperNavigationPanel();
            // and select the initial set of performers (starting with "A")
            IndexLetter = "A";
        }

        #region Upper navigation panel

        // Letters in upper navigation panel
        public ObservableCollection<IndexViewModel> LetterCollection { get; set; } =
            new ObservableCollection<IndexViewModel>();

        private string _indexLetter;
        public string IndexLetter
        {
            get { return _indexLetter; }
            set
            {
                _indexLetter = value;
                RaisePropertyChanged();
                SelectPerformersByFirstLetterAsync();   // fire and forget
            }
        }
        
        private void CreateUpperNavigationPanel()
        {
            // create the upper navigation panel
            foreach (var letter in "ABCDEFGHIJKLMNOPQRSTUVWXYZ")
            {
                LetterCollection.Add(new IndexViewModel
                {
                    Text = letter.ToString(),
                    IsActive = false
                });
            }

            LetterCollection.Add(new IndexViewModel
            {
                Text = "Other",
                IsActive = false
            });

            LetterCollection[0].IsActive = true;
        }

        private void ActivateUpperPanel(bool active)
        {
            LetterCollection.First(l => l.Text == IndexLetter).IsActive = active;
        }
        
        #endregion

        #region Lower navigation panel (page navigation)

        // Page numbers in lower navigation panel
        public ObservableCollection<IndexViewModel> PageCollection { get; set; } =
            new ObservableCollection<IndexViewModel>();
        
        private int _selectedPage = 0;
        private const int PerformersPerPage = 7;

        /// <summary>
        /// Pagination panel is created each time user updates search filters
        /// </summary>
        /// <param name="totalPages">Number of pages for performers in resulting set</param>
        private void CreatePageNavigationPanel(int totalPages)
        {
            PageCollection.Clear();

            if (totalPages <= 1)
            {
                return;
            }

            PageCollection = new ObservableCollection<IndexViewModel>(
                Enumerable.Range(1, totalPages)
                    .Select(p => new IndexViewModel
                    {
                        Text = p.ToString(),
                        IsActive = false
                    }))
                    {
                        [_selectedPage] = {IsActive = true}
                    };
            
            RaisePropertyChanged("PageCollection");
        }
        
        private void NavigatePage(object page)
        {
            PageCollection[_selectedPage].IsActive = false;

            _selectedPage = int.Parse(page.ToString()) - 1;
            
            // in each case just fire and forget
            switch (_filter)
            {
                case PerformerFilters.FilterByFirstLetter:
                    SelectPerformersByFirstLetterAsync();
                    break;
                case PerformerFilters.FilterByPattern:
                    SelectPerformersByPatternAsync();
                    break;
                case PerformerFilters.FilterByAlbumPattern:
                    SelectPerformersByAlbumPatternAsync();
                    break;
            }
        }

        #endregion
        
        #region CRUD

        /// <summary>
        /// Create Performer View Models for each performer
        /// (order albums, calculate rate and count the number of albums)
        /// </summary>
        /// <param name="performers">Pre-selected collection of performers to work with</param>
        private async Task FillPerformerViewModelsAsync(PageCollection<Performer> performers)
        {
            // why not? 
            GC.Collect();
            GC.WaitForPendingFinalizers();
            
            CreatePageNavigationPanel(performers.TotalPages);

            Performers.Clear();
            foreach (var performer in performers.Items)
            {
                var performerViewModel = Mapper.Map<PerformerViewModel>(performer);

                // Fill performer's albumlist

                IEnumerable<Album> albums;

                // If no album pattern filter is specified, 
                // then copy **all** albums to PerformerViewModel

                if (_filter != PerformerFilters.FilterByAlbumPattern)
                {
                    albums = await _unitOfWork.PerformerRepository
                                              .GetPerformerAlbumsAsync(performer.Id);
                }
                else
                {
                    albums = await _unitOfWork.PerformerRepository
                                              .GetPerformerAlbumsAsync(performer.Id, AlbumPattern);
                }

                performerViewModel.Albums = Mapper.Map<ObservableCollection<AlbumViewModel>>(albums);
                
                // Recalculate total rate and number of albums of performer
                performerViewModel.UpdateAlbumCollectionRate(_rateCalculator);

                // Finally, add fully created performer view model to the list
                Performers.Add(performerViewModel);

                // some animation effect )))
                await Task.Delay(50);
            }
        }

        /// <summary>
        /// Select performers whose name starts with string FirstLetter (or not a letter - "Other" case)
        /// </summary>
        private async Task SelectPerformersByFirstLetterAsync()
        {
            if (_filter != PerformerFilters.FilterByFirstLetter || _filterCriterion != IndexLetter)
            {
                _selectedPage = 0;

                _filter = PerformerFilters.FilterByFirstLetter;
                _filterCriterion = IndexLetter;
            }

            var performers = await 
                _unitOfWork.PerformerRepository
                           .GetByFirstLetterAsync(IndexLetter, _selectedPage, PerformersPerPage);
            
            await FillPerformerViewModelsAsync(performers);
        }

        /// <summary>
        /// Select performers whose name contains the search pattern PerformerPattern
        /// (specified in lower navigation panel)
        /// </summary>
        private async Task SelectPerformersByPatternAsync()
        {
            ActivateUpperPanel(false);
            
            if (_filter != PerformerFilters.FilterByPattern || _filterCriterion != PerformerPattern)
            {
                _selectedPage = 0;

                _filter = PerformerFilters.FilterByPattern;
                _filterCriterion = PerformerPattern;
            }

            var performers = await
                _unitOfWork.PerformerRepository
                           .GetBySubstringAsync(PerformerPattern, _selectedPage, PerformersPerPage);
            
            await FillPerformerViewModelsAsync(performers);
        }

        /// <summary>
        /// Select performers having albums whose name contains search pattern
        /// (specified in lower navigation panel)
        /// </summary>
        private async Task SelectPerformersByAlbumPatternAsync()
        {
            ActivateUpperPanel(false);

            if (_filter != PerformerFilters.FilterByAlbumPattern || _filterCriterion != AlbumPattern)
            {
                _selectedPage = 0;

                _filter = PerformerFilters.FilterByAlbumPattern;
                _filterCriterion = AlbumPattern;
            }

            var performers = await
                _unitOfWork.PerformerRepository
                           .GetByAlbumSubstringAsync(AlbumPattern, _selectedPage, PerformersPerPage);
            
            await FillPerformerViewModelsAsync(performers);
        }

        private void ViewSelectedPerformer()
        {
            if (SelectedPerformer == null)
            {
                MessageBox.Show("Please select performer to show!");
                return;
            }

            var performerWindow = new PerformerWindow
            {
                DataContext = SelectedPerformer
            };

            performerWindow.Show();
        }

        private void EditPerformer()
        {
            if (SelectedPerformer == null)
            {
                MessageBox.Show("Please select performer to edit!");
                return;
            }

            using (var scope = App.DiContainer.BeginLifetimeScope())
            {
                var editPerformerViewModel = scope.Resolve<EditPerformerViewModel>();

                editPerformerViewModel.Performer = SelectedPerformer;
                editPerformerViewModel.Countries = new ObservableCollection<Country>(_unitOfWork.CountryRepository.GetAll());

                var performerWindow = new EditPerformerWindow
                {
                    DataContext = editPerformerViewModel
                };

                performerWindow.ShowDialog();
            }
        }

        public void ViewAlbum(AlbumViewModel album)
        {
            using (var scope = App.DiContainer.BeginLifetimeScope())
            {
                var albumPlayback = scope.Resolve<AlbumPlaybackViewModel>();
                albumPlayback.Album = album;
                albumPlayback.Performer = SelectedPerformer;
                albumPlayback.LoadSongsAsync();

                var albumWindow = new AlbumWindow { DataContext = albumPlayback };
                albumWindow.Show();
            }
        }

        private void ViewSelectedAlbum()
        {
            if (SelectedAlbum == null)
            {
                MessageBox.Show("Please select album to show!");
                return;
            }

            ViewAlbum(SelectedAlbum);
        }

        private void EditAlbum()
        {
            if (SelectedAlbum == null)
            {
                MessageBox.Show("Please select album to edit!");
                return;
            }

            using (var scope = App.DiContainer.BeginLifetimeScope())
            {
                var albumViewModel = scope.Resolve<EditAlbumViewModel>();
                albumViewModel.Album = SelectedAlbum;
                albumViewModel.LoadSongsAsync();

                var albumWindow = new EditAlbumWindow { DataContext = albumViewModel };

                albumWindow.ShowDialog();
            }

            UpdatePerformerPanelAsync();
        }

        private async void AddPerformerAsync()
        {
            // set initial information of a newly added performer
            var performer = (
                    await _performerService.AddPerformerAsync(new Performer { Name = "Unknown" })
                ).Data;

            var performerViewModel = Mapper.Map<PerformerViewModel>(performer);

            using (var scope = App.DiContainer.BeginLifetimeScope())
            {
                var editPerformerViewModel = scope.Resolve<EditPerformerViewModel>();

                editPerformerViewModel.Performer = performerViewModel;
                editPerformerViewModel.Countries = new ObservableCollection<Country>(
                                                            await _unitOfWork.CountryRepository.GetAllAsync());

                var performerWindow = new EditPerformerWindow
                {
                    DataContext = editPerformerViewModel
                };

                performerWindow.ShowDialog();
            }

            // clear all performers shown in the main window
            Performers.Clear();
            PageCollection.Clear();

            ActivateUpperPanel(false);

            _selectedPage = 0;

            // and show only newly added performer (to focus user's attention on said performer)
            Performers.Add(performerViewModel);
        }

        private async void RemoveSelectedPerformerAsync()
        {
            if (SelectedPerformer == null)
            {
                MessageBox.Show("Please select performer to remove");
                return;
            }

            var message = $"Are you sure you want to delete '{SelectedPerformer.Name}' " +
                          $"including an entire discography?";

            if (MessageBox.Show(message, "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                await _performerService.RemovePerformerAsync(SelectedPerformer.Id);
                Performers.Remove(SelectedPerformer);
            }
        }

        private async void AddAlbumAsync()
        {
            if (SelectedPerformer == null)
            {
                MessageBox.Show("Please select performer!");
                return;
            }

            var albumToAdd = new Album
            {
                Id = SelectedPerformer.Id,
                Name = "New Album",
                ReleaseYear = (short)DateTime.Now.Year,
                TotalTime = "0:00"
            };

            var album = (
                    await _performerService.AddAlbumAsync(SelectedPerformer.Id, albumToAdd)
                ).Data;

            var albumViewModel = Mapper.Map<AlbumViewModel>(album);

            using (var scope = App.DiContainer.BeginLifetimeScope())
            {
                var editAlbumViewModel = scope.Resolve<EditAlbumViewModel>();
                editAlbumViewModel.Album = albumViewModel;
                
                var editAlbumWindow = new EditAlbumWindow
                {
                    DataContext = editAlbumViewModel
                };

                editAlbumWindow.ShowDialog();
            }

            await InsertAlbumToCollectionAsync(albumViewModel);
        }

        /// <summary>
        /// Insert the view model of a newly added album at the right place in performer's collection
        /// </summary>
        /// <param name="album">Album view model</param>
        private async Task InsertAlbumToCollectionAsync(AlbumViewModel album)
        {
            var albums = SelectedPerformer.Albums;
            var albumPos = albums.Count;

            for (var i = 0; i < albums.Count; i++)
            {
                // firstly, let's see where newly added album fits by its year of release
                if (album.ReleaseYear > albums[i].ReleaseYear)
                {
                    continue;
                }

                // then, there can be several albums with the same year of release
                // so loop through them to find the place for insertion (by album name)
                albumPos = i;
                while (albumPos < albums.Count &&
                       albums[albumPos].ReleaseYear == album.ReleaseYear &&
                       string.Compare(album.Name, albums[albumPos].Name, StringComparison.Ordinal) > 0)
                {
                    albumPos++;
                }

                break;
            }

            SelectedPerformer.Albums.Insert(albumPos, album);

            await UpdatePerformerPanelAsync();
        }

        private async void RemoveSelectedAlbumAsync()
        {
            if (SelectedPerformer == null)
            {
                MessageBox.Show("Please select performer first!");
                return;
            }

            if (SelectedAlbum == null)
            {
                MessageBox.Show("Please select album to remove");
                return;
            }

            var message = $"Are you sure you want to delete album " +
                          $"'{SelectedAlbum.Name}' " +
                          $"by '{SelectedPerformer.Name}'?";

            if (MessageBox.Show(message, "Confirmation", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
            {
                return;
            }

            await _albumService.RemoveAlbumAsync(SelectedAlbum.Id);

            // to update view
            SelectedPerformer.Albums.Remove(SelectedAlbum);

            await UpdatePerformerPanelAsync();
        }

        private void BeginMoveAlbum()
        {
            if (SelectedAlbum == null)
            {
                return;
            }

            _albumToMove = SelectedAlbum;
            _albumToMovePerformer = SelectedPerformer;

            MessageBox.Show("Now select the performer and click the 'Move album here' button");
        }

        private async void MoveAlbum()
        {
            if (_albumToMove == null)
            {
                MessageBox.Show("Please select album to move");
                return;
            }

            await _albumService.MoveAlbumToPerformerAsync(_albumToMove.Id, SelectedPerformer.Id);

            var album = Mapper.Map<Album>(_albumToMove);
            var performer = Mapper.Map<Performer>(_selectedPerformer);

            _albumToMovePerformer.Albums.Remove(_albumToMove);
            _albumToMovePerformer.UpdateAlbumCollectionRate(_rateCalculator);

            _albumToMove.Performer = performer;
            _albumToMove.LocateImagePath();
            
            if (MessageBox.Show("Do you want to move all corresponding files as well?",
                                "Confirmation",
                                MessageBoxButton.YesNoCancel) != MessageBoxResult.Yes)
            {
                await InsertAlbumToCollectionAsync(_albumToMove);
                return;
            }

            var pathlist = FileLocator.MakePerformerImagePathlist(performer);
            var path = string.Empty;

            if (pathlist.Count == 1)
            {
                path = Path.GetDirectoryName(pathlist.First());
            }
            else
            {
                var choice = new ChoiceWindow();
                choice.SetChoiceList(pathlist.Select(p => Path.GetDirectoryName(p)));
                choice.ShowDialog();

                path = choice.ChoiceResult;

                Directory.CreateDirectory(path);
            }

            // move album cover image file

            var albumPath = FileLocator.GetAlbumImagePath(album);
            if (albumPath != string.Empty)
            {
                File.Move(albumPath, $"{path}\\{Path.GetFileName(albumPath)}");

                _albumToMove.LocateImagePath();
            }

            // move folder with song files

            var albumFolder = FileLocator.FindAlbumPath(album);
            if (albumFolder != string.Empty)
            {
                var destinationFolder = $"{Path.GetDirectoryName(path)}\\{new DirectoryInfo(albumFolder).Name}";

                if (Path.GetPathRoot(albumFolder) == Path.GetPathRoot(path))
                {
                    Directory.Move(albumFolder, destinationFolder);
                }

                // !!!!! .NET, are you kidding me? ((( 

                else
                {
                    Directory.CreateDirectory(destinationFolder);

                    foreach (string dir in Directory.GetDirectories(albumFolder, "*", SearchOption.AllDirectories))
                    {
                        Directory.CreateDirectory(Path.Combine(destinationFolder, dir.Substring(albumFolder.Length + 1)));
                    }

                    foreach (string file in Directory.GetFiles(albumFolder, "*", SearchOption.AllDirectories))
                    {
                        File.Copy(file, Path.Combine(destinationFolder, file.Substring(albumFolder.Length + 1)));
                    }

                    Directory.Delete(albumFolder, true);
                }
            }

            await InsertAlbumToCollectionAsync(_albumToMove);

            _albumToMove = null;
            _albumToMovePerformer = null;
        }

        private async Task UpdatePerformerPanelAsync()
        {
            await UpdateSongCountAsync();
            SelectedPerformer.UpdateAlbumCollectionRate(_rateCalculator);
            RaisePropertyChanged("SelectedPerformer");
        }

        public async Task UpdateSongCountAsync()
        {
            if (SelectedPerformer != null)
            {
                SongCount = await _performerService.SongCountAsync(SelectedPerformer.Id);
            }
        }

        #endregion

        #region main menu

        private async Task StartRadioAsync()
        {
            using (var scope = App.DiContainer.BeginLifetimeScope())
            {
                var radio = scope.Resolve<IRadioService>();
                var player = scope.Resolve<IAudioPlayer>();

                // if radioplayer can't find songs to create playlist
                try
                {
                    await radio.MakeSonglistAsync();
                }
                // then why even try opening radio window? 
                catch (Exception)
                {
                    MessageBox.Show("Seems like there's not enough music files on your drives");
                    return;
                }

                var radioViewModel = new RadioViewModel(radio, player) { ShowAlbum = ViewAlbum };
                var radioWindow = new RadioPlayerWindow { DataContext = radioViewModel };
                radioWindow.Show();
            }
        }
        
        private void ShowStats()
        {
            using (var scope = App.DiContainer.BeginLifetimeScope())
            {
                var statsViewModel = scope.Resolve<StatsViewModel>();
                statsViewModel.LoadStatsAsync();

                var statsWindow = new StatsWindow { DataContext = statsViewModel };

                statsWindow.Show();
            }
        }

        private void EditCountries()
        {
            using (var scope = App.DiContainer.BeginLifetimeScope())
            {
                var countryViewModel = scope.Resolve<EditCountryViewModel>();
                countryViewModel.LoadCountriesAsync();

                var countriesWindow = new EditCountryWindow
                {
                    DataContext = countryViewModel
                };

                countriesWindow.ShowDialog();
            }
        }

        private void ShowSettings() => new SettingsWindow().ShowDialog();

        private void ShowHelp() => new HelpWindow().ShowDialog();

        #endregion
    }
}
