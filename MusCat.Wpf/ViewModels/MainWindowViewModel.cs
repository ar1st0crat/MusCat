using AutoMapper;
using MusCat.Application.Dto;
using MusCat.Application.Interfaces;
using MusCat.Core.Entities;
using MusCat.Core.Interfaces;
using MusCat.Core.Interfaces.Data;
using MusCat.Core.Util;
using MusCat.Events;
using MusCat.Infrastructure.Services;
using MusCat.ViewModels.Entities;
using MusCat.Views;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace MusCat.ViewModels
{
    class MainWindowViewModel : BindableBase
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPerformerService _performerService;
        private readonly IAlbumService _albumService;
        private readonly IRateCalculator _rateCalculator;
        private readonly IDialogService _dialogService;

        public ObservableCollection<PerformerViewModel> Performers { get; } = new ObservableCollection<PerformerViewModel>();

        private AlbumViewModel _selectedAlbum;
        public AlbumViewModel SelectedAlbum
        {
            get { return _selectedAlbum; }
            set { SetProperty(ref _selectedAlbum, value); }
        }

        private int _songCount;
        public int SongCount
        {
            get { return _songCount; }
            set { SetProperty(ref _songCount, value); }
        }

        private string _performerPattern;
        public string PerformerPattern
        {
            get { return _performerPattern; }
            set { SetProperty(ref _performerPattern, value); }
        }

        private string _albumPattern;
        public string AlbumPattern
        {
            get { return _albumPattern; }
            set { SetProperty(ref _albumPattern, value);
            }
        }

        private PerformerViewModel _selectedPerformer;
        public PerformerViewModel SelectedPerformer
        {
            get { return _selectedPerformer; }
            set
            {
                SetProperty(ref _selectedPerformer, value);

                if (_selectedPerformer != null)
                {
                    UpdateSongCountAsync();
                }
            }
        }

        public int AlbumCount => SelectedPerformer.Albums.Count;

        private PerformerFilters _filter = PerformerFilters.FilterByFirstLetter;
        private string _filterCriterion;

        private AlbumViewModel _albumToMove;
        private PerformerViewModel _albumToMovePerformer;


        #region Commands

        public DelegateCommand GeneralViewCommand { get; }
        public DelegateCommand GeneralDeleteCommand { get;  }
        public DelegateCommand GeneralEditCommand { get; }
        public DelegateCommand ViewPerformerCommand { get; }
        public DelegateCommand ViewAlbumCommand { get; }
        public DelegateCommand AddPerformerCommand { get; }
        public DelegateCommand AddAlbumCommand { get; }
        public DelegateCommand EditPerformerCommand { get; }
        public DelegateCommand EditMusiciansCommand { get; }
        public DelegateCommand EditCountriesCommand { get; }
        public DelegateCommand EditAlbumCommand { get; }
        public DelegateCommand DeletePerformerCommand { get; }
        public DelegateCommand DeleteAlbumCommand { get; }
        public DelegateCommand BeginMoveAlbumCommand { get; }
        public DelegateCommand MoveAlbumCommand { get; }
        public DelegateCommand PerformerSearchCommand { get; }
        public DelegateCommand AlbumSearchCommand { get; }
        public DelegateCommand StartRadioCommand { get; }
        public DelegateCommand StatsCommand { get; }
        public DelegateCommand SettingsCommand { get; }
        public DelegateCommand HelpCommand { get; }

        public DelegateCommand<string> IndexLetterCommand { get; }
        public DelegateCommand<string> IndexPageCommand { get; }

        #endregion


        public MainWindowViewModel(IEventAggregator eventAggregator,
                             IUnitOfWork unitOfWork,
                             IPerformerService performerService,
                             IAlbumService albumService,
                             IRateCalculator rateCalculator,
                             IDialogService dialogService)
        {
            Guard.AgainstNull(eventAggregator);
            Guard.AgainstNull(unitOfWork);
            Guard.AgainstNull(performerService);
            Guard.AgainstNull(albumService);
            Guard.AgainstNull(rateCalculator);
            Guard.AgainstNull(dialogService);

            _eventAggregator = eventAggregator;
            _unitOfWork = unitOfWork;
            _performerService = performerService;
            _albumService = albumService;
            _rateCalculator = rateCalculator;
            _dialogService = dialogService;

            _eventAggregator
                .GetEvent<AlbumRateUpdatedEvent>()
                .Subscribe(UpdateRate, ThreadOption.UIThread);


            // setting up all commands (quite a lot of them)

            GeneralViewCommand = new DelegateCommand(() =>
            {
                if (_selectedAlbum != null)
                {
                    ViewSelectedAlbum();
                }
                else
                {
                    ViewSelectedPerformer();
                }
            });

            GeneralDeleteCommand = new DelegateCommand(() =>
            {
                if (_selectedAlbum != null)
                {
                    RemoveSelectedAlbumAsync();
                }
                else
                {
                    RemoveSelectedPerformerAsync();
                }
            });

            GeneralEditCommand = new DelegateCommand(() =>
            {
                if (_selectedAlbum != null)
                {
                    EditAlbum();
                }
                else
                {
                    EditPerformer();
                }
            });

            ViewPerformerCommand = new DelegateCommand(ViewSelectedPerformer);
            EditPerformerCommand = new DelegateCommand(EditPerformer);
            EditCountriesCommand = new DelegateCommand(EditCountries);
            ViewAlbumCommand = new DelegateCommand(ViewSelectedAlbum);
            EditAlbumCommand = new DelegateCommand(EditAlbum);
            AddPerformerCommand = new DelegateCommand(AddPerformerAsync);
            AddAlbumCommand = new DelegateCommand(AddAlbumAsync);
            DeletePerformerCommand = new DelegateCommand(RemoveSelectedPerformerAsync);
            DeleteAlbumCommand = new DelegateCommand(RemoveSelectedAlbumAsync);
            BeginMoveAlbumCommand = new DelegateCommand(BeginMoveAlbum);
            MoveAlbumCommand = new DelegateCommand(MoveAlbum);
            PerformerSearchCommand = new DelegateCommand(async () => await SelectPerformersByPatternAsync());
            StartRadioCommand = new DelegateCommand(StartRadio);
            StatsCommand = new DelegateCommand(ShowStats);
            SettingsCommand = new DelegateCommand(ShowSettings);
            HelpCommand = new DelegateCommand(ShowHelp);

            IndexLetterCommand = new DelegateCommand<string>(NavigateLetter);
            IndexPageCommand = new DelegateCommand<string>(NavigatePage);

            // create navigation panel
            CreateUpperNavigationPanel();

            // and select the initial set of performers (starting with "A")
            IndexLetter = "A";
        }


        #region Upper navigation panel

        public ObservableCollection<IndexViewModel> LetterCollection { get; set; } = new ObservableCollection<IndexViewModel>();

        private string _indexLetter;
        public string IndexLetter
        {
            get { return _indexLetter; }
            set
            {
                SetProperty(ref _indexLetter,value);
                SelectPerformersByFirstLetterAsync();
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

        public ObservableCollection<IndexViewModel> PageCollection { get; set; } = new ObservableCollection<IndexViewModel>();
        
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

        private void NavigateLetter(string indexLetter)
        {
            if (indexLetter is null)
            {
                return;
            }

            _filter = PerformerFilters.FilterByFirstLetter;
            ActivateUpperPanel(false);
            IndexLetter = indexLetter;
            ActivateUpperPanel(true);
        }

        private void NavigatePage(string page)
        {
            if (page is null)
            {
                return;
            }

            PageCollection[_selectedPage].IsActive = false;

            _selectedPage = int.Parse(page) - 1;
            
            // in each case just fire and forget
            switch (_filter)
            {
                case PerformerFilters.FilterByFirstLetter:
                    SelectPerformersByFirstLetterAsync();
                    break;
                case PerformerFilters.FilterByPattern:
                    SelectPerformersByPatternAsync();
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
        private async Task FillPerformerViewModelsAsync(PageCollection<PerformerDto> performers)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();

            CreatePageNavigationPanel(performers.TotalPages);

            Performers.Clear();
            foreach (var performer in performers.Items)
            {
                var performerViewModel = Mapper.Map<PerformerViewModel>(performer);

                // Fill performer's albumlist
                var albums = await _performerService.GetPerformerAlbumsAsync(performer.Id, AlbumPattern);
                
                performerViewModel.Albums = Mapper.Map<ObservableCollection<AlbumViewModel>>(albums);

                // Recalculate total rate and number of albums of performer
                UpdatePerformerRate(performerViewModel);

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
                _performerService.GetPerformersByFirstLetterAsync(
                    IndexLetter, _selectedPage, PerformersPerPage);

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
                _performerService.GetPerformersBySubstringAsync(
                    PerformerPattern, _selectedPage, PerformersPerPage);
            
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
            //if (SelectedPerformer == null)
            //{
            //    MessageBox.Show("Please select performer to edit!");
            //    return;
            //}

            //using (var scope = App.DiContainer.BeginLifetimeScope())
            //{
            //    var editPerformerViewModel = scope.Resolve<EditPerformerViewModel>();

            //    editPerformerViewModel.Performer = SelectedPerformer;
            //    editPerformerViewModel.Countries = new ObservableCollection<Country>(_unitOfWork.CountryRepository.GetAll());

            //    var performerWindow = new EditPerformerWindow
            //    {
            //        DataContext = editPerformerViewModel
            //    };

            //    performerWindow.ShowDialog();
            //}
        }

        /// <summary>
        /// This is the handler of AlbumRateUpdated event 
        /// (that can be raised in various different windows)
        /// </summary>
        /// <param name="album"></param>
        private void UpdateRate(AlbumViewModel album)
        {
            var performerOnScreen = Performers.FirstOrDefault(p => p.Id == album.PerformerId);

            if (performerOnScreen is null)
            {
                return;
            }

            var albumOnScreen = performerOnScreen.Albums.FirstOrDefault(a => a.Id == album.Id);

            albumOnScreen.Rate = album.Rate;
            
            UpdatePerformerRate(performerOnScreen);
        }

        private void UpdatePerformerRate(PerformerViewModel performer)
        {
            var rates = performer.Albums.Select(a => a.Rate);
            performer.AlbumCollectionRate = _rateCalculator.Calculate(rates);
        }

        public void ViewAlbum(AlbumViewModel album)
        {
            var parameters = new DialogParameters
            {
                { "album", album }
            };

            _dialogService.Show("AlbumWindow", parameters, null);
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
            ////if (SelectedAlbum == null)
            ////{
            ////    MessageBox.Show("Please select album to edit!");
            ////    return;
            ////}

            ////using (var scope = App.DiContainer.BeginLifetimeScope())
            ////{
            ////    var albumViewModel = scope.Resolve<EditAlbumViewModel>();
            ////    albumViewModel.Album = SelectedAlbum;
            ////    albumViewModel.LoadSongsAsync();

            ////    var albumWindow = new EditAlbumWindow { DataContext = albumViewModel };

            ////    albumWindow.ShowDialog();
            ////}

            ////UpdatePerformerPanelAsync();
        }

        private async void AddPerformerAsync()
        {
            //// set initial information of a newly added performer
            //var performer = (
            //        await _performerService.AddPerformerAsync(new Performer { Name = "Unknown" })
            //    ).Data;

            //var performerViewModel = Mapper.Map<PerformerViewModel>(performer);

            //using (var scope = App.DiContainer.BeginLifetimeScope())
            //{
            //    var editPerformerViewModel = scope.Resolve<EditPerformerViewModel>();

            //    editPerformerViewModel.Performer = performerViewModel;
            //    editPerformerViewModel.Countries = new ObservableCollection<Country>(
            //                                                await _unitOfWork.CountryRepository.GetAllAsync());

            //    var performerWindow = new EditPerformerWindow
            //    {
            //        DataContext = editPerformerViewModel
            //    };

            //    performerWindow.ShowDialog();
            //}

            //// clear all performers shown in the main window
            //Performers.Clear();
            //PageCollection.Clear();

            //ActivateUpperPanel(false);

            //_selectedPage = 0;

            //// and show only newly added performer (to focus user's attention on said performer)
            //Performers.Add(performerViewModel);
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
            //if (SelectedPerformer == null)
            //{
            //    MessageBox.Show("Please select performer!");
            //    return;
            //}

            //var albumToAdd = new Album
            //{
            //    Id = SelectedPerformer.Id,
            //    Name = "New Album",
            //    ReleaseYear = (short)DateTime.Now.Year,
            //    TotalTime = "0:00"
            //};

            //var album = (
            //        await _performerService.AddAlbumAsync(SelectedPerformer.Id, albumToAdd)
            //    ).Data;

            //var albumViewModel = Mapper.Map<AlbumViewModel>(album);

            //using (var scope = App.DiContainer.BeginLifetimeScope())
            //{
            //    var editAlbumViewModel = scope.Resolve<EditAlbumViewModel>();
            //    editAlbumViewModel.Album = albumViewModel;
                
            //    var editAlbumWindow = new EditAlbumWindow
            //    {
            //        DataContext = editAlbumViewModel
            //    };

            //    editAlbumWindow.ShowDialog();
            //}

            //await InsertAlbumToCollectionAsync(albumViewModel);
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
            
            UpdatePerformerRate(_albumToMovePerformer);


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

            UpdatePerformerRate(SelectedPerformer);
            //RaisePropertyChanged("SelectedPerformer");
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

        private void EditCountries()
        {
            //using (var scope = App.DiContainer.BeginLifetimeScope())
            //{
            //    var countryViewModel = scope.Resolve<EditCountryViewModel>();
            //    countryViewModel.LoadCountriesAsync();

            //    var countriesWindow = new EditCountryWindow
            //    {
            //        DataContext = countryViewModel
            //    };

            //    countriesWindow.ShowDialog();
            //}
        }

        private void StartRadio() => _dialogService.Show("RadioWindow");

        private void ShowStats() => _dialogService.Show("StatsWindow");

        private void ShowSettings() => new SettingsWindow().ShowDialog();

        private void ShowHelp() => new HelpWindow().ShowDialog();

        #endregion
    }
}
