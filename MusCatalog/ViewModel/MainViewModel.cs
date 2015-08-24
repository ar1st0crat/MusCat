using MusCatalog.Model;
using MusCatalog.View;
using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Collections.Generic;


namespace MusCatalog.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<PerformerViewModel> performers = new ObservableCollection<PerformerViewModel>();
        public ObservableCollection<PerformerViewModel> Performers
        {
            get { return performers; }
            set
            {
                performers = value;
                RaisePropertyChanged("Performers");
            }
        }

        public PerformerViewModel SelectedPerformer { get; set; }

        public MainViewModel()
        {
            SelectPerformersByFirstLetter("A");
        }

        public void FillPerformerList( IQueryable<Performer> performersSelected, string albumString = null )
        {
            performers.Clear();

            foreach (var perf in performersSelected)
            {
                var pvModel = new PerformerViewModel { Performer = perf };

                /// Fill performer'salbumlist
                List<Album> albums;

                // If no filter is specified, then copy all albums to PerformerViewModel
                if (albumString == null)
                {
                    albums = perf.Albums.OrderBy(a => a.ReleaseYear).ToList();
                    foreach (var album in albums)
                    {
                        pvModel.Albums.Add(new AlbumViewModel { Album = album });
                    }
                }
                // Otherwise, filter out albums to show according to album search string
                else
                {
                    albums = perf.Albums.Where(a => a.Name.ToUpper().Contains(albumString.ToUpper())).ToList();
                    foreach (var album in albums)
                    {
                        pvModel.Albums.Add(new AlbumViewModel { Album = album });
                    }
                }

                /// The total rate of performer is calculated based on the following statistics of album rates:
                /// 
                ///     if the number of albums is more than 2 then the worst rate and the best rate are discarded
                ///     and the total rate is an average of remaining rates
                ///     
                ///     otherwise - the total rate is simply an average of album rates
                ///     
                byte? avgRate = null;

                int ratedCount = albums.Count(t => t.Rate.HasValue);
                if (ratedCount > 0)
                {
                    int sumRate = albums.Sum(t =>
                    {
                        if (t.Rate.HasValue)
                            return t.Rate.Value;
                        else
                            return 0;
                    });

                    if (ratedCount > 2)
                    {
                        byte minRate = albums.Min(r => r.Rate).Value;
                        byte maxRate = albums.Max(r => r.Rate).Value;
                        sumRate -= (minRate + maxRate);
                        ratedCount -= 2;
                    }

                    avgRate = (byte)Math.Round((double)sumRate / ratedCount, MidpointRounding.AwayFromZero);
                }

                pvModel.AlbumCount = albums.Count();
                pvModel.AlbumCollectionRate = avgRate;

                performers.Add(pvModel);
            }
        }

        public void SelectPerformersByFirstLetter( string letter = "A" )
        {
            /*
                var total = bannersPhrases.Select(p => p.Phrase).Count();
                var pageSize = 10; // set your page size, which is number of records per page

                var page = 1; // set current page number, must be >= 1

                var skip = pageSize * (page-1);

                var canPage = skip < total;

                if (canPage) // do what you wish if you can page no further
                   return;

                Phrases = bannersPhrases.Select(p => p.Phrase)
                             .Skip(skip)
                             .Take(pageSize)
                             .ToArray(); 
             */

            using (var context = new MusCatEntities())
            {
                IQueryable<Performer> performersSelected;

                // single letters ('A', 'B', 'C', ..., 'Z')
                if (letter.Length == 1)
                {
                    performersSelected = from p in context.Performers.Include("Country").Include("Albums")
                                 where p.Name.ToUpper().StartsWith(letter)
                                 orderby p.Name
                                 select p;
                }
                // The "Other" option
                // (all performers whose name doesn't start with capital English letter, e.g. "10CC", "Пикник", etc.)
                else
                {
                    performersSelected = from p in context.Performers.Include("Country").Include("Albums")
                                 where p.Name.ToUpper().Substring(0, 1).CompareTo("A") < 0 ||
                                       p.Name.ToUpper().Substring(0, 1).CompareTo("Z") > 0
                                 orderby p.Name
                                 select p;
                }

                FillPerformerList( performersSelected );
            }
        }

        public void LoadPerformersByName( string name )
        {
            using (var context = new MusCatEntities())
            {
                var performersSelected = from p in context.Performers.Include("Country").Include("Albums")
                                 where p.Name.ToUpper().Contains(name.ToUpper())
                                 orderby p.Name
                                 select p;

                FillPerformerList( performersSelected );
            }
        }

        public void LoadPerformersByAlbumName( string albumString )
        {
            using (var context = new MusCatEntities())
            {
                var performersSelected = context.Performers.Include("Country")
                                                   .Where(p => p.Albums
                                                   .Where(a => a.Name.Contains(albumString))
                                                   .Count() > 0)
                                                   .OrderBy(p => p.Name);

                FillPerformerList(performersSelected, albumString);
            }
        }

        public void ViewSelectedPerformer()
        {
            if (SelectedPerformer == null)
            {
                MessageBox.Show("Please select performer to edit!");
                return;
            }

            PerformerWindow perfWindow = new PerformerWindow();// SelectedPerformer );
            perfWindow.DataContext = SelectedPerformer;
            perfWindow.Show();
        }

        public void EditPerformer()
        {
            if (SelectedPerformer == null)
            {
                MessageBox.Show( "Please select performer to edit!" );
                return;
            }

            EditPerformerViewModel viewmodel = new EditPerformerViewModel( SelectedPerformer.Performer );
            EditPerformerWindow perfWindow = new EditPerformerWindow();
            perfWindow.DataContext = viewmodel;
            perfWindow.ShowDialog();
        }

        public void AddPerformer()
        {
            // set initial information of a newly added performer
            Performer perf = new Performer { Name = "Unknown performer" };

            using (var context = new MusCatEntities())
            {
                perf.ID = context.Performers.Max(p => p.ID) + 1;
                context.Performers.Add(perf);
                context.SaveChanges();

                EditPerformerViewModel viewmodel = new EditPerformerViewModel(perf);
                EditPerformerWindow perfWindow = new EditPerformerWindow();
                perfWindow.DataContext = viewmodel;
                perfWindow.ShowDialog();

                // TODO: do this only if the first letter is current letter
                if (perf.Name.ToUpper()[0] == performers[0].Performer.Name.ToUpper()[0])
                {
                    performers.Add(new PerformerViewModel { Performer = perf, Albums = new ObservableCollection<AlbumViewModel>() });
                }

                MessageBox.Show("Performer was succesfully added to database");
            }
        }

        /// <summary>
        /// cRud: Remove performer selected in the list of performers
        /// </summary>
        public void RemoveSelectedPerformer()
        {
            var perf = SelectedPerformer.Performer;

            if (perf == null)
            {
                MessageBox.Show("Please select performer to remove");
            }
            else if (MessageBox.Show(string.Format("Are you sure you want to delete '{0}'?",
                                        perf.Name),
                                        "Confirmation",
                                        MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                using (var context = new MusCatEntities())
                {
                    //context.DeletePerformerByID((int?)(perf.ID));                         // option1: stored procedure
                    var performerToDelete = context.Performers                              // option2: LINQ query
                                                   .Where(p => p.ID == perf.ID)
                                                   .SingleOrDefault<Performer>();
                    context.Performers.Remove(performerToDelete);
                    context.SaveChanges();

                    performers.Remove(SelectedPerformer);
                }
            }
        }

        public void ViewSelectedAlbum()
        {
            if (SelectedPerformer == null || SelectedPerformer.SelectedAlbum == null)
            {
                MessageBox.Show("Please select album to edit!");
                return;
            }

            // lazy load songs of selected album
            SelectedPerformer.SelectedAlbum.LoadSongs();

            AlbumWindow albumWindow = new AlbumWindow();
            albumWindow.DataContext = new AlbumPlaybackViewModel( SelectedPerformer.SelectedAlbum );
            albumWindow.Show();
        }

        public void EditAlbum()
        {
            if (SelectedPerformer == null || SelectedPerformer.SelectedAlbum == null)
            {
                MessageBox.Show("Please select album to edit!");
                return;
            }

            // lazy load songs of selected album
            SelectedPerformer.SelectedAlbum.LoadSongs();

            EditAlbumViewModel viewmodel = new EditAlbumViewModel( SelectedPerformer.SelectedAlbum.Album );
            AlbumWindow albumWindow = new AlbumWindow();
            albumWindow.DataContext = viewmodel;
            albumWindow.Show();
        }

        public void AddAlbum()
        {
            if (SelectedPerformer == null)
            {
                MessageBox.Show("Please select performer!");
                return;
            }

            // set initial information of a newly added album
            Album a = new Album {   Name = "New Album",
                                    TotalTime = "00:00",
                                    PerformerID = SelectedPerformer.Performer.ID,
                                    ReleaseYear = (short)DateTime.Now.Year };

            using (var context = new MusCatEntities())
            {
                a.ID = context.Albums.Max(alb => alb.ID) + 1;
                context.Albums.Add(a);
                context.SaveChanges();

                a.Performer = SelectedPerformer.Performer;
                SelectedPerformer.Albums.Add(new AlbumViewModel { Album = a });

                EditAlbumViewModel viewmodel = new EditAlbumViewModel(a);
                EditAlbumWindow editAlbum = new EditAlbumWindow();
                editAlbum.ShowDialog();
            }
        }

        /// <summary>
        /// Remove album selected in the list of albums
        /// </summary>
        public void RemoveSelectedAlbum()
        {
            if (SelectedPerformer == null)
            {
                MessageBox.Show("Please select performer first!");
            }

            var selectedAlbum = SelectedPerformer.SelectedAlbum;

            if (selectedAlbum == null)
            {
                MessageBox.Show("Please select album to remove");
            }
            else if (MessageBox.Show(string.Format("Are you sure you want to delete\n '{0}' \nby '{1}'?",
                                            selectedAlbum.Album.Name, selectedAlbum.Album.Performer.Name),
                                            "Confirmation",
                                            MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                using (var context = new MusCatEntities())
                {
                    //context.DeleteAlbumByID((int?)(selectedAlbum.ID));               // option1: stored procedure
                    var albumToDelete = context.Albums                                 // option2: LINQ query
                                               .Where(a => a.ID == selectedAlbum.Album.ID)
                                               .SingleOrDefault<Album>();
                    context.Albums.Remove(albumToDelete);
                    context.SaveChanges();

                    SelectedPerformer.Albums.Remove(selectedAlbum);
                }
            }
        }

        #region INotifyPropertyChanged event and method

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }
}
