using MusCatalog.ViewModel;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;


namespace MusCatalog.View
{
    /// <summary>
    /// Interaction logic for AlbumWindow.xaml
    /// Some WinForms programming style here, since interaction between MediaPlayer and Slider could be made only in code-behind
    /// </summary>
    public partial class AlbumWindow : Window
    {
        public AlbumWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Handle window closing event to correctly dispose resources
        /// </summary>
        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ((AlbumPlaybackViewModel)DataContext).Close();
        }

        /// <summary>
        /// Rewind the song to the position specified by playback slider
        /// </summary>
        /// <param name="sender">Playback slider</param>
        private void SeekPlaybackPosition(object sender, DragCompletedEventArgs e)
        {
            ((AlbumPlaybackViewModel)DataContext).SeekPlaybackPosition();
        }

        /// <summary>
        /// Start playing or pause the selected song (playback button click handler)
        /// </summary>
        private void PlaySongClick(object sender, RoutedEventArgs e)
        {
            ((AlbumPlaybackViewModel)DataContext).PlaybackSongAction();
        }

        /// <summary>
        /// Just an additional feature of the AlbumWindow:
        /// user can't edit album info except that he/she can update album rate by clicking on the 5-star rate control
        /// </summary>
        private void NewRateMouseDown(object sender, MouseButtonEventArgs e)
        {
            ((AlbumPlaybackViewModel)DataContext).UpdateRate();
        }
    }
}
