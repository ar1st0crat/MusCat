using MusCatalog.Model;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.ComponentModel;
using System.Windows.Data;


namespace MusCatalog.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Album selectedAlbum = null;

        // References to "selected" and "deselected" buttons in upper navigation panel
        LetterButton prevButton = null;
        LetterButton pressedButton = null;
        string curLetter = "A";

        /// <summary>
        /// Main window initialization
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            FileLocator.Initialize();

            // create the upper navigation panel
            foreach (char c in "ABCDEFGHIJKLMNOPQRSTUVWXYZ")
            {
                LetterButton b = new LetterButton(c.ToString());
                b.Click += LetterClick;

                lettersPanel.Children.Add(b);
            }

            LetterButton bOther = new LetterButton("Other", 70);
            bOther.Click += LetterClick;
            lettersPanel.Children.Add(bOther);

            // Start with the "A-letter"-list
            FillPerformersListByFirstLetter("A");
            prevButton = (LetterButton)lettersPanel.Children[0];
            prevButton.Select();
        }

        /// <summary>
        /// Populate the list of performers whose name starts with specific letter (or not a letter - "other" case)
        /// </summary>
        /// <param name="letter">The first letter of a performer's name ("A", "B", "C", ..., "Z") or "Other"</param>
        private void FillPerformersListByFirstLetter( string letter )
        {
            using (var context = new MusCatEntities())
            {
                IQueryable<Performer> performers;

                // single letters ('A', 'B', 'C', ..., 'Z')
                if (letter.Length == 1)                             
                {
                    performers = from p in context.Performers.Include("Country")
                                 where p.Name.ToUpper().StartsWith(letter)
                                 orderby p.Name
                                 select p;
                }
                // The "Other" option
                // (all performers whose name doesn't start with capital English letter, e.g. "10CC", "Пикник", etc.)
                else
                {
                    performers = from p in context.Performers.Include("Country")
                                     where p.Name.ToUpper().Substring(0, 1).CompareTo("A") < 0 || 
                                           p.Name.ToUpper().Substring(0, 1).CompareTo("Z") > 0
                                     orderby p.Name
                                     select p;
                }

                // ============================= order each performer's albums by year of release, then by name (in collection view)
                foreach (var perf in performers)
                {
                    ICollectionView view = CollectionViewSource.GetDefaultView(perf.Albums);
                    view.SortDescriptions.Add( new SortDescription("ReleaseYear", ListSortDirection.Ascending));
                    view.SortDescriptions.Add( new SortDescription("Name", ListSortDirection.Ascending));
                }

                this.perflist.ItemsSource = performers.ToList();
                this.perflist.SelectedIndex = -1;
            }

            
            // ============================================================================
            // REFACTOR! REFACTOR! REFACTOR! REFACTOR! REFACTOR! REFACTOR! REFACTOR! 
            // ============================================================================
            this.NavigationPanel.Children.Clear();

            for (int i = 0; i < 5; i++)
            {
                var nb = new TextBlock();
                nb.Name = "nb" + (i * 7 + 1).ToString();
                nb.Text = (i+1).ToString();
                nb.TextDecorations = TextDecorations.Underline;
                nb.Cursor = Cursors.Hand;
                nb.Margin = new Thickness(5, 0, 0, 0);
                nb.Foreground = Brushes.Yellow;
                nb.Background = Brushes.Transparent;
                nb.MouseDown += NavigationClick;
                this.NavigationPanel.Children.Add(nb);
            }

            ((TextBlock)this.NavigationPanel.Children[0]).TextDecorations = null;
        }

        /// <summary>
        /// Select performers whose name contains the search pattern (specified in lower navigation panel)
        /// </summary>
        private void PerformerSearchClick(object sender, MouseButtonEventArgs e)
        {
            using (var context = new MusCatEntities())
            {
                var performers = from p in context.Performers.Include("Country")
                                 where p.Name.ToUpper().Contains(this.PerformerSearch.Text.ToUpper())
                                 orderby p.Name
                                 select p;

                // order each performer's albums by year of release, then by name (in collection view)
                foreach (var perf in performers)
                {
                    ICollectionView view = CollectionViewSource.GetDefaultView(perf.Albums);
                    view.SortDescriptions.Add(new SortDescription("ReleaseYear", ListSortDirection.Ascending));
                    view.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
                }
                this.perflist.ItemsSource = performers.ToList();
                this.perflist.SelectedIndex = -1;

                // deselect all buttons in upper navigation panel
                if (pressedButton != null)
                {
                    pressedButton.DeSelect();
                }
                else
                {
                    prevButton.DeSelect();
                }
            }
        }

        /// <summary>
        /// Select performers having albums whose name contains search pattern (specified in lower navigation panel)
        /// </summary>
        private void AlbumSearchClick(object sender, MouseButtonEventArgs e)
        {
            using (var context = new MusCatEntities())
            {
                var performers = context.Performers.Include("Country")
                                                   .Where(p => p.Albums
                                                   .Where(a => a.Name.Contains(this.AlbumSearch.Text))
                                                   .Count() > 0);

                foreach (var perf in performers)
                {
                    var filteredAlbums = perf.Albums.Where(a => a.Name.ToUpper().Contains(this.AlbumSearch.Text.ToUpper())).ToList();
                    perf.Albums.Clear();
                    foreach (var album in filteredAlbums)
                    {
                        perf.Albums.Add(album);
                    }
                }

                // order each performer's albums by year of release, then by name (in collection view)
                foreach (var perf in performers)
                {
                    ICollectionView view = CollectionViewSource.GetDefaultView(perf.Albums);
                    view.SortDescriptions.Add(new SortDescription("ReleaseYear", ListSortDirection.Ascending));
                    view.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
                }
                this.perflist.ItemsSource = performers.ToList();
                this.perflist.SelectedIndex = -1;

                // deselect all buttons in upper navigation panel
                if (pressedButton != null)
                {
                    pressedButton.DeSelect();
                }
                else
                {
                    prevButton.DeSelect();
                }
            }
        }
                
        /// <summary>
        /// Remove performer selected in the list of performers
        /// </summary>
        private void RemoveSelectedPerformer()
        {
            Performer perf = this.perflist.SelectedItem as Performer;

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
                    context.Performers.Remove( performerToDelete );
                    context.SaveChanges();

                    // update collection
                    FillPerformersListByFirstLetter( curLetter );
                }
            }
        }

        /// <summary>
        /// Remove album selected in the list of albums
        /// </summary>
        private void RemoveSelectedAlbum()
        {
            if ( selectedAlbum == null )
            {
                MessageBox.Show( "Please select album to remove" );
            }
            else if (MessageBox.Show(string.Format("Are you sure you want to delete\n '{0}' \nby '{1}'?",
                                            selectedAlbum.Name, selectedAlbum.Performer.Name),
                                            "Confirmation",
                                            MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                using (var context = new MusCatEntities())
                {
                    //context.DeleteAlbumByID((int?)(selectedAlbum.ID));               // option1: stored procedure
                    var albumToDelete = context.Albums                                 // option2: LINQ query
                                               .Where(a => a.ID == selectedAlbum.ID)
                                               .SingleOrDefault<Album>();
                    context.Albums.Remove( albumToDelete );
                    context.SaveChanges();

                    Performer perf = this.perflist.SelectedItem as Performer;
                    perf.Albums.Remove( selectedAlbum );
                    
                    // update selected albums collection view
                    ICollectionView view = CollectionViewSource.GetDefaultView(perf.Albums);
                    view.Refresh();
                }
            }
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

            curLetter = pressedButton.Content.ToString();
            FillPerformersListByFirstLetter( curLetter );
        }

        /// <summary>
        /// Lower navigation panel click handler
        /// </summary>
        private void NavigationClick(object sender, RoutedEventArgs e)
        {
            var b = sender as TextBlock;
            MessageBox.Show( b.Name );
        }

        /// <summary>
        /// Synchronize currently selected album with corresponding variable
        /// </summary>
        private void AlbumSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var lb = sender as ListBox;
            selectedAlbum = lb.SelectedItem as Album;
        }

        private void SelectedAlbumsMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            AlbumWindow albumWindow = new AlbumWindow(selectedAlbum);
            albumWindow.Show();
        }

        #region KeyDown handlers

        /// <summary>
        /// KeyDown handler (for performers):
        ///     Enter   -> View performer info
        ///     Del     -> Remove performer and asoociated discography
        ///     F4      -> Edit performer
        /// </summary>
        private void PerflistPreviewKeyDown(object sender, KeyEventArgs e)
        {
            // if some album is selected then propagate event handling to albumlist
            if (selectedAlbum != null)
            {
                return;
            }

            Performer perf = perflist.SelectedItem as Performer;

            if ( e.Key == Key.Enter )
            {
                PerformerWindow performerWindow = new PerformerWindow( perf );
                performerWindow.Show();
            }
            else if ( e.Key == Key.Delete )
            {
                RemoveSelectedPerformer();
            }
        }

        /// <summary>
        /// KeyDown handler (for albums):
        ///     Enter   -> View album info
        ///     Del     -> Remove album
        ///     F4      -> Edit album
        /// </summary>
        private void SelectedAlbumKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                AlbumWindow albumWindow = new AlbumWindow(selectedAlbum);
                albumWindow.Show();
            }
            else if (e.Key == Key.Delete)
            {
                RemoveSelectedAlbum();
            }
        }

        #endregion
        
        #region Menu click handlers boilerplate code
        
        private void MenuAddPerformerClick(object sender, RoutedEventArgs e)
        {
            // set initial information of a newly added performer
            Performer p = new Performer { Name = "Unknown performer" };
            
            using (var context = new MusCatEntities())
            {
                p.ID = context.Performers.Max( perf => perf.ID ) + 1;
                context.Performers.Add( p );
                context.SaveChanges();

                EditPerformerWindow editPerformer = new EditPerformerWindow( p );
                editPerformer.ShowDialog();

                // TODO: do this only if the first letter is current letter
                FillPerformersListByFirstLetter( curLetter );
            }
        }

        private void MenuEditPerformerClick(object sender, RoutedEventArgs e)
        {
            if (this.perflist.SelectedIndex < 0)
            {
                MessageBox.Show( "Please choose performer to edit!" );
                return;
            }

            var perf = this.perflist.SelectedItem as Performer;

            EditPerformerWindow perfWindow = new EditPerformerWindow( perf );
            perfWindow.ShowDialog();
        }

        private void MenuRemovePerformerClick(object sender, RoutedEventArgs e)
        {
            RemoveSelectedPerformer();
        }

        private void MenuMusiciansClick(object sender, RoutedEventArgs e)
        {
            //MisiciansWindow musicians = new MusiciansWindow();
            //musicians.ShowDialog();
        }

        private void MenuAddAlbumClick(object sender, RoutedEventArgs e)
        {
            if ( this.perflist.SelectedIndex < 0 )
            {
                MessageBox.Show( "Please choose the performer!" );
                return;
            }

            Performer perf = this.perflist.SelectedItem as Performer;

            // set initial information of a newly added album
            Album a = new Album { Name="New Album", TotalTime="00:00", PerformerID=perf.ID, ReleaseYear=(short)DateTime.Now.Year };
            
            using (var context = new MusCatEntities())
            {
                a.ID = context.Albums.Max( alb => alb.ID ) + 1;
                context.Albums.Add(a);
                context.SaveChanges();

                a.Performer = perf;
                perf.Albums.Add(a);

                EditAlbumWindow editAlbum = new EditAlbumWindow( a );
                editAlbum.ShowDialog();

                ICollectionView view = CollectionViewSource.GetDefaultView(perf.Albums);
                view.Refresh();
            }
        }

        private void MenuEditAlbumClick(object sender, RoutedEventArgs e)
        {
            if (selectedAlbum != null)
            {
                EditAlbumWindow albumWindow = new EditAlbumWindow(selectedAlbum);
                albumWindow.Show();
            }
        }

        private void MenuRemoveAlbumClick(object sender, RoutedEventArgs e)
        {
            RemoveSelectedAlbum();
        }

        private void MenuStatsClick(object sender, RoutedEventArgs e)
        {
            //StatsWindow stats = new StatsWindow();
            //stats.Show();
        }

        private void MenuSettingsClick(object sender, RoutedEventArgs e)
        {
            //SettingsWindow settings = new SettingsWindow();
            //settings.ShowDialog();
        }
        
        private void MenuRadioClick(object sender, RoutedEventArgs e)
        {
            RadioPlayerWindow radio = new RadioPlayerWindow();
            radio.Show();
        }

        private void MenuHelpClick(object sender, RoutedEventArgs e)
        {
            //HelpWindow info = new HelpWindow();
            //info.ShowDialog();
        }

        #endregion
    }
}
