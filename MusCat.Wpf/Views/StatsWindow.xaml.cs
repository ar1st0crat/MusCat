using MusCat.ViewModels;
using System.Windows.Controls;

namespace MusCat.Views
{
    public partial class StatsWindow : UserControl
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
