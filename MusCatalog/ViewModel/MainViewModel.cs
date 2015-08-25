using MusCatalog.Model;
using MusCatalog.View;
using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using System.Windows.Controls.Primitives;
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
            set
            {
                performers = value;
                RaisePropertyChanged("Performers");
            }
        }

        public PerformerViewModel SelectedPerformer { get; set; }

        // Letters in upper navigation panel
        public ObservableCollection<LetterButton> LetterCollection { get; set; }

        // References to "selected" and "deselected" buttons in upper navigation panel
        private LetterButton prevButton = null;
        private LetterButton pressedButton = null;

        // Letters in upper navigation panel
        public ObservableCollection<UIElement> PageCollection { get; set; }

        public string PerformerPattern { get; set; }
        public string AlbumPattern { get; set; }
        public string FirstLetter { get; set; }
        

        public MainViewModel()
        {
            PageCollection = new ObservableCollection<UIElement>();
            CreateUpperNavigationPanel();
            SelectPerformersByFirstLetter();
        }

        #region Upper navigation panel

        public void CreateUpperNavigationPanel()
        {
            LetterCollection = new ObservableCollection<LetterButton>();

            // create the upper navigation panel
            foreach (char c in "ABCDEFGHIJKLMNOPQRSTUVWXYZ")
            {
                LetterButton b = new LetterButton(c.ToString());
                b.Click += LetterClick;
                LetterCollection.Add(b);
            }

            LetterButton bOther = new LetterButton("Other", 70);
            bOther.Click += LetterClick;
            LetterCollection.Add(bOther);

            // Start with the "A-letter"-list
            prevButton = LetterCollection[0];
            prevButton.Select();

            FirstLetter = "A";
        }

        /// <summary>
        /// Upper navigation panel click handler
        /// </summary>
        private void LetterClick(object sender, RoutedEventArgs e)
        {
            pressedButton = (LetterButton)sender;
            prevButton.DeSelect();
            pressedButton.Select();
            prevButton = pressedButton;
            FirstLetter = pressedButton.Content.ToString();
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

        PerformerFilters Filter = PerformerFilters.FilteredByFirstLetter;

        int SelectedPage = 0;
        int PerformersPerPage = 5;

        private void NavigatePage(object sender, RoutedEventArgs e)
        {
            SelectedPage = (int)((TextBlock)sender).Tag;

            switch (Filter)
            {
                case PerformerFilters.FilteredByFirstLetter:
                    SelectPerformersByFirstLetter();
                    break;
                case PerformerFilters.FilteredByPattern:
                    SelectPerformersByName();
                    break;
                case PerformerFilters.FilteredByAlbumPattern:
                    SelectPerformersByAlbumName();
                    break;
            }
        }

        public void FillPerformerList( IQueryable<Performer> performersSelected )
        {
            performers.Clear();

            //
            PageCollection.Clear();
            var total = Math.Ceiling( (double)performersSelected.Count() / PerformersPerPage );
            if (total > 1)
            {
                for (int i = 0; i < total; i++)
                {
                    var nb = new TextBlock();
                    nb.Tag = i;
                    nb.Text = (i+1).ToString();
                    nb.TextDecorations = TextDecorations.Underline;
                    nb.Cursor = Cursors.Hand;
                    nb.Margin = new Thickness(5, 0, 0, 0);
                    nb.Foreground = Brushes.Yellow;
                    nb.Background = Brushes.Transparent;
                    nb.MouseDown += NavigatePage;

                    PageCollection.Add( nb );
                }

                ((TextBlock)PageCollection.ElementAt(SelectedPage)).TextDecorations = null;
            }
            //

            var performersPaged = performersSelected.Skip( SelectedPage * PerformersPerPage )
                                                    .Take( PerformersPerPage )
                                                    .ToList(); 

            foreach (var perf in performersPaged)
            {
                var pvModel = new PerformerViewModel { Performer = perf };

                /// Fill performer'salbumlist
                List<Album> albums;

                // If no filter is specified, then copy all albums to PerformerViewModel
                if (AlbumPattern == null || AlbumPattern == "")
                {
                    albums = perf.Albums.OrderBy(a => a.ReleaseYear)
                                        .ThenBy(a => a.Name)
                                        .ToList();
                    foreach (var album in albums)
                    {
                        pvModel.Albums.Add(new AlbumViewModel { Album = album });
                    }
                }
                // Otherwise, filter out albums to show according to album search string
                else
                {
                    albums = perf.Albums.Where(a => a.Name.ToUpper().Contains(AlbumPattern.ToUpper()))
                                                                    .OrderBy(a => a.ReleaseYear)
                                                                    .ThenBy(a => a.Name)
                                                                    .ToList();
                    foreach (var album in albums)
                    {
                        pvModel.Albums.Add(new AlbumViewModel { Album = album });
                    }
                }

                /// The total rate of performer is calculated based on the following statistics of album rates:
                /// 
                ///     if the number of albums is more than 2 then the worst rate and the best rate are discarded
                ///     and the total rate is an average of remaining rates
                ///     
                ///     otherwise - the total rate is simply an average of album rates
                ///     
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

                pvModel.AlbumCount = albums.Count();
                pvModel.AlbumCollectionRate = avgRate;

                performers.Add(pvModel);
            }
        }

        /// <summary>
        /// Populate the list of performers whose name starts with specific letter (or not a letter - "other" case)
        /// </summary>
        /// <param name="letter">The first letter of a performer's name ("A", "B", "C", ..., "Z") or "Other"</param>
        public void SelectPerformersByFirstLetter()
        {
            using (var context = new MusCatEntities())
            {
                IQueryable<Performer> performersSelected;

                // single letters ('A', 'B', 'C', ..., 'Z')
                if (FirstLetter.Length == 1)
                {
                    performersSelected = from p in context.Performers.Include("Country").Include("Albums")
                                 where p.Name.ToUpper().StartsWith(FirstLetter)
                                 orderby p.Name
                                 select p;
                }
                // The "Other" option
                // (all performers whose name doesn't start with capital English letter, e.g. "10CC", "Пикник", etc.)
                else
                {
                    performersSelected = from p in context.Performers.Include("Country").Include("Albums")
                                 where p.Name.ToUpper().Substring(0, 1).CompareTo("A") < 0 ||
                                       p.Name.ToUpper().Substring(0, 1).CompareTo("Z") > 0
                                 orderby p.Name
                                 select p;
                }

                Filter = PerformerFilters.FilteredByFirstLetter;
                FillPerformerList( performersSelected );
            }
        }

        /// <summary>
        /// Select performers whose name contains the search pattern (specified in lower navigation panel)
        /// </summary>
        public void SelectPerformersByName()
        {
            using (var context = new MusCatEntities())
            {
                var performersSelected = from p in context.Performers.Include("Country").Include("Albums")
                                 where p.Name.ToUpper().Contains(PerformerPattern.ToUpper())
                                 orderby p.Name
                                 select p;

                SelectedPage = 0;
                Filter = PerformerFilters.FilteredByPattern;
                FillPerformerList( performersSelected );

                ResetButtons();
            }
        }

        /// <summary>
        /// Select performers having albums whose name contains search pattern (specified in lower navigation panel)
        /// </summary>
        public void SelectPerformersByAlbumName()
        {
            using (var context = new MusCatEntities())
            {
                var performersSelected = context.Performers.Include("Country")
                                                   .Where(p => p.Albums
                                                   .Where(a => a.Name.Contains(AlbumPattern))
                                                   .Count() > 0)
                                                   .OrderBy(p => p.Name);

                SelectedPage = 0;
                Filter = PerformerFilters.FilteredByAlbumPattern;
                FillPerformerList(performersSelected);

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

            PerformerWindow perfWindow = new PerformerWindow();// SelectedPerformer );
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

                // TODO: do this only if the first letter is current letter
                if (perf.Name.ToUpper()[0] == performers[0].Performer.Name.ToUpper()[0])
                {
                    performers.Add(new PerformerViewModel { Performer = perf });
                }

                MessageBox.Show("Performer was succesfully added to database");
            }
        }

        /// <summary>
        /// cRud: Remove performer selected in the list of performers
        /// </summary>
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
            albumWindow.DataContext = new AlbumPlaybackViewModel( SelectedPerformer.SelectedAlbum );
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
            albumWindow.Show();
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

                AlbumViewModel avModel = new AlbumViewModel { Album = a, Songs = new ObservableCollection<Song>() };
                
                // TODO: insert at the right place
                SelectedPerformer.Albums.Add( avModel );

                EditAlbumWindow editAlbum = new EditAlbumWindow();
                editAlbum.DataContext = new EditAlbumViewModel( avModel );
                editAlbum.ShowDialog();
            }
        }

        /// <summary>
        /// Remove album selected in the list of albums
        /// </summary>
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
