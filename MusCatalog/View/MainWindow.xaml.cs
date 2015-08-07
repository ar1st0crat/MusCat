using MusCatalog.Model;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


namespace MusCatalog.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        LetterButton prevButton = null;

        string curLetter = "A";

        /// <summary>
        /// Populate the list of performers whose name starts with specific letter (or not a letter - "other" case)
        /// </summary>
        /// <param name="letter">The first letter of a performer's name ("A", "B", "C", ..., "Z") or "Other"</param>
        private void FillPerformersListByFirstLetter( string letter )
        {
            using (var context = new MusCatEntities())
            {
                IQueryable<Performers> performers;
                                                
                if (letter.Length == 1)
                {
                    performers = from p in context.Performers.Include("Albums")
                                 where p.Performer.StartsWith(letter)
                                 orderby p.Performer
                                 select p;
                }
                else
                {
                    performers = from p in context.Performers.Include("Albums")
                                     where (p.Performer.Substring(0, 1).CompareTo("A") < 0 || p.Performer.Substring(0, 1).CompareTo("Z") > 0)
                                     orderby p.Performer
                                     select p;
                }

                // ====================================================== here we sort each preformer's albums by the year of release
                // ======================================================           Possible ways to write better code:
                // ======================================================               1) DataLoadOptions (failed so far)
                // ======================================================               2) select new { ... } - but will need to rewrite converters
                // ======================================================               3) CollectionViewSource (failed so far)
                foreach (var perf in performers)
                {
                    int albumCount = perf.Albums.Count;

                    var albs = perf.Albums.OrderBy(a => a.AYear).ToList();

                    perf.Albums.Clear();

                    foreach (var alb in albs)
                    {
                        perf.Albums.Add(alb);
                    }
                }
                // ====================================================================================================

                this.perflist.ItemsSource = performers.ToList();
                this.perflist.SelectedIndex = -1;
            }
        }

        
        /// <summary>
        /// TODO
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            MusCatFileLocator.Initialize();
            
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
            LetterButton pressedButton = (LetterButton)sender;

            prevButton.DeSelect();
            pressedButton.Select();
            
            prevButton = pressedButton;

            curLetter = pressedButton.Content.ToString();
            FillPerformersListByFirstLetter( curLetter );
        }
               

        /// <summary>
        /// TODO
        /// </summary>
        private void perflist_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch ( e.Key )
            {
                case Key.Delete:
                {
                    Performers perf = perflist.SelectedItem as Performers;

                    if (MessageBox.Show( string.Format( "Are you sure you want to delete '{0}'?", perf.Performer ),
                                            "Confirmation",
                                            MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        using (var context = new MusCatEntities())
                        {
                            context.DeleteByPID( (int?)(perf.PID) );
                            context.SaveChanges();
                        }
                        FillPerformersListByFirstLetter( curLetter );
                    }
                    break;
                }

                case Key.Enter:
                    perflist_MouseDoubleClick(sender, null);
                    break;
            }
        }

    
        /// <summary>
        /// TODO
        /// </summary>
        private void perflist_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount > 1)
            {
                Performers p = perflist.SelectedItem as Performers;
                PerformerWindow performerWindow = new PerformerWindow( p );
                performerWindow.Show();
            }
        }
        

        /// <summary>
        /// TODO
        /// </summary>
        private void SelectedAlbums_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ListBox lb = sender as ListBox;

            if (lb.Name == "SelectedAlbums")
            {
                Albums a = lb.SelectedItem as Albums;
                AlbumWindow albumWindow = new AlbumWindow( a );
                albumWindow.Show();

                this.perflist.InvalidateVisual();
            }
        }

        /// <summary>
        /// TODO
        /// </summary>
        private void SelectedAlbumKeyDown(object sender, KeyEventArgs e)
        {
            SelectedAlbums_MouseDoubleClick(sender, null);
        }


        /// <summary>
        /// TODO
        /// </summary>
        private void MusCatRadioClick(object sender, RoutedEventArgs e)
        {
            RadioPlayerWindow radio = new RadioPlayerWindow();
            radio.Show();
        }
        

        /// <summary>
        /// TODO
        /// </summary>
        private void FindClick(object sender, RoutedEventArgs e)
        {
        }


        /// <summary>
        /// Show stats window
        /// </summary>
        private void MenuStatsClick(object sender, RoutedEventArgs e)
        {
            //StatsWindow stats = new StatsWindow();
            //stats.Show();
        }
    }
}
