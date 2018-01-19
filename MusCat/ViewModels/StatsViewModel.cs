using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using LiveCharts;
using LiveCharts.Wpf;
using MusCat.Entities;
using MusCat.Services.Stats;

namespace MusCat.ViewModels
{
    class StatsViewModel : ViewModelBase
    {
        private readonly StatsService _stats = new StatsService();

        private long _performerCount;
        public long PerformerCount
        {
            get { return _performerCount; }
            set
            {
                _performerCount = value;
                RaisePropertyChanged();
            }
        }

        private long _albumCount;
        public long AlbumCount
        {
            get { return _albumCount; }
            set
            {
                _albumCount = value;
                RaisePropertyChanged();
            }
        }

        public long SongCount { get; set; }

        public List<Album> LatestAlbums { get; set; }

        public SeriesCollection Decades { get; set; }
        public SeriesCollection Countries { get; set; }
        public string[] Labels { get; set; }
        public Func<double, string> Formatter { get; set; }
        public ColorsCollection SeriesColor { get; set; } =
            new ColorsCollection
            {
                Color.FromRgb(195,225,125),
                Color.FromRgb(125,190,240)
            };

        public async Task LoadStatsAsync()
        {
            PerformerCount = await _stats.PerformerCountAsync();
            AlbumCount = await _stats.AlbumCountAsync();
            SongCount = await _stats.SongCountAsync();

            // most recently added albums 

            LatestAlbums = (await _stats.GetLatestAlbumsAsync()).ToList();


            // bar chart "decades - album count - average album rate"

            var decades = await _stats.GetAlbumDecadesAsync();

            Labels = decades.Select(d => d.Decade).ToArray();

            Decades = new SeriesCollection
                {
                    new ColumnSeries
                    {
                        Title = "Album count",
                        Values = new ChartValues<int>(decades.Select(d => d.TotalCount)),
                        DataLabels = true,
                        Foreground = new SolidColorBrush(Colors.Gold)
                    },
                    new ColumnSeries
                    {
                        Title = "Average rate",
                        Values = new ChartValues<int>(decades.Select(d => d.MaxRatedCount)),
                        DataLabels = true,
                        Foreground = new SolidColorBrush(Colors.HotPink)
                    }
                };

            // pie chart "performer - countries"

            var countries = await _stats.GetPerformerCountriesAsync();

            Countries = new SeriesCollection();

            foreach (var country in countries)
            {
                Countries.Add(new PieSeries
                {
                    Title = country.Key,
                    Values = new ChartValues<int> { country.Count() },
                    DataLabels = true
                });
            }
        }
    }
}
