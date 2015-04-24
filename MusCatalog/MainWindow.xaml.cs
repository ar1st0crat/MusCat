using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MusCatalog
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        LetterButton prevButton = null;

        /// <summary>
        /// ...
        /// </summary>
        /// <param name="letter"></param>
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

                this.perflist.ItemsSource = performers.ToList();
                this.perflist.SelectedIndex = -1;
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            
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


        private void LetterClick(object sender, RoutedEventArgs e)
        {
            LetterButton pressedButton = (LetterButton)sender;

            prevButton.DeSelect();
            pressedButton.Select();
            
            prevButton = pressedButton;

            string letter = pressedButton.Content.ToString();
            FillPerformersListByFirstLetter( letter );
        }
               

        private void MenuStatsClick(object sender, RoutedEventArgs e)
        {

        }

        private void perflist_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                MessageBox.Show( "Are you sure you want to delete ?" );
            }
        }

    
        private void perflist_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount > 1)
            {
                Performers p = perflist.SelectedItem as Performers;
                MessageBox.Show(p.PID.ToString());
                MessageBox.Show(p.Performer);
            }
        }

        private void SelectedAlbums_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ListBox lb = e.Source as ListBox;

            if (lb.Name == "SelectedAlbums")
            {
                Albums a = lb.SelectedItem as Albums;
                MessageBox.Show(a.AID.ToString());
                MessageBox.Show(a.Album);
            }
        }

        private void MusCatRadioClick(object sender, RoutedEventArgs e)
        {
            RadioPlayerWindow radio = new RadioPlayerWindow();
            radio.Show();
        }
    }
}
