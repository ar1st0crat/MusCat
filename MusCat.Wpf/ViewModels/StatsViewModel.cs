using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using AutoMapper;
using LiveCharts;
using LiveCharts.Wpf;
using MusCat.Core.Interfaces.Stats;
using MusCat.Core.Util;
using MusCat.ViewModels.Entities;

namespace MusCat.ViewModels
{
    class StatsViewModel : ViewModelBase
    {
        private readonly IStatsService _stats;

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

        private long _songCount;
        public long SongCount
        {
            get { return _songCount; }
            set
            {
                _songCount = value;
                RaisePropertyChanged();
            }
        }

        public const int LatestAlbumsCount = 7;

        private List<AlbumViewModel> _latestAlbums;
        public List<AlbumViewModel> LatestAlbums
        {
            get { return _latestAlbums; }
            set
            {
                _latestAlbums = value;
                RaisePropertyChanged();
            }
        }

        private SeriesCollection _decades;
        public SeriesCollection Decades
        {
            get { return _decades; }
            set
            {
                _decades = value;
                RaisePropertyChanged();
            }
        }

        private SeriesCollection _countries;
        public SeriesCollection Countries
        {
            get { return _countries; }
            set
            {
                _countries = value;
                RaisePropertyChanged();
            }
        }

        private string[] _labels;
        public string[] Labels
        {
            get { return _labels; }
            set
            {
                _labels = value;
                RaisePropertyChanged();
            }
        }

        public Func<double, string> Formatter { get; set; }
        public ColorsCollection SeriesColor { get; set; } =
            new ColorsCollection
            {
                Color.FromRgb(195,225,125),
                Color.FromRgb(125,190,240)
            };

        public StatsViewModel(IStatsService stats)
        {
            Guard.AgainstNull(stats);
            _stats = stats;
        } 

        public async Task LoadStatsAsync()
        {
            PerformerCount = await _stats.PerformerCountAsync();
            AlbumCount = await _stats.AlbumCountAsync();
            SongCount = await _stats.SongCountAsync();

            // most recently added albums 

            var latestAlbums = await _stats.GetLatestAlbumsAsync(LatestAlbumsCount);
            LatestAlbums = Mapper.Map<List<AlbumViewModel>>(latestAlbums);

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
