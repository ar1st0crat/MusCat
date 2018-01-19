using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using MusCat.Entities;
using MusCat.Repositories.Base;
using MusCat.Services;
using MusCat.Services.Radio;
using MusCat.Utils;
using MusCat.Views;

namespace MusCat.ViewModels
{
    /// <summary>
    /// MainViewModel is responsible for CRUD operations with performers and albums
    /// (and other stuff from main menu such as Radio, Stats, Settings, Help)
    /// </summary>
    class MainViewModel : ViewModelBase
    {
        private readonly UnitOfWork _unitOfWork = new UnitOfWork();
        
        public ObservableCollection<PerformerViewModel> Performers { get; } = 
            new ObservableCollection<PerformerViewModel>();

        public PerformerViewModel SelectedPerformer { get; set; }
        
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
        public RelayCommand PerformerSearchCommand { get; private set; }
        public RelayCommand IndexLetterCommand { get; private set; }
        public RelayCommand IndexPageCommand { get; private set; }
        public RelayCommand AlbumSearchCommand { get; private set; }
        public RelayCommand StartRadioCommand { get; private set; }
        public RelayCommand StatsCommand { get; private set; }
        public RelayCommand SettingsCommand { get; private set; }
        public RelayCommand HelpCommand { get; private set; }

        #endregion

        public MainViewModel()
        {
            // setting up all commands (quite a lot of them)

            GeneralViewCommand = new RelayCommand(async () =>
            {
                if (SelectedPerformer?.SelectedAlbum != null)
                {
                    await ViewSelectedAlbumAsync();
                }
                else
                {
                    ViewSelectedPerformer();
                }
            });

            GeneralDeleteCommand = new RelayCommand(async () =>
            {
                if (SelectedPerformer?.SelectedAlbum != null)
                {
                    await RemoveSelectedAlbumAsync();
                }
                else
                {
                    await RemoveSelectedPerformerAsync();
                }
            });

            GeneralEditCommand = new RelayCommand(async () =>
            {
                if (SelectedPerformer?.SelectedAlbum != null)
                {
                    await EditAlbumAsync();
                }
                else
                {
                    await EditPerformerAsync();
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
            EditPerformerCommand = new RelayCommand(async () => await EditPerformerAsync());
            EditCountriesCommand = new RelayCommand(EditCountries);
            EditMusiciansCommand = new RelayCommand(() => { });
            ViewAlbumCommand = new RelayCommand(async () => await ViewSelectedAlbumAsync());
            EditAlbumCommand = new RelayCommand(async () => await EditAlbumAsync());
            AddPerformerCommand = new RelayCommand(async () => await AddPerformerAsync());
            AddAlbumCommand = new RelayCommand(async () => await AddAlbumAsync());
            DeletePerformerCommand = new RelayCommand(async () => await RemoveSelectedPerformerAsync());
            DeleteAlbumCommand = new RelayCommand(async () => await RemoveSelectedAlbumAsync());
            PerformerSearchCommand = new RelayCommand(async () => await SelectPerformersByPatternAsync());
            AlbumSearchCommand = new RelayCommand(async () => await SelectPerformersByAlbumPatternAsync());
            StartRadioCommand = new RelayCommand(async() => await StartRadioAsync());
            StatsCommand = new RelayCommand(ShowStats);
            SettingsCommand = new RelayCommand(ShowSettings);
            HelpCommand = new RelayCommand(ShowHelp);

            FileLocator.Initialize();

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
        private const int PerformersPerPage = 10;

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
            
            // first, load relevant data into memory
            var performerViews = new List<PerformerViewModel>();
            foreach (var performer in performers.Items)
            {
                var performerView = new PerformerViewModel { Performer = performer };

                // Fill performer's albumlist
                IEnumerable<Album> albums;

                // If no album pattern filter is specified, 
                // then copy **all** albums to PerformerViewModel
                if (_filter != PerformerFilters.FilterByAlbumPattern)
                {
                    albums = await _unitOfWork.PerformerRepository.GetPerformerAlbumsAsync(performer);
                }
                else
                {
                    albums = await _unitOfWork.PerformerRepository.GetPerformerAlbumsAsync(performer, AlbumPattern);
                }
                
                foreach (var album in albums)
                {
                    performerView.Albums.Add(new AlbumViewModel { Album = album });
                }

                // Recalculate total rate and number of albums of performer
                performerView.AlbumCount = performerView.Albums.Count();
                performerView.UpdateAlbumCollectionRate();

                // Finally, add fully created performer view model to the list
                performerViews.Add(performerView);
            }

            // actual binding with observable collection happens here
            Performers.Clear();
            foreach (var performer in performerViews)
            {
                Performers.Add(performer);

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

            var performers = await _unitOfWork.PerformerRepository
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

            var performers = await _unitOfWork.PerformerRepository
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

            var performers = await _unitOfWork.PerformerRepository
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

        private async Task EditPerformerAsync()
        {
            if (SelectedPerformer == null)
            {
                MessageBox.Show("Please select performer to edit!");
                return;
            }

            var viewmodel = new EditPerformerViewModel(SelectedPerformer)
            {
                UnitOfWork = _unitOfWork,
                Countries = new ObservableCollection<Country>(
                                    await _unitOfWork.CountryRepository.GetAllAsync())
            };
            var performerWindow = new EditPerformerWindow { DataContext = viewmodel };

            performerWindow.Show();
        }

        private async Task ViewSelectedAlbumAsync()
        {
            var album = SelectedPerformer?.SelectedAlbum;
            if (album == null)
            {
                MessageBox.Show("Please select album to show!");
                return;
            }

            // load songs of selected album lazily
            album.Songs = new ObservableCollection<Song>(
                await _unitOfWork.AlbumRepository.GetAlbumSongsAsync(album.Album));

            var albumWindow = new AlbumWindow();
            var albumViewModel = new AlbumPlaybackViewModel(album)
            {
                Performer = SelectedPerformer,
                UnitOfWork = _unitOfWork
            };

            albumWindow.DataContext = albumViewModel;
            albumWindow.Show();
        }

        private async Task EditAlbumAsync()
        {
            var album = SelectedPerformer?.SelectedAlbum;
            if (album == null)
            {
                MessageBox.Show("Please select album to edit!");
                return;
            }

            album.Songs = new ObservableCollection<Song>(
                await _unitOfWork.AlbumRepository.GetAlbumSongsAsync(album.Album));

            var albumWindow = new EditAlbumWindow
            {
                DataContext = new EditAlbumViewModel(album) { UnitOfWork = _unitOfWork }
            };

            albumWindow.ShowDialog();

            SelectedPerformer.UpdateAlbumCollectionRate();
        }

        private async Task AddPerformerAsync()
        {
            // set initial information of a newly added performer
            var performer = new Performer { Name = "Unknown performer" };

            await _unitOfWork.PerformerRepository.AddAsync(performer);
            await _unitOfWork.SaveAsync();

            var viewmodel = new EditPerformerViewModel(new PerformerViewModel { Performer = performer })
            {
                UnitOfWork = _unitOfWork,
                Countries = new ObservableCollection<Country>(
                    await _unitOfWork.CountryRepository.GetAllAsync())
            };
            
            var performerWindow = new EditPerformerWindow { DataContext = viewmodel };

            performerWindow.ShowDialog();

            // clear all performers shown in the main window
            Performers.Clear();
            PageCollection.Clear();

            _selectedPage = 0;

            // and show only newly added performer (to focus user's attention on said performer)
            Performers.Add(new PerformerViewModel { Performer = performer });
        }

        private async Task RemoveSelectedPerformerAsync()
        {
            var performer = SelectedPerformer?.Performer;

            if (performer == null)
            {
                MessageBox.Show("Please select performer to remove");
            }
            else if (MessageBox.Show(string.Format("Are you sure you want to delete '{0}'?", performer.Name),
                                        "Confirmation",
                                        MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                _unitOfWork.PerformerRepository.Delete(performer);
                await _unitOfWork.SaveAsync();

                Performers.Remove(SelectedPerformer);
            }
        }

        private async Task AddAlbumAsync()
        {
            if (SelectedPerformer == null)
            {
                MessageBox.Show("Please select performer!");
                return;
            }

            // set initial information of a newly added album
            var album = new Album
            {
                Name = "New Album",
                TotalTime = "00:00",
                PerformerID = SelectedPerformer.Performer.ID,
                ReleaseYear = (short)DateTime.Now.Year
            };

            await _unitOfWork.AlbumRepository.AddAsync(album);
            await _unitOfWork.SaveAsync();


            album.Performer = SelectedPerformer.Performer;

            var albumView = new AlbumViewModel { Album = album };
            var editAlbum = new EditAlbumWindow
            {
                DataContext = new EditAlbumViewModel(albumView) { UnitOfWork = _unitOfWork }
            };

            editAlbum.ShowDialog();

            var albums = SelectedPerformer.Albums;

            // Insert the view model of a newly added album at the right place in performer's collection
            var albumPos = albums.Count;

            for (var i = 0; i < albums.Count; i++)
            {
                // firstly, let's see where newly added album fits by its year of release
                if (album.ReleaseYear > albums[i].Album.ReleaseYear)
                {
                    continue;
                }

                // then, there can be several albums with the same year of release
                // so loop through them to find the place for insertion (by album name)
                albumPos = i;
                while (albums[albumPos].Album.ReleaseYear == album.ReleaseYear &&
                       string.Compare(album.Name, albums[albumPos].Album.Name, StringComparison.Ordinal) > 0 &&
                       albumPos < albums.Count)
                {
                    albumPos++;
                }

                break;
            }

            SelectedPerformer.Albums.Insert(albumPos, albumView);

            // to update view
            SelectedPerformer.AlbumCount = SelectedPerformer.Albums.Count();
            SelectedPerformer.UpdateAlbumCollectionRate();
        }

        private async Task RemoveSelectedAlbumAsync()
        {
            if (SelectedPerformer == null)
            {
                MessageBox.Show("Please select performer first!");
                return;
            }

            var selectedAlbum = SelectedPerformer.SelectedAlbum;

            if (selectedAlbum == null)
            {
                MessageBox.Show("Please select album to remove");
                return;
            }

            if (MessageBox.Show(string.Format("Are you sure you want to delete\n '{0}' \nby '{1}'?",
                                    selectedAlbum.Album.Name,
                                    selectedAlbum.Album.Performer.Name),
                                    "Confirmation",
                                    MessageBoxButton.YesNo) != MessageBoxResult.Yes)
            {
                return;
            }

            _unitOfWork.AlbumRepository.Delete(selectedAlbum.Album);
            await _unitOfWork.SaveAsync();

            SelectedPerformer.Albums.Remove(selectedAlbum);

            // to update view
            SelectedPerformer.AlbumCount = SelectedPerformer.Albums.Count();
            SelectedPerformer.UpdateAlbumCollectionRate();
        }

        #endregion
        
        #region main menu

        private async Task StartRadioAsync()
        {
            var radio = new RadioService();

            // if radioplayer can't find songs to create playlist
            // then why even try opening radio window? 

            try
            {
                await radio.MakeSonglistAsync();
            }
            catch (Exception)
            {
                MessageBox.Show("Seems like there's not enough music files on your drives");
                return;
            }

            var radioWindow = new RadioPlayerWindow();
            radioWindow.Show();
        }

        private void EditCountries()
        {
            var countriesWindow = new CountriesWindow
            {
                DataContext = new CountriesViewModel(_unitOfWork)
            };

            countriesWindow.ShowDialog();
        }

        private async void ShowStats()
        {
            var viewModel = new StatsViewModel();
            await viewModel.LoadStatsAsync();

            var statsWindow = new StatsWindow { DataContext = viewModel };

            statsWindow.Show();
        }

        private void ShowSettings()
        {
            var settingsWindow = new SettingsWindow();
            settingsWindow.ShowDialog();
        }

        private void ShowHelp()
        {
            var helpWindow = new HelpWindow();
            helpWindow.ShowDialog();
        }

        #endregion
    }
}
