using MusCatalog.Controls;
using MusCatalog.Model;
using MusCatalog.View;
using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MusCatalog.ViewModel
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
        public RelayCommand AlbumSearchCommand { get; private set; }
        public RelayCommand EditMusiciansCommand { get; private set; }
        public RelayCommand StartRadioCommand { get; private set; }
        public RelayCommand StatsCommand { get; private set; }
        public RelayCommand SettingsCommand { get; private set; }
        public RelayCommand HelpCommand { get; private set; }

        #endregion

        #region Upper navigation panel

        // Letters in upper navigation panel
        public ObservableCollection<LetterNavigationButton> LetterCollection { get; set; }
        public string FirstLetter = "A";

        // References to "selected" and "deselected" buttons in upper navigation panel
        private LetterNavigationButton _previousButton;
        private LetterNavigationButton _pressedButton;
        
        public void CreateUpperNavigationPanel()
        {
            LetterCollection = new ObservableCollection<LetterNavigationButton>();

            // create the upper navigation panel
            foreach (var c in "ABCDEFGHIJKLMNOPQRSTUVWXYZ")
            {
                var button = new LetterNavigationButton(c.ToString());
                button.Click += SetFirstLetter;
                LetterCollection.Add(button);
            }

            var buttonOther = new LetterNavigationButton("Other", 70);
            buttonOther.Click += SetFirstLetter;
            LetterCollection.Add(buttonOther);

            // Start with the "A-letter"-list
            _previousButton = LetterCollection[0];
            _previousButton.Select();
        }

        /// <summary>
        /// Upper navigation panel click handler
        /// </summary>
        private void SetFirstLetter(object sender, RoutedEventArgs e)
        {
            _pressedButton = (LetterNavigationButton)sender;
            _previousButton.DeSelect();
            _pressedButton.Select();
            _previousButton = _pressedButton;
            _filter = PerformerFilters.FilterByFirstLetter;
            _selectedPage = 0;

            FirstLetter = _pressedButton.Content.ToString();
            SelectPerformersByFirstLetter();
        }

        /// <summary>
        /// Deselect all buttons in upper navigation panel
        /// </summary>
        private void ResetButtons()
        {
            if (_pressedButton != null)
            {
                _pressedButton.DeSelect();
            }
            else
            {
                _previousButton.DeSelect();
            }
        }

        #endregion

        #region Lower navigation panel (page navigation)

        // Page numbers in lower navigation panel
        public ObservableCollection<UIElement> PageCollection { get; set; }
        
        private int _selectedPage = 0;
        private const int PerformersPerPage = 10;

        private void CreatePageNavigationPanel(int totalCount)
        {
            PageCollection.Clear();

            var total = Math.Ceiling((double)totalCount / PerformersPerPage);

            if (!(total > 1))
            {
                return;
            }

            for (var i = 0; i < total; i++)
            {
                var nb = new TextBlock
                {
                    Tag = i,
                    Text = (i + 1).ToString(),
                    TextDecorations = TextDecorations.Underline,
                    Cursor = Cursors.Hand,
                    Margin = new Thickness(5, 0, 0, 0),
                    Foreground = Brushes.Yellow,
                    Background = Brushes.Transparent
                };
                nb.MouseDown += NavigatePage;

                PageCollection.Add(nb);
            }

            ((TextBlock)PageCollection.ElementAt(_selectedPage)).TextDecorations = null;
        }
        
        private void NavigatePage(object sender, RoutedEventArgs e)
        {
            _selectedPage = (int)((TextBlock)sender).Tag;

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

            PageCollection = new ObservableCollection<UIElement>();

            // create navigation panel
            CreateUpperNavigationPanel();
            // and select the initial set of performers (starting with "A")
            SelectPerformersByFirstLetter();
        }

        /// <summary>
        /// The total rate of performer's album collection is calculated based on the following statistics of album rates:
        /// 
        ///     if the number of albums is more than 2, then the worst rate and the best rate are discarded
        ///     and the total rate is an average of remaining rates
        ///     
        ///     otherwise - the total rate is simply an average of album rates
        ///     
        /// </summary>
        /// <param name="albums">Collection of performer's albums</param>
        /// <returns>The total rate of performer's album collection</returns>
        private byte? CalculateAlbumCollectionRate(IEnumerable<Album> albums)
        {
            var ratedCount = albums.Count(t => t.Rate.HasValue);

            if (ratedCount <= 0)
            {
                return null;
            }

            var sumRate = albums.Sum(t => t.Rate ?? 0);

            if (ratedCount > 2)
            {
                var minRate = albums.Min(r => r.Rate).Value;
                var maxRate = albums.Max(r => r.Rate).Value;
                sumRate -= (minRate + maxRate);
                ratedCount -= 2;
            }

            return (byte)Math.Round((double)sumRate / ratedCount, 
                                        MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// Create Performer View Models for each performer (order albums, calculate rate and count the number of albums)
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
                    albums = performer.Albums.OrderBy(a => a.ReleaseYear)
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
                performerView.AlbumCollectionRate = CalculateAlbumCollectionRate(albums);

                // Finally, add fully created performer view model to the list
                Performers.Add(performerView);
            }
        }

        /// <summary>
        /// Select performers whose name starts with string FirstLetter (or not a letter - "Other" case)
        /// </summary>
        public void SelectPerformersByFirstLetter()
        {
            using (var context = new MusCatEntities())
            {
                IQueryable<Performer> performersSelected;

                // query can be of two kinds:

                // 1) single letters ('a', 'b', 'c', ..., 'z')
                if (FirstLetter.Length == 1)
                {
                    performersSelected = 
                        from p in context.Performers.Include("Country").Include("Albums")
                        where p.Name.ToLower().StartsWith(FirstLetter)
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

                if (_filter != PerformerFilters.FilterByFirstLetter)
                {
                    _filter = PerformerFilters.FilterByFirstLetter;
                    _selectedPage = 0;
                }

                FillPerformerViewModels(performersSelected);
            }
        }

        /// <summary>
        /// Select performers whose name contains the search pattern PerformerPattern
        /// (specified in lower navigation panel)
        /// </summary>
        public void SelectPerformersByPattern()
        {
            using (var context = new MusCatEntities())
            {
                // main query in this case
                var performersSelected = 
                    from p in context.Performers.Include("Country").Include("Albums")
                    where p.Name.ToLower().Contains(PerformerPattern.ToLower())
                    orderby p.Name
                    select p;

                if (_filter != PerformerFilters.FilterByPattern)
                {
                    _selectedPage = 0;
                    _filter = PerformerFilters.FilterByPattern;
                }

                FillPerformerViewModels(performersSelected);

                // deselect all buttons in upper navigation panel
                ResetButtons();

                FirstLetter = "_";
            }
        }

        /// <summary>
        /// Select performers having albums whose name contains search pattern (specified in lower navigation panel)
        /// </summary>
        public void SelectPerformersByAlbumPattern()
        {
            using (var context = new MusCatEntities())
            {
                // main query in this case
                var performersSelected = 
                    context.Performers.Include("Country")
                                      .Where(p => p.Albums.Any(a => a.Name.Contains(AlbumPattern)))
                                      .OrderBy(p => p.Name);

                if (_filter != PerformerFilters.FilterByAlbumPattern)
                {
                    _selectedPage = 0;
                    _filter = PerformerFilters.FilterByAlbumPattern;
                }

                FillPerformerViewModels(performersSelected);

                // deselect all buttons in upper navigation panel
                ResetButtons();

                FirstLetter = "_";
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
                performer.ID = context.Performers.Max(p => p.ID) + 1;
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
                ResetButtons();
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

        public void ViewSelectedAlbum()
        {
            if (SelectedPerformer?.SelectedAlbum == null)
            {
                MessageBox.Show("Please select album to show!");
                return;
            }

            // load songs of selected album lazily
            SelectedPerformer.SelectedAlbum.LoadSongs();

            var albumWindow = new AlbumWindow();
            var albumViewModel = new AlbumPlaybackViewModel(SelectedPerformer.SelectedAlbum);

            // user can change album rate in Album View Window, so we "stay in touch"
            albumViewModel.RateUpdated += AlbumRateUpdated;
            albumWindow.DataContext = albumViewModel;
            albumWindow.Show();
        }

        public void EditAlbum()
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

            SelectedPerformer.AlbumCollectionRate =
                    CalculateAlbumCollectionRate(SelectedPerformer.Performer.Albums);
        }

        public void AddAlbum()
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
                album.ID = context.Albums.Max(a => a.ID) + 1;
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
                    while (albums[albumPos].Album.ReleaseYear == album.ReleaseYear 
                           &&
                           string.Compare(album.Name, albums[albumPos].Album.Name, StringComparison.Ordinal) > 0
                           &&
                           albumPos < albums.Count)
                    {
                        albumPos++;
                    }

                    break;
                }

                SelectedPerformer.Albums.Insert(albumPos, albumView);

                // to update view
                SelectedPerformer.AlbumCount = SelectedPerformer.Albums.Count();
                SelectedPerformer.AlbumCollectionRate = 
                    CalculateAlbumCollectionRate(SelectedPerformer.Performer.Albums);
            }
        }

        public void RemoveSelectedAlbum()
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
                context.SaveChanges();

                SelectedPerformer.Albums.Remove(selectedAlbum);
                    
                // to update view
                SelectedPerformer.AlbumCount = SelectedPerformer.Albums.Count();
            }
        }

        public void StartRadio()
        {
            var radioWindow = new RadioPlayerWindow();
            radioWindow.Show();
        }

        /// <summary>
        /// Handler of the RateUpdated event
        /// </summary>
        /// <param name="sender">Performer whose album has been updated</param>
        /// <param name="e">Empty</param>
        public void AlbumRateUpdated(object sender, EventArgs e)
        {
            var performer = sender as Performer;
            foreach (var performerView in Performers)
            {
                if (performerView.Performer.ID == performer.ID)
                {
                    performerView.AlbumCollectionRate = 
                        CalculateAlbumCollectionRate(performer.Albums);
                    return;
                }
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
