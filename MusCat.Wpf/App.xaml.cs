using System.Configuration;
using System.Linq;
using MusCat.Views;
using System.Windows;
using Autofac;
using AutoMapper;
using AutoMapper.EquivalencyExpression;
using MusCat.Core.Entities;
using MusCat.Core.Interfaces;
using MusCat.Core.Interfaces.Audio;
using MusCat.Core.Interfaces.Data;
using MusCat.Core.Interfaces.Networking;
using MusCat.Core.Interfaces.Radio;
using MusCat.Core.Interfaces.Tracklist;
using MusCat.Core.Interfaces.Stats;
using MusCat.Core.Services;
using MusCat.Application.Interfaces;
using MusCat.Infrastructure.Business;
using MusCat.Infrastructure.Data;
using MusCat.Infrastructure.Services;
using MusCat.Infrastructure.Services.Audio;
using MusCat.Infrastructure.Services.Networking;
using MusCat.Infrastructure.Services.Radio;
using MusCat.Infrastructure.Services.Tracklist;
using MusCat.Infrastructure.Services.Stats;
using MusCat.ViewModels;
using MusCat.ViewModels.Entities;
using MusCat.Application.Dto;

namespace MusCat
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        public static IContainer DiContainer { get; set; }

        void AppInitialize(object sender, StartupEventArgs e)
        {
            InitializeMappings();

            DiContainer = InitializeDiContainer();

            // Setup FileLocator:

            if (FileLocator.MustBeConfigured())
            {
                MessageBox.Show("Please specify folders for MusCat to look for media files");

                // Disable shutdown when the dialog is closed
                Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;

                var settingsWindow = new SettingsWindow();
                if (settingsWindow.ShowDialog() == false)
                {
                    MessageBox.Show("Default path will be used: " + FileLocator.Pathlist.FirstOrDefault());
                }
            }

            FileLocator.LoadConfiguration();

            // Normal shutdown

            Current.ShutdownMode = ShutdownMode.OnLastWindowClose;
            
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

                cfg.AddProfile<Infrastructure.InfrastructureProfile>();

                cfg.CreateMap<Performer, PerformerViewModel>()
                   .EqualityComparison((src, dest) => src.Id == dest.Id)
                   .ForMember(m => m.Albums, opt => opt.Ignore())
                   .AfterMap((src, dest) => dest.LocateImagePath())
                   .ReverseMap();

                cfg.CreateMap<PerformerDto, PerformerViewModel>()
                   .EqualityComparison((src, dest) => src.Id == dest.Id)
                   .AfterMap((src, dest) => dest.LocateImagePath())
                   .ReverseMap();

                cfg.CreateMap<Album, AlbumViewModel>()
                   .EqualityComparison((src, dest) => src.Id == dest.Id)
                   .AfterMap((src, dest) => dest.LocateImagePath())
                   .ReverseMap();

                cfg.CreateMap<AlbumDto, AlbumViewModel>()
                   .EqualityComparison((src, dest) => src.Id == dest.Id)
                   .AfterMap((src, dest) => dest.LocateImagePath())
                   .ReverseMap();

                cfg.CreateMap<Song, SongViewModel>()
                   .EqualityComparison((src, dest) => src.Id == dest.Id)
                   .ReverseMap();

                cfg.CreateMap<Song, SongDto>()
                   .EqualityComparison((src, dest) => src.Id == dest.Id)
                   .ReverseMap();

                cfg.CreateMap<Song, RadioSongViewModel>()
                   .AfterMap((src, dest) => dest.LocateAlbumImagePath(src.Album));

                cfg.CreateMap<SongViewModel, Track>()
                   .EqualityComparison((src, dest) => src.TrackNo == dest.No)
                   .ForMember(dest => dest.No, opt => opt.MapFrom(src => src.TrackNo))
                   .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Name))
                   .ForMember(dest => dest.Duration, opt => opt.MapFrom(src => src.TimeLength));

                cfg.CreateMap<Track, SongViewModel>()
                   .EqualityComparison((src, dest) => src.No == dest.TrackNo)
                   .ForMember(dest => dest.Id, opt => opt.MapFrom(src => -1))
                   .ForMember(dest => dest.TrackNo, opt => opt.MapFrom(src => src.No))
                   .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Title))
                   .ForMember(dest => dest.TimeLength, opt => opt.MapFrom(src => src.Duration));

                cfg.CreateMap<CountryDto, CountryViewModel>()
                   .EqualityComparison((src, dest) => src.Id == dest.Id);
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
            builder.RegisterType<SongService>().As<ISongService>();
            builder.RegisterType<CountryService>().As<ICountryService>();
            builder.Register(c => new StatsService(connectionString)).As<IStatsService>();
            builder.Register(c => new RandomSongSelector(connectionString)).As<ISongSelector>();
            builder.RegisterType<AudioPlayer>().As<IAudioPlayer>();
            builder.RegisterType<RadioService>().As<IRadioService>();
            builder.RegisterType<Mp3TracklistHelper>().As<ITracklistHelper>();
            builder.RegisterType<WebLoader>().As<IWebLoader>();
            builder.RegisterType<RateCalculator>().As<IRateCalculator>();
            builder.RegisterType<MainViewModel>();
            builder.RegisterType<AlbumPlaybackViewModel>();
            builder.RegisterType<StatsViewModel>();
            builder.RegisterType<EditPerformerViewModel>();
            builder.RegisterType<EditAlbumViewModel>();
            builder.RegisterType<EditCountryViewModel>();

            return builder.Build();
        }
    }
}
