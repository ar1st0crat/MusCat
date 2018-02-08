using System.Configuration;
using System.Linq;
using MusCat.Views;
using System.Windows;
using Autofac;
using AutoMapper;
using AutoMapper.EquivalencyExpression;
using MusCat.Core.Entities;
using MusCat.Core.Interfaces;
using MusCat.Core.Interfaces.Data;
using MusCat.Core.Interfaces.Domain;
using MusCat.Core.Interfaces.Networking;
using MusCat.Core.Interfaces.Stats;
using MusCat.Core.Services;
using MusCat.Infrastructure.Data;
using MusCat.Infrastructure.Services;
using MusCat.Infrastructure.Services.Networking;
using MusCat.Infrastructure.Services.Stats;
using MusCat.ViewModels;
using MusCat.ViewModels.Entities;

namespace MusCat
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IContainer DiContainer { get; set; }

        void AppInitialize(object sender, StartupEventArgs e)
        {
            InitializeMappings();

            DiContainer = InitializeDiContainer();

            // Disable shutdown when the dialog is closed
            Current.ShutdownMode = ShutdownMode.OnLastWindowClose;

            // Setup FileLocator:

            if (FileLocator.MustBeConfigured())
            {
                MessageBox.Show("Please specify folders for MusCat to look for media files");

                var settingsWindow = new SettingsWindow();
                if (settingsWindow.ShowDialog() == false)
                {
                    MessageBox.Show("Default path will be used: " + FileLocator.Pathlist.FirstOrDefault());
                }
            }

            FileLocator.LoadConfiguration();

            // Show main window:

            var mainWindow = new MainWindow
            {
                DataContext = DiContainer.Resolve<MainViewModel>()
            };

            mainWindow.Show();
        }

        /// <summary>
        /// Initialize AutoMapper mappings
        /// </summary>
        private void InitializeMappings()
        {
            Mapper.Initialize(cfg =>
            {
                cfg.AddCollectionMappers();

                cfg.CreateMap<Performer, PerformerViewModel>()
                   .EqualityComparison((o, ovm) => o.Id == ovm.Id)
                   .ForMember(m => m.Albums, opt => opt.Ignore())
                   .ReverseMap();

                cfg.CreateMap<Album, AlbumViewModel>()
                   .EqualityComparison((o, ovm) => o.Id == ovm.Id)
                   .ReverseMap();

                cfg.CreateMap<Country, CountryViewModel>()
                   .EqualityComparison((o, ovm) => o.Id == ovm.Id);
            });
        }

        /// <summary>
        /// Setup Autofac
        /// </summary>
        private IContainer InitializeDiContainer()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["MusCatDbContext"].ConnectionString;

            var builder = new ContainerBuilder();

            builder.Register(c => new UnitOfWork(connectionString)).As<IUnitOfWork>().SingleInstance();
            builder.RegisterType<PerformerService>().As<IPerformerService>();
            builder.RegisterType<AlbumService>().As<IAlbumService>();
            builder.RegisterType<CountryService>().As<ICountryService>();
            builder.Register(c => new StatsService(connectionString)).As<IStatsService>();
            builder.Register(c => new RandomSongSelector(connectionString)).As<ISongSelector>();
            builder.RegisterType<AudioPlayer>().As<IAudioPlayer>();
            builder.RegisterType<RadioService>().As<IRadioService>();
            builder.RegisterType<Mp3SonglistHelper>().As<ISonglistHelper>();
            builder.RegisterType<LastfmDataLoader>().As<IWebDataLoader>();
            builder.RegisterType<RateCalculator>().As<IRateCalculator>();
            builder.RegisterType<MainViewModel>();
            builder.RegisterType<EditPerformerViewModel>();
            builder.RegisterType<EditAlbumViewModel>();
            builder.RegisterType<AlbumPlaybackViewModel>();
            builder.RegisterType<CountriesViewModel>();

            return builder.Build();
        }
    }
}
