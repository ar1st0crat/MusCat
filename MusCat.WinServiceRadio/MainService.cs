using MusCat.Infrastructure.Services;
using MusCat.Infrastructure.Services.Radio;
using System;
using System.Configuration;
using System.Threading;
using System.Web.Http;
using System.Web.Http.SelfHost;

namespace MusCat.WinServiceRadio
{
    public class MainService
    {
        private readonly HttpSelfHostServer _server;

        private static Timer _timer;

        public static RadioService Radio;
        public static DateTime StartPlayTime;


        public MainService()
        {
            var selfHostConfiguration = new HttpSelfHostConfiguration(@"http://localhost:5555");

            selfHostConfiguration.Routes.MapHttpRoute(
                name: "Default",
                routeTemplate: "{controller}/{action}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            _server = new HttpSelfHostServer(selfHostConfiguration);

            ConfigurePaths();
            ConfigureRadioService();

            _timer = new Timer(UpdateCounter, 0, Radio.CurrentSong.DurationInSeconds() * 1000, Timeout.Infinite);
            StartPlayTime = DateTime.Now;
        }

        public void Start()
        {
            _server.OpenAsync().Wait();
        }

        public void Stop()
        {
            _server.CloseAsync().Wait();
            _server.Dispose();
            _timer?.Dispose();
        }

        static void ConfigurePaths()
        {
            FileLocator.LoadConfiguration();
        }

        static void ConfigureRadioService()
        {
            var conn = ConfigurationManager.ConnectionStrings["MusCatDbContext"].ConnectionString;
            Radio = new RadioService(new RandomSongSelector(conn));

            Radio.MakeSonglist();
        }

        public static void MoveToNextSong()
        {
            Radio.MoveToNextSong();

            _timer.Change(Radio.CurrentSong.DurationInSeconds() * 1000, Timeout.Infinite);
            StartPlayTime = DateTime.Now;
        }

        static void UpdateCounter(object obj)
        {
            MoveToNextSong();
        }
    }
}
