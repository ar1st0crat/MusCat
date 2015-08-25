using MusCatalog.ViewModel;
using System.Windows;
using System.Windows.Input;

namespace MusCatalog.View
{
    /// <summary>
    /// Interaction logic for EditAlbumWindow.xaml
    /// </summary>
    public partial class EditAlbumWindow : Window
    {
        public EditAlbumWindow()
        {
            InitializeComponent();
        }

        private void ParseMp3(object sender, RoutedEventArgs e)
        {
            ((EditAlbumViewModel)DataContext).ParseMp3();
        }

        private void SaveSongCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        private void SaveSong(object sender, ExecutedRoutedEventArgs e)
        {
            ((EditAlbumViewModel)DataContext).SaveSong();
        }

        private void DeleteSongCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        private void DeleteSong(object sender, RoutedEventArgs e)
        {
            ((EditAlbumViewModel)DataContext).DeleteSong();
        }

        private void AddSong(object sender, RoutedEventArgs e)
        {
            ((EditAlbumViewModel)DataContext).AddSong();
        }

        private void SaveAlbumInformation(object sender, RoutedEventArgs e)
        {
            ((EditAlbumViewModel)DataContext).SaveAlbumInformation();
        }

        private void FixNames(object sender, RoutedEventArgs e)
        {
            ((EditAlbumViewModel)DataContext).FixNames();
        }

        private void FixTimes(object sender, RoutedEventArgs e)
        {
            ((EditAlbumViewModel)DataContext).FixTimes();
        }

        private void ClearAll(object sender, RoutedEventArgs e)
        {
            ((EditAlbumViewModel)DataContext).ClearAll();
        }

        private void SaveAll(object sender, RoutedEventArgs e)
        {
            ((EditAlbumViewModel)DataContext).SaveAll();
        }

        private void LoadAlbumImageFromClipboard(object sender, RoutedEventArgs e)
        {
            ((EditAlbumViewModel)DataContext).LoadAlbumImageFromClipboard();
        }

        private void LoadAlbumImageFromFile(object sender, RoutedEventArgs e)
        {
            ((EditAlbumViewModel)DataContext).LoadAlbumImageFromFile();
        }
    }
        

    //public static class AlbumCommands
    //{
    //    public static readonly RoutedUICommand SaveSong = new RoutedUICommand
    //            (
    //                    "Save Song",
    //                    "Save Song",
    //                    typeof(AlbumCommands),
    //                    new InputGestureCollection()
    //                            {
    //                                    new KeyGesture( Key.S, ModifierKeys.Control )
    //                            }
    //            );

    //    //Define more commands here, just like the one above
    //}
}
