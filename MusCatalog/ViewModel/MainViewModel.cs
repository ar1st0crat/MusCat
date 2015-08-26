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
    enum PerformerFilters : byte
    {
        FilteredByFirstLetter,
        FilteredByPattern,
        FilteredByAlbumPattern
    }

    public class MainViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<PerformerViewModel> performers = new ObservableCollection<PerformerViewModel>();
        public ObservableCollection<PerformerViewModel> Performers
        {
            get { return performers; }
        }

        public PerformerViewModel SelectedPerformer { get; set; }
        
        private string performerPattern;
        public string PerformerPattern
        {
            get { return performerPattern; }
            set
            {
                performerPattern = value;
                RaisePropertyChanged("PerformerPattern");
            }
        }

        private string albumPattern;
        public string AlbumPattern
        {
            get { return albumPattern; }
            set
            {
                albumPattern = value;
                RaisePropertyChanged("AlbumPattern");
            }
        }

        PerformerFilters Filter = PerformerFilters.FilteredByFirstLetter;
        

        #region Upper navigation panel

        // Letters in upper navigation panel
        public ObservableCollection<LetterNavigationButton> LetterCollection { get; set; }
        public string FirstLetter = "A";

        // References to "selected" and "deselected" buttons in upper navigation panel
        private LetterNavigationButton prevButton = null;
        private LetterNavigationButton pressedButton = null;


        public void CreateUpperNavigationPanel()
        {
            LetterCollection = new ObservableCollection<LetterNavigationButton>();

            // create the upper navigation panel
            foreach (char c in "ABCDEFGHIJKLMNOPQRSTUVWXYZ")
            {
                LetterNavigationButton b = new LetterNavigationButton( c.ToString() );
                b.Click += LetterClick;
                LetterCollection.Add(b);
            }

            LetterNavigationButton bOther = new LetterNavigationButton( "Other", 70 );
            bOther.Click += LetterClick;
            LetterCollection.Add(bOther);

            // Start with the "A-letter"-list
            prevButton = LetterCollection[0];
            prevButton.Select();
        }

        /// <summary>
        /// Upper navigation panel click handler
        /// </summary>
        private void LetterClick(object sender, RoutedEventArgs e)
        {
            pressedButton = (LetterNavigationButton)sender;
            prevButton.DeSelect();
            pressedButton.Select();
            prevButton = pressedButton;
            FirstLetter = pressedButton.Content.ToString();
            Filter = PerformerFilters.FilteredByFirstLetter;
            SelectedPage = 0;
            SelectPerformersByFirstLetter();
        }

        /// <summary>
        /// Deselect all buttons in upper navigation panel
        /// </summary>
        private void ResetButtons()
        {
            if (pressedButton != null)
            {
                pressedButton.DeSelect();
            }
            else
            {
                prevButton.DeSelect();
            }
        }

        #endregion

        #region Lower navigation panel (page navigation)

        // Page numbers in lower navigation panel
        private ObservableCollection<UIElement> pageCollection = new ObservableCollection<UIElement>();
        public ObservableCollection<UIElement> PageCollection
        {
            get { return pageCollection; }
            set { pageCollection = value; }
        }

        int SelectedPage = 0;
        int PerformersPerPage = 10;

        private void CreatePageNavigationPanel( int totalCount )
        {
            PageCollection.Clear();

            var total = Math.Ceiling((double)totalCount / PerformersPerPage);
            
            if (total > 1)
            {
                for (int i = 0; i < total; i++)
                {
                    var nb = new TextBlock();
                    nb.Tag = i;
                    nb.Text = (i + 1).ToString();
                    nb.TextDecorations = TextDecorations.Underline;
                    nb.Cursor = Cursors.Hand;
                    nb.Margin = new Thickness(5, 0, 0, 0);
                    nb.Foreground = Brushes.Yellow;
                    nb.Background = Brushes.Transparent;
                    nb.MouseDown += NavigatePage;

                    PageCollection.Add(nb);
                }

                ((TextBlock)PageCollection.ElementAt(SelectedPage)).TextDecorations = null;
            }
        }
        
        private void NavigatePage(object sender, RoutedEventArgs e)
        {
            SelectedPage = (int)((TextBlock)sender).Tag;

            switch (Filter)
            {
                case PerformerFilters.FilteredByFirstLetter:
                    SelectPerformersByFirstLetter();
                    break;
                case PerformerFilters.FilteredByPattern:
                    SelectPerformersByPattern();
                    break;
                case PerformerFilters.FilteredByAlbumPattern:
                    SelectPerformersByAlbumPattern();
                    break;
            }
        }

        #endregion


        public MainViewModel()
        {
            CreateUpperNavigationPanel();
            SelectPerformersByFirstLetter();
        }

        /// <summary>
        /// The total rate of performer's album collection is calculated based on the following statistics of album rates:
        /// 
        ///     if the number of albums is more than 2 then the worst rate and the best rate are discarded
        ///     and the total rate is an average of remaining rates
        ///     
        ///     otherwise - the total rate is simply an average of album rates
        ///     
        /// </summary>
        /// <param name="albums">The list of performer's albums</param>
        /// <returns>The total rate of performer's album collection</returns>
        private byte? CalculateAlbumCollectionRate( List<Album> albums )
        {
            byte? avgRate = null;

            int ratedCount = albums.Count(t => t.Rate.HasValue);
            if (ratedCount > 0)
            {
                int sumRate = albums.Sum(t =>
                {
                    if (t.Rate.HasValue)
                        return t.Rate.Value;
                    else
                        return 0;
                });

                if (ratedCount > 2)
                {
                    byte minRate = albums.Min(r => r.Rate).Value;
                    byte maxRate = albums.Max(r => r.Rate).Value;
                    sumRate -= (minRate + maxRate);
                    ratedCount -= 2;
                }

                avgRate = (byte)Math.Round((double)sumRate / ratedCount, MidpointRounding.AwayFromZero);
            }

            return avgRate;
        }

        /// <summary>
        /// Create Performer View Models for each performer (order albums, calculate rate and count the number of albums)
        /// </summary>
        /// <param name="performersSelected">Pre-selected collection of performers to work with</param>
        private void MakePerformerViewModels( IQueryable<Performer> performersSelected )
        {
            // tryin' it out 
            GC.Collect();
            GC.WaitForPendingFinalizers();

            CreatePageNavigationPanel( performersSelected.Count() );

            var performersPaged = performersSelected.Skip( SelectedPage * PerformersPerPage )
                                                    .Take( PerformersPerPage )
                                                    .ToList();
            performers.Clear();

            foreach (var perf in performersPaged)
            {
                var performerView = new PerformerViewModel { Performer = perf };

                /// Fill performer's albumlist
                List<Album> albums;

                // If no album pattern filter is specified, then copy --all-- albums to PerformerViewModel
                if ( Filter != PerformerFilters.FilteredByAlbumPattern )
                {
                    albums = perf.Albums.OrderBy(a => a.ReleaseYear)
                                        .ThenBy(a => a.Name)
                                        .ToList();
                    foreach (var album in albums)
                    {
                        performerView.Albums.Add(new AlbumViewModel { Album = album });
                    }
                }
                // Otherwise, --filter out albums to show-- according to album search string
                else
                {
                    albums = perf.Albums.Where(a => a.Name.ToUpper().Contains(AlbumPattern.ToUpper()))
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
                performerView.AlbumCollectionRate = CalculateAlbumCollectionRate( albums );

                // Finally, add fully created performer view model to the list
                performers.Add( performerView );
            }
        }

        /// <summary>
        /// Select performers whose name starts with string FirstLetter (or not a letter - "Other" case)
        /// </summary>
        /// <param name="letter">The first letter of a performer's name ("A", "B", "C", ..., "Z") or "Other"</param>
        public void SelectPerformersByFirstLetter()
        {
            using (var context = new MusCatEntities())
            {
                IQueryable<Performer> performersSelected;

                // query can be of two kinds:

                // 1) single letters ('A', 'B', 'C', ..., 'Z')
                if (FirstLetter.Length == 1)
                {
                    performersSelected = from p in context.Performers.Include("Country").Include("Albums")
                                         where p.Name.ToUpper().StartsWith(FirstLetter)
                                         orderby p.Name
                                         select p;
                }
                // 2) The "Other" option
                //    (all performers whose name doesn't start with capital English letter, e.g. "10CC", "Пикник", etc.)
                else
                {
                    performersSelected = from p in context.Performers.Include("Country").Include("Albums")
                                         where p.Name.ToUpper().Substring(0, 1).CompareTo("A") < 0 ||
                                               p.Name.ToUpper().Substring(0, 1).CompareTo("Z") > 0
                                         orderby p.Name
                                         select p;
                }

                if (Filter != PerformerFilters.FilteredByFirstLetter)
                {
                    Filter = PerformerFilters.FilteredByFirstLetter;
                    SelectedPage = 0;
                }

                MakePerformerViewModels( performersSelected );
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
                var performersSelected = from p in context.Performers.Include("Country").Include("Albums")
                                         where p.Name.ToUpper().Contains(PerformerPattern.ToUpper())
                                         orderby p.Name
                                         select p;

                if (Filter != PerformerFilters.FilteredByPattern)
                {
                    SelectedPage = 0;
                    Filter = PerformerFilters.FilteredByPattern;
                }

                MakePerformerViewModels( performersSelected );

                // deselect all buttons in upper navigation panel
                ResetButtons();
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
                var performersSelected = context.Performers.Include("Country")
                                                .Where(p => p.Albums
                                                .Where(a => a.Name.Contains(AlbumPattern))
                                                .Count() > 0)
                                                .OrderBy(p => p.Name);

                if (Filter != PerformerFilters.FilteredByAlbumPattern)
                {
                    SelectedPage = 0;
                    Filter = PerformerFilters.FilteredByAlbumPattern;
                }

                MakePerformerViewModels(performersSelected);

                // deselect all buttons in upper navigation panel
                ResetButtons();
            }
        }

        public void ViewSelectedPerformer()
        {
            if (SelectedPerformer == null)
            {
                MessageBox.Show("Please select performer to edit!");
                return;
            }

            PerformerWindow perfWindow = new PerformerWindow();
            perfWindow.DataContext = SelectedPerformer;
            perfWindow.Show();
        }

        public void EditPerformer()
        {
            if (SelectedPerformer == null)
            {
                MessageBox.Show( "Please select performer to edit!" );
                return;
            }

            EditPerformerViewModel viewmodel = new EditPerformerViewModel( SelectedPerformer );
            EditPerformerWindow perfWindow = new EditPerformerWindow();
            perfWindow.DataContext = viewmodel;
            perfWindow.Show();
        }

        public void AddPerformer()
        {
            // set initial information of a newly added performer
            Performer perf = new Performer { Name = "Unknown performer" };

            using (var context = new MusCatEntities())
            {
                perf.ID = context.Performers.Max(p => p.ID) + 1;
                context.Performers.Add(perf);
                context.SaveChanges();

                EditPerformerViewModel viewmodel = new EditPerformerViewModel(new PerformerViewModel { Performer = perf });
                EditPerformerWindow perfWindow = new EditPerformerWindow();
                perfWindow.DataContext = viewmodel;
                perfWindow.ShowDialog();

                // Update current list of performers only if the first letter of a newly added performer
                // is the first letter currently selectd in upper navigation panel
                if (perf.Name.ToUpper()[0] == performers[0].Performer.Name.ToUpper()[0])
                {
                    // TODO: insert performer at the right place in the list
                    performers.Add(new PerformerViewModel { Performer = perf });
                }

                MessageBox.Show("Performer was succesfully added to database");
            }
        }

        public void RemoveSelectedPerformer()
        {
            var perf = SelectedPerformer.Performer;

            if (perf == null)
            {
                MessageBox.Show("Please select performer to remove");
            }
            else if (MessageBox.Show(string.Format("Are you sure you want to delete '{0}'?",
                                        perf.Name),
                                        "Confirmation",
                                        MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                using (var context = new MusCatEntities())
                {
                    //context.DeletePerformerByID((int?)(perf.ID));                         // option1: stored procedure
                    var performerToDelete = context.Performers                              // option2: LINQ query
                                                   .Where(p => p.ID == perf.ID)
                                                   .SingleOrDefault<Performer>();
                    context.Performers.Remove(performerToDelete);
                    context.SaveChanges();

                    performers.Remove(SelectedPerformer);
                }
            }
        }

        /// <summary>
        /// Handler of the RateUpdated event
        /// </summary>
        /// <param name="sender">Performer whose album has been updated</param>
        /// <param name="e">Empty</param>
        public void AlbumRateUpdated(object sender, EventArgs e)
        {
            var perf = sender as Performer;
            foreach ( var performerView in performers )
            {
                if (performerView.Performer.ID == perf.ID)
                {
                    performerView.AlbumCollectionRate = CalculateAlbumCollectionRate(perf.Albums.ToList());
                    return;
                }
            }
        }

        public void ViewSelectedAlbum()
        {
            if (SelectedPerformer == null || SelectedPerformer.SelectedAlbum == null)
            {
                MessageBox.Show("Please select album to edit!");
                return;
            }

            // lazy load songs of selected album
            SelectedPerformer.SelectedAlbum.LoadSongs();

            AlbumWindow albumWindow = new AlbumWindow();
            var albumViewModel = new AlbumPlaybackViewModel( SelectedPerformer.SelectedAlbum );

            // TODO: unsubscribe to avoid memory leak!
            albumViewModel.RateUpdated += AlbumRateUpdated;
            albumWindow.DataContext = albumViewModel;
            albumWindow.Show();
        }

        public void EditAlbum()
        {
            if (SelectedPerformer == null || SelectedPerformer.SelectedAlbum == null)
            {
                MessageBox.Show("Please select album to edit!");
                return;
            }

            // lazy load songs of selected album
            SelectedPerformer.SelectedAlbum.LoadSongs();

            EditAlbumWindow albumWindow = new EditAlbumWindow();
            albumWindow.DataContext = new EditAlbumViewModel( SelectedPerformer.SelectedAlbum );
            albumWindow.ShowDialog();

            SelectedPerformer.AlbumCollectionRate =
                    CalculateAlbumCollectionRate(SelectedPerformer.Performer.Albums.ToList());
        }

        public void AddAlbum()
        {
            if (SelectedPerformer == null)
            {
                MessageBox.Show("Please select performer!");
                return;
            }

            // set initial information of a newly added album
            Album a = new Album {   Name = "New Album",
                                    TotalTime = "00:00",
                                    PerformerID = SelectedPerformer.Performer.ID,
                                    ReleaseYear = (short)DateTime.Now.Year };

            using (var context = new MusCatEntities())
            {
                a.ID = context.Albums.Max(alb => alb.ID) + 1;
                context.Albums.Add(a);
                context.SaveChanges();

                a.Performer = SelectedPerformer.Performer;

                AlbumViewModel albumView = new AlbumViewModel { Album = a };
                
                EditAlbumWindow editAlbum = new EditAlbumWindow();
                editAlbum.DataContext = new EditAlbumViewModel( albumView );
                editAlbum.ShowDialog();

                // TODO: insert at the right place
                SelectedPerformer.Albums.Add(albumView);

                // to update view
                SelectedPerformer.AlbumCount = SelectedPerformer.Albums.Count();
                SelectedPerformer.AlbumCollectionRate = 
                    CalculateAlbumCollectionRate( SelectedPerformer.Performer.Albums.ToList() );
            }
        }

        public void RemoveSelectedAlbum()
        {
            if (SelectedPerformer == null)
            {
                MessageBox.Show("Please select performer first!");
            }

            var selectedAlbum = SelectedPerformer.SelectedAlbum;

            if (selectedAlbum == null)
            {
                MessageBox.Show("Please select album to remove");
            }
            else if (MessageBox.Show(string.Format("Are you sure you want to delete\n '{0}' \nby '{1}'?",
                                            selectedAlbum.Album.Name, selectedAlbum.Album.Performer.Name),
                                            "Confirmation",
                                            MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                using (var context = new MusCatEntities())
                {
                    //context.DeleteAlbumByID((int?)(selectedAlbum.ID));               // option1: stored procedure
                    var albumToDelete = context.Albums                                 // option2: LINQ query
                                               .Where(a => a.ID == selectedAlbum.Album.ID)
                                               .SingleOrDefault<Album>();
                    context.Albums.Remove(albumToDelete);
                    context.SaveChanges();

                    SelectedPerformer.Albums.Remove(selectedAlbum);
                    
                    // to update view
                    SelectedPerformer.AlbumCount = SelectedPerformer.Albums.Count();
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
