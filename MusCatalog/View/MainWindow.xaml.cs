using MusCatalog.Model;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;


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
                IQueryable<Performer> performers;
                                                
                if (letter.Length == 1)                             // 'A', 'B', 'C', ..., 'Z'
                {
                    performers = from p in context.Performers
                                 where p.Name.StartsWith(letter)
                                 orderby p.Name
                                 select p;
                }
                else                                                // The "Other" option
                {
                    performers = from p in context.Performers
                                     where p.Name.Substring(0, 1).CompareTo("A") < 0 || p.Name.Substring(0, 1).CompareTo("Z") > 0
                                     orderby p.Name
                                     select p;
                }

                // ========================================= here we attach albums to each performer and order them by year of release.
                //                                                      Possible ways to write better code:
                //                                                          1) DataLoadOptions (performers AND albums) (failed so far)
                //                                                          2) select new { ... } - but will need to rewrite converters
                //                                                          3) CollectionViewSource (failed so far)
                foreach (var perf in performers)
                {
                    var albs = perf.Albums.OrderBy(a => a.ReleaseYear).ToList();

                    perf.Albums.Clear();

                    foreach (var alb in albs)
                    {
                        perf.Albums.Add( alb );
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
                    Performer perf = perflist.SelectedItem as Performer;

                    if (MessageBox.Show( string.Format( "Are you sure you want to delete '{0}'?", perf.Name ),
                                            "Confirmation",
                                            MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        using (var context = new MusCatEntities())
                        {
                            context.DeletePerformerByID( (int?)(perf.ID) );
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
                Performer p = perflist.SelectedItem as Performer;
                //PerformerWindow performerWindow = new PerformerWindow( p );
                //performerWindow.Show();

                string filepath = string.Format(@"F:\{0}\{1}\Picture\photo.jpg", char.ToUpperInvariant(p.Name[0]), p.Name);
                Directory.CreateDirectory( Path.GetDirectoryName( filepath ) );

                if ( !Clipboard.ContainsImage() )
                {
                    MessageBox.Show( "No image in clipboard!" );
                    return;
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
                    MessageBox.Show( ex.Message );
                }
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
                Album a = lb.SelectedItem as Album;
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
