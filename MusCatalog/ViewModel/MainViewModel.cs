using MusCatalog.Model;
using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using MusCatalog.View;


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
            LoadPerformers();
        }

        public void LoadPerformers( string letter = "A" )
        {
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

                performers.Clear();

                foreach (var perf in performersSelected)
                {
                    byte? avgRate = null;

                    int ratedCount = perf.Albums.Count(t => t.Rate.HasValue);

                    if (ratedCount > 0)
                    {
                        int sumRate = perf.Albums.Sum(t =>
                        {
                            if (t.Rate.HasValue)
                                return t.Rate.Value;
                            else
                                return 0;
                        });

                        if (ratedCount > 2)
                        {
                            byte minRate = perf.Albums.Min(r => r.Rate).Value;
                            byte maxRate = perf.Albums.Max(r => r.Rate).Value;
                            sumRate -= (minRate + maxRate);
                            ratedCount -= 2;
                        }

                        avgRate = (byte)Math.Round((double)sumRate / ratedCount, MidpointRounding.AwayFromZero);
                    }

                    var albs = perf.Albums.OrderBy(a => a.ReleaseYear).ToList();

                    var pvModel = new PerformerViewModel { Performer = perf, AlbumCount = perf.Albums.Count(), AlbumCollectionRate=avgRate };

                    foreach (var alb in albs)
                    {
                        pvModel.Albums.Add(new AlbumViewModel { Album = alb });
                    }

                    performers.Add( pvModel );
                }
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

                // order each performer's albums by year of release, then by name (in collection view)
                performers.Clear();

                foreach (var perf in performersSelected)
                {
                    byte? avgRate = null;

                    int ratedCount = perf.Albums.Count(t => t.Rate.HasValue);

                    if (ratedCount > 0)
                    {
                        int sumRate = perf.Albums.Sum(t =>
                        {
                            if (t.Rate.HasValue)
                                return t.Rate.Value;
                            else
                                return 0;
                        });

                        if (ratedCount > 2)
                        {
                            byte minRate = perf.Albums.Min(r => r.Rate).Value;
                            byte maxRate = perf.Albums.Max(r => r.Rate).Value;
                            sumRate -= (minRate + maxRate);
                            ratedCount -= 2;
                        }

                        avgRate = (byte)Math.Round((double)sumRate / ratedCount, MidpointRounding.AwayFromZero);
                    }

                    var albs = perf.Albums.OrderBy(a => a.ReleaseYear).ToList();

                    perf.Albums.Clear();
                    foreach (var alb in albs)
                    {
                        perf.Albums.Add(alb);
                    }

                    performers.Add(new PerformerViewModel { Performer = perf, AlbumCount = perf.Albums.Count(), AlbumCollectionRate = avgRate });
                }
            }
        }

        public void LoadPerformersByAlbumName(string name)
        {
            using (var context = new MusCatEntities())
            {
                var performersSelected = context.Performers.Include("Country")
                                                   .Where(p => p.Albums
                                                   .Where(a => a.Name.Contains(name))
                                                   .Count() > 0)
                                                   .OrderBy(p => p.Name);

                performers.Clear();

                foreach (var perf in performersSelected)
                {
                    var filteredAlbums = perf.Albums.Where(a => a.Name.ToUpper().Contains(name.ToUpper())).ToList();
                    perf.Albums.Clear();
                    foreach (var album in filteredAlbums)
                    {
                        perf.Albums.Add(album);
                    }

                    byte? avgRate = null;

                    int ratedCount = perf.Albums.Count(t => t.Rate.HasValue);

                    if (ratedCount > 0)
                    {
                        int sumRate = perf.Albums.Sum(t =>
                        {
                            if (t.Rate.HasValue)
                                return t.Rate.Value;
                            else
                                return 0;
                        });

                        if (ratedCount > 2)
                        {
                            byte minRate = perf.Albums.Min(r => r.Rate).Value;
                            byte maxRate = perf.Albums.Max(r => r.Rate).Value;
                            sumRate -= (minRate + maxRate);
                            ratedCount -= 2;
                        }

                        avgRate = (byte)Math.Round((double)sumRate / ratedCount, MidpointRounding.AwayFromZero);
                    }

                    var albs = perf.Albums.OrderBy(a => a.ReleaseYear).ToList();

                    perf.Albums.Clear();
                    foreach (var alb in albs)
                    {
                        perf.Albums.Add(alb);
                    }

                    performers.Add(new PerformerViewModel { Performer = perf, AlbumCount = perf.Albums.Count(), AlbumCollectionRate = avgRate });
                }
            }
        }

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

                    performers.Remove( SelectedPerformer );
                }
            }   
        }

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

                    SelectedPerformer.Albums.Remove( selectedAlbum );
                }
            }
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

                EditPerformerViewModel model = new EditPerformerViewModel(perf);
                EditPerformerWindow perfWindow = new EditPerformerWindow(model);
                perfWindow.ShowDialog();

                // TODO: do this only if the first letter is current letter
                if ( perf.Name.ToUpper()[0] == performers[0].Performer.Name.ToUpper()[0] )
                {
                    performers.Add(new PerformerViewModel { Performer = perf, Albums = new ObservableCollection<AlbumViewModel>() });
                }

                MessageBox.Show( "Performer was succesfully added to database" );
            }
        }

        public void EditPerformer()
        {
            if (SelectedPerformer == null)
            {
                MessageBox.Show( "Please select performer to edit!" );
                return;
            }

            EditPerformerViewModel model = new EditPerformerViewModel( SelectedPerformer.Performer );
            EditPerformerWindow perfWindow = new EditPerformerWindow( model );
            perfWindow.ShowDialog();
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
