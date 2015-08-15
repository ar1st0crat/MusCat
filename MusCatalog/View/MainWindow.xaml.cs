using MusCatalog.Model;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.ComponentModel;
using System.Windows.Data;


namespace MusCatalog.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //
        LetterButton prevButton = null;
        LetterButton pressedButton = null;
        //
        string curLetter = "A";
        //
        Album selectedAlbum = null;

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
                    performers = from p in context.Performers
                                 where p.Name.StartsWith(letter)
                                 orderby p.Name
                                 select p;
                }
                // The "Other" option
                else
                {
                    performers = from p in context.Performers
                                     where p.Name.Substring(0, 1).CompareTo("A") < 0 || p.Name.Substring(0, 1).CompareTo("Z") > 0
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
        /// TODO
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
        /// TODO
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
        /// Main window initialization
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            FileLocator.Initialize();

            foreach (char c in "ABCDEFGHIJKLMNOPQRSTUVWXYZ")
            {
                LetterButton b = new LetterButton( c.ToString() );
                b.Click += LetterClick;

                lettersPanel.Children.Add( b );
            }

            LetterButton bOther = new LetterButton( "Other", 70 );
            bOther.Click += LetterClick;
            lettersPanel.Children.Add( bOther );

            // Start with the "A-letter"-list
            prevButton = (LetterButton)lettersPanel.Children[0];
            prevButton.Select();
            FillPerformersListByFirstLetter("A");
        }


        /// <summary>
        /// TODO
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
        /// TODO
        /// </summary>
        private void NavigationClick(object sender, RoutedEventArgs e)
        {
            var b = sender as TextBlock;
            MessageBox.Show( b.Name );
        }
               

        /// <summary>
        /// TODO
        /// </summary>
        private void PerflistPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (selectedAlbum != null)      // if some album is selected then propagate event handling to albumlist
                return;

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
        /// TODO
        /// </summary>
        private void PerflistMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount > 1)
            {
                if (!Clipboard.ContainsImage())
                {
                    MessageBox.Show("No image in clipboard!");
                    return;
                }

                Performer p = perflist.SelectedItem as Performer;

                var filepaths = FileLocator.MakePathImagePerformer( p );
                string filepath = filepaths[0];

                if ( filepaths.Count > 1 )
                {
                    ChoiceWindow choice = new ChoiceWindow();
                    choice.SetChoiceList( filepaths );
                    choice.ShowDialog();

                    if (choice.ChoiceResult == "")
                    {
                        return;
                    }
                    filepath = choice.ChoiceResult;
                }
                

                Directory.CreateDirectory( Path.GetDirectoryName( filepath ) );

                // first check if file already exists
                if (File.Exists(filepath))
                {
                    File.Delete(filepath);
                }

                var image = Clipboard.GetImage();
                try
                {
                    using (var fileStream = new FileStream(filepath, FileMode.Create))
                    {
                        BitmapEncoder encoder = new JpegBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create(image));
                        encoder.Save(fileStream);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }


        /// <summary>
        /// Synchronize currently selected album with corresponding variable
        /// </summary>
        private void AlbumSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var lb = sender as ListBox;
            selectedAlbum = lb.SelectedItem as Album;
        }

        /// <summary>
        /// TODO
        /// </summary>
        private void SelectedAlbumsMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            AlbumWindow albumWindow = new AlbumWindow(selectedAlbum);
            albumWindow.Show();
        }

        /// <summary>
        /// TODO
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


        //
        // ================== Menu click handlers boilerplate code ========================
        //
        private void MenuAddPerformerClick(object sender, RoutedEventArgs e)
        {
            //PerformerWindow perfWindow = new PerformerWindow();
            //perfWindow.ShowDialog();
            FillPerformersListByFirstLetter( curLetter );
        }

        private void MenuEditPerformerClick(object sender, RoutedEventArgs e)
        {
            PerformerWindow perfWindow = new PerformerWindow();
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
            Performer perf = this.perflist.SelectedItem as Performer;
            Album a = new Album();
            a.Name = "Unknown";
            a.ReleaseYear = 2000;
            a.Rate = 9;
            a.TotalTime = "40:02";
            a.PerformerID = perf.ID;
            
            using (var context = new MusCatEntities())
            {
                a.ID = context.Albums.Max( alb => alb.ID ) + 1;
                context.Albums.Add(a);
                context.SaveChanges();

                a.Performer = perf;
                perf.Albums.Add(a);
                
                ICollectionView view = CollectionViewSource.GetDefaultView(perf.Albums);
                view.Refresh();
            }
        }

        private void MenuEditAlbumClick(object sender, RoutedEventArgs e)
        {
            if (selectedAlbum != null)
            {
                AlbumWindow albumWindow = new AlbumWindow(selectedAlbum);
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

        private void PerformerSearchClick(object sender, MouseButtonEventArgs e)
        {
            using (var context = new MusCatEntities())
            {
                var performers = from p in context.Performers
                                 where p.Name.ToUpper().Contains(this.PerformerSearch.Text.ToUpper())
                                 orderby p.Name
                                 select p;

                // ============================= order each performer's albums by year of release, then by name (in collection view)
                foreach (var perf in performers)
                {
                    ICollectionView view = CollectionViewSource.GetDefaultView(perf.Albums);
                    view.SortDescriptions.Add(new SortDescription("ReleaseYear", ListSortDirection.Ascending));
                    view.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
                }
                this.perflist.ItemsSource = performers.ToList();
                this.perflist.SelectedIndex = -1;


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

        private void AlbumSearchClick(object sender, MouseButtonEventArgs e)
        {
            using (var context = new MusCatEntities())
            {
                var performers = context.Performers.Where(p => p.Albums
                                                   .Where(a => a.Name.Contains(this.AlbumSearch.Text))
                                                   .Count() > 0);

                foreach (var perf in performers)
                {
                    var filteredAlbums = perf.Albums.Where( a => a.Name.ToUpper().Contains(this.AlbumSearch.Text.ToUpper()) ).ToList();
                    perf.Albums.Clear();
                    foreach (var album in filteredAlbums)
                    {
                        perf.Albums.Add( album );
                    }
                }

                // ============================= order each performer's albums by year of release, then by name (in collection view)
                foreach (var perf in performers)
                {
                    ICollectionView view = CollectionViewSource.GetDefaultView(perf.Albums);
                    view.SortDescriptions.Add(new SortDescription("ReleaseYear", ListSortDirection.Ascending));
                    view.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
                }
                this.perflist.ItemsSource = performers.ToList();
                this.perflist.SelectedIndex = -1;


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
    }
}
