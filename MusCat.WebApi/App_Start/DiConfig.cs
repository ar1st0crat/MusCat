using Autofac;
using Autofac.Integration.WebApi;
using MusCat.Application.Interfaces;
using MusCat.Core.Interfaces;
using MusCat.Core.Interfaces.Data;
using MusCat.Core.Interfaces.Networking;
using MusCat.Core.Interfaces.Radio;
using MusCat.Core.Interfaces.Stats;
using MusCat.Core.Interfaces.Tracklist;
using MusCat.Core.Services;
using MusCat.Infrastructure.Business;
using MusCat.Infrastructure.Data;
using MusCat.Infrastructure.Services.Networking;
using MusCat.Infrastructure.Services.Radio;
using MusCat.Infrastructure.Services.Stats;
using MusCat.Infrastructure.Services.Tracklist;
using System.Configuration;
using System.Reflection;
using System.Web.Http;

namespace MusCat.WebApi
{
    public static class DiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            var connectionString = ConfigurationManager.ConnectionStrings["MusCatDbContext"].ConnectionString;

            var builder = new ContainerBuilder();

            builder.RegisterApiControllers(Assembly.GetExecutingAssembly());
            builder.RegisterWebApiFilterProvider(config);

            builder.Register(c => new UnitOfWork(connectionString)).As<IUnitOfWork>().InstancePerRequest();//.SingleInstance();
            builder.RegisterType<PerformerService>().As<IPerformerService>().InstancePerRequest();
            builder.RegisterType<AlbumService>().As<IAlbumService>().InstancePerRequest();
            builder.RegisterType<SongService>().As<ISongService>();
            builder.RegisterType<CountryService>().As<ICountryService>();
            builder.Register(c => new StatsService(connectionString)).As<IStatsService>();
            builder.Register(c => new RandomSongSelector(connectionString)).As<ISongSelector>();
            builder.RegisterType<RadioService>().As<IRadioService>();
            builder.RegisterType<Mp3TracklistHelper>().As<ITracklistHelper>();
            builder.RegisterType<WebLoader>().As<IWebLoader>();
            builder.RegisterType<RateCalculator>().As<IRateCalculator>();

            var container = builder.Build();

            config.DependencyResolver = new AutofacWebApiDependencyResolver(container);
        }
    }
}