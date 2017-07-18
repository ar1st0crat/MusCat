using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using MusCat.Model;
using MusCat.View;
using MusCat.Utils;

namespace MusCat.ViewModel
{
    /// <summary>
    /// MainViewModel is responsible for CRUD operations with performers and albums
    /// (and other stuff from main menu such as Radio, Stats, Settings, Help)
    /// </summary>
    class MainViewModel : INotifyPropertyChanged
    {
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
                RaisePropertyChanged("PerformerPattern");
            }
        }

        private string _albumPattern;
        public string AlbumPattern
        {
            get { return _albumPattern; }
            set
            {
                _albumPattern = value;
                RaisePropertyChanged("AlbumPattern");
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
        public RelayCommand EditAlbumCommand { get; private set; }
        public RelayCommand DeletePerformerCommand { get; private set; }
        public RelayCommand DeleteAlbumCommand { get; private set; }
        public RelayCommand PerformerSearchCommand { get; private set; }
        public RelayCommand IndexLetterCommand { get; private set; }
        public RelayCommand IndexPageCommand { get; private set; }
        public RelayCommand AlbumSearchCommand { get; private set; }
        public RelayCommand EditMusiciansCommand { get; private set; }
        public RelayCommand StartRadioCommand { get; private set; }
        public RelayCommand StatsCommand { get; private set; }
        public RelayCommand SettingsCommand { get; private set; }
        public RelayCommand HelpCommand { get; private set; }

        #endregion

        public MainViewModel()
        {
            // setting up all commands (quite a lot of them)
            GeneralViewCommand = new RelayCommand(() =>
            {
                if (SelectedPerformer?.SelectedAlbum != null)
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
                if (SelectedPerformer?.SelectedAlbum != null)
                {
                    RemoveSelectedAlbum();
                }
                else
                {
                    RemoveSelectedPerformer();
                }
            });

            GeneralEditCommand = new RelayCommand(() =>
            {
                if (SelectedPerformer?.SelectedAlbum != null)
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
            ViewAlbumCommand = new RelayCommand(ViewSelectedAlbum);
            AddPerformerCommand = new RelayCommand(AddPerformer);
            AddAlbumCommand = new RelayCommand(AddAlbum);
            EditPerformerCommand = new RelayCommand(EditPerformer);
            EditAlbumCommand = new RelayCommand(EditAlbum);
            DeletePerformerCommand = new RelayCommand(RemoveSelectedPerformer);
            DeleteAlbumCommand = new RelayCommand(RemoveSelectedAlbum);
            PerformerSearchCommand = new RelayCommand(SelectPerformersByPattern);
            AlbumSearchCommand = new RelayCommand(SelectPerformersByAlbumPattern);
            EditMusiciansCommand = new RelayCommand(() => { });
            StartRadioCommand = new RelayCommand(StartRadio);
            StatsCommand = new RelayCommand(() => { });
            SettingsCommand = new RelayCommand(() => { });
            HelpCommand = new RelayCommand(() => { });

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
                SelectPerformersByFirstLetter();
                RaisePropertyChanged("IndexLetter");
            }
        }
        
        private void CreateUpperNavigationPanel()
        {
            // create the upper navigation panel
            foreach (var c in "ABCDEFGHIJKLMNOPQRSTUVWXYZ")
            {
                LetterCollection.Add(new IndexViewModel
                {
                    Text = c.ToString(),
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
        /// <param name="totalCount">Number of performers in resulting set</param>
        private void CreatePageNavigationPanel(int totalCount)
        {
            PageCollection.Clear();

            var total = (int)Math.Ceiling((double)totalCount / PerformersPerPage);
            
            if (total <= 1)
            {
                return;
            }

            PageCollection = new ObservableCollection<IndexViewModel>(
                Enumerable.Range(1, total)
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
            
            switch (_filter)
            {
                case PerformerFilters.FilterByFirstLetter:
                    SelectPerformersByFirstLetter();
                    break;
                case PerformerFilters.FilterByPattern:
                    SelectPerformersByPattern();
                    break;
                case PerformerFilters.FilterByAlbumPattern:
                    SelectPerformersByAlbumPattern();
                    break;
            }
        }

        #endregion
        
        #region CRUD

        /// <summary>
        /// Create Performer View Models for each performer
        /// (order albums, calculate rate and count the number of albums)
        /// </summary>
        /// <param name="performersSelected">Pre-selected collection of performers to work with</param>
        private void FillPerformerViewModels(IQueryable<Performer> performersSelected)
        {
            // tryin' it out 
            GC.Collect();
            GC.WaitForPendingFinalizers();

            CreatePageNavigationPanel(performersSelected.Count());

            var performersPaged = 
                performersSelected.Skip(_selectedPage * PerformersPerPage)
                                  .Take(PerformersPerPage)
                                  .ToList();
            Performers.Clear();

            foreach (var performer in performersPaged)
            {
                var performerView = new PerformerViewModel { Performer = performer };

                // Fill performer's albumlist
                List<Album> albums;

                // If no album pattern filter is specified, then copy **all** albums to PerformerViewModel
                if (_filter != PerformerFilters.FilterByAlbumPattern)
                {
                    albums = performer.Albums
                                      .OrderBy(a => a.ReleaseYear)
                                      .ThenBy(a => a.Name)
                                      .ToList();

                    foreach (var album in albums)
                    {
                        performerView.Albums.Add(new AlbumViewModel { Album = album });
                    }
                }
                // Otherwise, **filter out albums to show** according to album search pattern
                else
                {
                    albums = performer.Albums
                                      .Where(a => a.Name.ToLower().Contains(AlbumPattern.ToLower()))
                                                                  .OrderBy(a => a.ReleaseYear)
                                                                  .ThenBy(a => a.Name)
                                                                  .ToList();
                    foreach (var album in albums)
                    {
                        performerView.Albums.Add(new AlbumViewModel { Album = album });
                    }
                }

                // Recalculate total rate and number of albums of performer
                performerView.AlbumCount = albums.Count();
                performerView.UpdateAlbumCollectionRate();
                
                // Finally, add fully created performer view model to the list
                Performers.Add(performerView);
            }
        }

        /// <summary>
        /// Select performers whose name starts with string FirstLetter (or not a letter - "Other" case)
        /// </summary>
        private void SelectPerformersByFirstLetter()
        {
            using (var context = new MusCatEntities())
            {
                IQueryable<Performer> performersSelected;

                // query can be of two kinds:

                // 1) single letters ('a', 'b', 'c', ..., 'z')
                if (IndexLetter.Length == 1)
                {
                    performersSelected = 
                        from p in context.Performers.Include("Country").Include("Albums")
                        where p.Name.ToLower().StartsWith(IndexLetter)
                        orderby p.Name
                        select p;
                }
                // 2) The "Other" option
                //    (all performers whose name doesn't start with capital English letter, e.g. "10CC", "Пикник", etc.)
                else
                {
                    performersSelected = 
                        from p in context.Performers.Include("Country").Include("Albums")
                        where string.Compare(p.Name.ToLower().Substring(0, 1), "a", StringComparison.Ordinal) < 0 ||
                              string.Compare(p.Name.ToLower().Substring(0, 1), "z", StringComparison.Ordinal) > 0
                        orderby p.Name
                        select p;
                }

                if (_filter != PerformerFilters.FilterByFirstLetter ||
                    _filterCriterion != IndexLetter)
                {
                    _selectedPage = 0;

                    _filter = PerformerFilters.FilterByFirstLetter;
                    _filterCriterion = IndexLetter;
                }

                FillPerformerViewModels(performersSelected);
            }
        }

        /// <summary>
        /// Select performers whose name contains the search pattern PerformerPattern
        /// (specified in lower navigation panel)
        /// </summary>
        private void SelectPerformersByPattern()
        {
            ActivateUpperPanel(false);

            using (var context = new MusCatEntities())
            {
                // main query in this case
                var performersSelected = 
                    from p in context.Performers.Include("Country").Include("Albums")
                    where p.Name.ToLower().Contains(PerformerPattern.ToLower())
                    orderby p.Name
                    select p;

                if (_filter != PerformerFilters.FilterByPattern || 
                    _filterCriterion != PerformerPattern)
                {
                    _selectedPage = 0;

                    _filter = PerformerFilters.FilterByPattern;
                    _filterCriterion = PerformerPattern;
                }
                
                FillPerformerViewModels(performersSelected);
            }
        }

        /// <summary>
        /// Select performers having albums whose name contains search pattern
        /// (specified in lower navigation panel)
        /// </summary>
        private void SelectPerformersByAlbumPattern()
        {
            ActivateUpperPanel(false);

            using (var context = new MusCatEntities())
            {
                // main query in this case
                var performersSelected = 
                    context.Performers.Include("Country")
                                      .Where(p => p.Albums.Any(a => a.Name.Contains(AlbumPattern)))
                                      .OrderBy(p => p.Name);

                if (_filter != PerformerFilters.FilterByAlbumPattern ||
                    _filterCriterion != AlbumPattern)
                {
                    _selectedPage = 0;

                    _filter = PerformerFilters.FilterByAlbumPattern;
                    _filterCriterion = AlbumPattern;
                }

                FillPerformerViewModels(performersSelected);
            }
        }

        public void ViewSelectedPerformer()
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

        public void EditPerformer()
        {
            if (SelectedPerformer == null)
            {
                MessageBox.Show("Please select performer to edit!");
                return;
            }

            var viewmodel = new EditPerformerViewModel(SelectedPerformer);
            var performerWindow = new EditPerformerWindow
            {
                DataContext = viewmodel
            };

            performerWindow.Show();
        }

        public void AddPerformer()
        {
            // set initial information of a newly added performer
            var performer = new Performer { Name = "Unknown performer" };

            using (var context = new MusCatEntities())
            {
                performer.ID = context.Performers.Any() ? 
                    context.Performers.Max(p => p.ID) + 1 : 1;

                context.Performers.Add(performer);
                context.SaveChanges();

                var viewmodel = new EditPerformerViewModel(new PerformerViewModel
                {
                    Performer = performer
                });

                var performerWindow = new EditPerformerWindow
                {
                    DataContext = viewmodel
                };

                performerWindow.ShowDialog();

                // clear all performers shown in the main window
                Performers.Clear();
                PageCollection.Clear();

                _selectedPage = 0;

                // and show only newly added performer (to focus user's attention on said performer)
                Performers.Add(new PerformerViewModel
                {
                    Performer = performer
                });
            }
        }

        public void RemoveSelectedPerformer()
        {
            var performer = SelectedPerformer.Performer;

            if (performer == null)
            {
                MessageBox.Show("Please select performer to remove");
            }
            else if (MessageBox.Show(string.Format("Are you sure you want to delete '{0}'?", performer.Name),
                                        "Confirmation",
                                        MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                using (var context = new MusCatEntities())
                {
                    var performerToRemove = 
                        context.Performers.SingleOrDefault(p => p.ID == performer.ID);

                    context.Performers.Remove(performerToRemove);
                    context.SaveChanges();

                    Performers.Remove(SelectedPerformer);
                }
            }
        }

        private void ViewSelectedAlbum()
        {
            if (SelectedPerformer?.SelectedAlbum == null)
            {
                MessageBox.Show("Please select album to show!");
                return;
            }

            // load songs of selected album lazily
            SelectedPerformer.SelectedAlbum.LoadSongs();

            var albumWindow = new AlbumWindow();
            var albumViewModel = new AlbumPlaybackViewModel(SelectedPerformer.SelectedAlbum)
            {
                Performer = SelectedPerformer
            };

            albumWindow.DataContext = albumViewModel;
            albumWindow.Show();
        }

        private void EditAlbum()
        {
            if (SelectedPerformer?.SelectedAlbum == null)
            {
                MessageBox.Show("Please select album to edit!");
                return;
            }
            
            SelectedPerformer.SelectedAlbum.LoadSongs();

            var albumWindow = new EditAlbumWindow
            {
                DataContext = new EditAlbumViewModel(SelectedPerformer.SelectedAlbum)
            };

            albumWindow.ShowDialog();

            SelectedPerformer.UpdateAlbumCollectionRate();
        }

        private void AddAlbum()
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

            using (var context = new MusCatEntities())
            {
                album.ID = context.Albums.Any() ? 
                    context.Albums.Max(a => a.ID) + 1 : 1;

                context.Albums.Add(album);
                context.SaveChanges();

                album.Performer = SelectedPerformer.Performer;

                var albumView = new AlbumViewModel
                {
                    Album = album
                };

                var editAlbum = new EditAlbumWindow
                {
                    DataContext = new EditAlbumViewModel(albumView)
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
        }

        private async void RemoveSelectedAlbum()
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

            using (var context = new MusCatEntities())
            {
                var albumToRemove = 
                    context.Albums.SingleOrDefault(a => a.ID == selectedAlbum.Album.ID);

                context.Albums.Remove(albumToRemove);
                await context.SaveChangesAsync();

                SelectedPerformer.Albums.Remove(selectedAlbum);
                    
                // to update view
                SelectedPerformer.AlbumCount = SelectedPerformer.Albums.Count();
                SelectedPerformer.UpdateAlbumCollectionRate();
            }
        }

        #endregion
        
        #region main menu

        private void StartRadio()
        {
            var radio = new Radio();

            // if radioplayer can't find songs to play then why even try opening radio window? 
            if (radio.SelectRandomSong() == null)
            {
                MessageBox.Show("Seems like there's not enough music files on your drives");
                return;
            }

            var radioWindow = new RadioPlayerWindow();
            radioWindow.Show();
        }

        #endregion

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
