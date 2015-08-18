using System.Windows;
using MusCatalog.Model;

namespace MusCatalog.View
{
    /// <summary>
    /// Interaction logic for PerformerWindow.xaml
    /// </summary>
    public partial class PerformerWindow : Window
    {
        public PerformerWindow( Performer perf )
        {
            InitializeComponent();
            this.DataContext = perf;
        }
    }
}
