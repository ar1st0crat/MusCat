using MusCatalog.ViewModel;
using System.Windows;

namespace MusCatalog.View
{
    /// <summary>
    /// Interaction logic for EditPerformerWindow.xaml
    /// </summary>
    public partial class EditPerformerWindow : Window
    {
        public EditPerformerWindow()
        {
            InitializeComponent();
        }

        private void SavePerformerInformation(object sender, RoutedEventArgs e)
        {
            ((EditPerformerViewModel)DataContext).SavePerformerInformation();
        }

        private void LoadPerformerImageFromFile(object sender, RoutedEventArgs e)
        {
            ((EditPerformerViewModel)DataContext).LoadPerformerImageFromFile();
        }

        private void LoadPerformerImageFromClipboard(object sender, RoutedEventArgs e)
        {
            ((EditPerformerViewModel)DataContext).LoadPerformerImageFromClipboard();
        }
    }
}
