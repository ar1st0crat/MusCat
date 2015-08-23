using MusCatalog.Model;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.ComponentModel;
using System.Windows.Data;
using MusCatalog.ViewModel;


namespace MusCatalog.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
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
            //FillPerformersListByFirstLetter("A");
            prevButton = (LetterButton)lettersPanel.Children[0];
            prevButton.Select();
        }

        /// <summary>
        /// Populate the list of performers whose name starts with specific letter (or not a letter - "other" case)
        /// </summary>
        /// <param name="letter">The first letter of a performer's name ("A", "B", "C", ..., "Z") or "Other"</param>
        private void FillPerformersListByFirstLetter( string letter )
        {
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
            ((MainViewModel)DataContext).LoadPerformersByName( this.PerformerSearch.Text );

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

        /// <summary>
        /// Select performers having albums whose name contains search pattern (specified in lower navigation panel)
        /// </summary>
        private void AlbumSearchClick(object sender, MouseButtonEventArgs e)
        {
            ((MainViewModel)DataContext).LoadPerformersByAlbumName(this.AlbumSearch.Text);

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
                
        /// <summary>
        /// Remove performer selected in the list of performers
        /// </summary>
        private void RemoveSelectedPerformer()
        {
            ((MainViewModel)DataContext).RemoveSelectedPerformer();
        }

        /// <summary>
        /// Remove album selected in the list of albums
        /// </summary>
        private void RemoveSelectedAlbum()
        {
            ((MainViewModel)DataContext).RemoveSelectedAlbum();
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

            ((MainViewModel)DataContext).LoadPerformers( curLetter );
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

        private void SelectedAlbumsMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            AlbumWindow albumWindow = new AlbumWindow( ((MainViewModel)DataContext).SelectedPerformer.SelectedAlbum.Album );
            albumWindow.Show();
        }

        #region KeyDown handlers

        /// <summary>
        /// KeyDown handler (for performers):
        ///     Enter   -> View performer info
        ///     Del     -> Remove performer and asoociated discography
        ///     F4      -> Edit performer
        /// </summary>
        private void PerformerlistPreviewKeyDown(object sender, KeyEventArgs e)
        {
            // if some album is selected then propagate event handling to albumlist
            if ( ((MainViewModel)DataContext).SelectedPerformer == null ||
                 ((MainViewModel)DataContext).SelectedPerformer.SelectedAlbum.Album != null )
            {
                return;
            }

            Performer perf = Performerlist.SelectedItem as Performer;

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
                AlbumWindow albumWindow = new AlbumWindow( ((MainViewModel)DataContext).SelectedPerformer.SelectedAlbum.Album );
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
            ((MainViewModel)DataContext).AddPerformer();
        }

        private void MenuEditPerformerClick(object sender, RoutedEventArgs e)
        {
            ((MainViewModel)DataContext).EditPerformer();
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
            if ( this.Performerlist.SelectedIndex < 0 )
            {
                MessageBox.Show( "Please choose the performer!" );
                return;
            }

            Performer perf = this.Performerlist.SelectedItem as Performer;

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
            if (((MainViewModel)DataContext).SelectedPerformer.SelectedAlbum.Album != null)
            {
                EditAlbumWindow albumWindow = new EditAlbumWindow(((MainViewModel)DataContext).SelectedPerformer.SelectedAlbum.Album);
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
