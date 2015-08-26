using MusCatalog.ViewModel;
using System.Windows;
using System.Windows.Input;


namespace MusCatalog.View
{
    /// <summary>
    /// Class for interaction logic for RadioPlayerWindow.xaml
    /// </summary>
    public partial class RadioPlayerWindow : Window
    {
        public RadioPlayerWindow()
        {
            InitializeComponent();
        }

        private void PlaybackMouseDown(object sender, MouseButtonEventArgs e)
        {
            ((RadioViewModel)DataContext).SongPlaybackAction();
        }
        
        private void StopMouseDown(object sender, MouseButtonEventArgs e)
        {
            ((RadioViewModel)DataContext).Stop();
        }
        
        private void NextMouseDown(object sender, MouseButtonEventArgs e)
        {
            ((RadioViewModel)DataContext).PlayNextSong();
        }
        
        private void PrevMouseDown(object sender, MouseButtonEventArgs e)
        {
            ((RadioViewModel)DataContext).PlayPreviousSong();
        }
        
        private void RadioPlayerKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Right)
            {
                ((RadioViewModel)DataContext).PlayNextSong();
            }
            else if (e.Key == Key.Left)
            {
                ((RadioViewModel)DataContext).PlayPreviousSong();
            }
        }

        private void CurrentAlbumClick(object sender, MouseButtonEventArgs e)
        {
            ((RadioViewModel)DataContext).ViewAlbumContainingCurrentSong();
        }
    }
}
