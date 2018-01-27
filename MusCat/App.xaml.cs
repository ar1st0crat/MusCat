using System.Linq;
using MusCat.Views;
using System.Windows;
using Autofac;
using AutoMapper;
using AutoMapper.EquivalencyExpression;
using MusCat.Core.Entities;
using MusCat.Core.Interfaces;
using MusCat.Core.Interfaces.Data;
using MusCat.Core.Services;
using MusCat.Infrastructure.Data;
using MusCat.Infrastructure.Services;
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
            // Initialize AutoMapper mappings:

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

                cfg.CreateMap<Album, AlbumPlaybackViewModel>()
                   .EqualityComparison((o, ovm) => o.Id == ovm.Id)
                   .ForMember(a => a.Songs, opt => opt.Ignore())
                   .ReverseMap();

                cfg.CreateMap<AlbumViewModel, AlbumPlaybackViewModel>();

                cfg.CreateMap<Song, SongViewModel>()
                   .EqualityComparison((o, ovm) => o.Id == ovm.Id)
                   .ReverseMap();

                cfg.CreateMap<Country, CountryViewModel>()
                   .EqualityComparison((o, ovm) => o.Id == ovm.Id);
            });


            // Setup Autofac:

            var builder = new ContainerBuilder();
            builder.RegisterType<UnitOfWork>().As<IUnitOfWork>();
            builder.RegisterType<RateCalculator>().As<IRateCalculator>();
            
            DiContainer = builder.Build();


            // Setup FileLocator:

            if (FileLocator.MustBeConfigured())
            {
                MessageBox.Show("Please specify folders for MusCat to look for media files");

                var settingsWindow = new SettingsWindow();
                if (settingsWindow.ShowDialog() == false)
                {
                    MessageBox.Show("Default path will be used: " + FileLocator.Pathlist.FirstOrDefault());
                    return;
                }
            }
            FileLocator.LoadConfiguration();
            

            // Show main window:

            var mainWindow = new MainWindow();
            mainWindow.Show();
        }
    }
}
