using MusCat.ViewModels;
using System.Windows;

namespace MusCat.Views
{
    /// <summary>
    /// Interaction logic for StatsWindow.xaml
    /// </summary>
    public partial class StatsWindow : Window
    {
        public StatsWindow()
        {
            InitializeComponent();
        }

        private void PieChart_DataClick(object sender, LiveCharts.ChartPoint chartPoint)
        {
            var country = chartPoint.SeriesView.Title;

            (DataContext as StatsViewModel).UpdateTopPerformersAsync(country);
        }
    }
}
