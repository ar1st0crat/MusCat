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
        Button prevButton = null;

        /// <summary>
        /// ...
        /// </summary>
        /// <param name="letter"></param>
        private void FillPerformersListByFirstLetter( string letter )
        {
            using (var context = new MusCatEntities())
            {
                var performers = from p in context.Performers.Include("Albums")
                                 where p.Performer.StartsWith( letter )
                                 orderby p.Performer
                                 select p;

                this.perflist.ItemsSource = performers.ToList();

                this.perflist.SelectedIndex = -1;
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            // ===================================================================
            Button bA = new Button();
            bA.Content = "A";
            bA.Click += LetterClick;
            bA.Width = bA.Height = 50;
            lettersPanel.Children.Add( bA );
            prevButton = bA;

            foreach (char c in "BCDEFGHIJKLMNOPQRSTUVWXYZ")
            {
                Button b = new Button();
                b.Content = c;
                b.Click += LetterClick;
                b.Width = b.Height = 30;

                lettersPanel.Children.Add( b );
            }

            Button bOther = new Button();
            bOther.Content = "Other";
            bOther.Click += LetterClick;
            bOther.Width = 70;
            bOther.Height = 30;
            lettersPanel.Children.Add( bOther );

            // Start with the A-list
            FillPerformersListByFirstLetter("A");
        }


        private void LetterClick(object sender, RoutedEventArgs e)
        {
            Button pressedButton = ((Button)sender);

            string letter = pressedButton.Content.ToString();
            pressedButton.Width = pressedButton.Height = 50;
            prevButton.Width = prevButton.Height = 30;

            prevButton = pressedButton;

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
    }
}
