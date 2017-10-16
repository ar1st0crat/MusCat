﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using LiveCharts;
using LiveCharts.Wpf;
using MusCat.Entities;
using MusCat.Repositories;

namespace MusCat.ViewModels
{
    class StatsViewModel : INotifyPropertyChanged
    {
        private readonly StatsRepository _repository;

        public long PerformerCount { get; set; }
        public long AlbumCount { get; set; }
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


        public StatsViewModel()
        {
            _repository = new StatsRepository();

            PerformerCount = _repository.PerformerCount;
            AlbumCount = _repository.AlbumCount;
            SongCount = _repository.SongCount;

            GetStats();
        }

        public async Task GetStats()
        {
            // most recently added albums 

            LatestAlbums = (await _repository.GetLatestAlbumsAsync()).ToList();
            RaisePropertyChanged("LatestAlbums");

            // bar chart "decades - album count - average album rate"

            var decades = await _repository.GetAlbumDecadesAsync();

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

            RaisePropertyChanged("Decades");
            RaisePropertyChanged("Labels");

            // pie chart "performer - countries"

            var countries = await _repository.GetPerformerCountriesAsync();

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

            RaisePropertyChanged("Countries");
        }

        #region INotifyPropertyChanged event and method

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}