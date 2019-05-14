using System.Collections.Concurrent;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Timers;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using MusCat.Infrastructure.Data;
using MusCat.Infrastructure.Services;
using MusCat.Core.Interfaces.Radio;
using MusCat.Infrastructure.Services.Radio;

namespace MusCat.WebApi.Controllers
{
    public class RadioController : ApiController
    {
        private static readonly IRadioService RadioService =
            new RadioService(new RandomSongSelector(ConfigurationManager.ConnectionStrings["MusCatDbContext"].ConnectionString));

        private static ConcurrentBag<StreamWriter> _clients;
        private static Timer _timer;

        static RadioController()//IRadioService radioService)
        {
            //RadioService = radioService;
            RadioService.MakeSonglist();

            //_clients = new ConcurrentBag<StreamWriter>();

            //_timer = new Timer
            //{
            //    Interval = 2000,
            //    AutoReset = true
            //};
            //_timer.Elapsed += timer_Elapsed;
            //_timer.Start();
        }

        [HttpGet]
        public HttpResponseMessage Stream(HttpRequestMessage request)
        {
            var response = Request.CreateResponse();

            response.Content = new PushStreamContent((a, b, c) =>
            {
                var path = FileLocator.FindSongPath(RadioService.CurrentSong);

                var audio = new HttpFileStream(path);

                audio.WriteToStream(a, b, c);
            }, 
            "audio/mpeg");

            return response;

            //var response = request.CreateResponse();
            //response.Content = new PushStreamContent((a, b, c) => { OnStreamAvailable(a, b, c); }, "text/event-stream");
            //return response;
        }

        [HttpGet]
        [Route("api/radio/current")]
        public HttpResponseMessage Current(HttpRequestMessage request)
        {
            var song = RadioService.CurrentSong;
            var result = new
                         {
                             Id = song.AlbumId,
                             Name = song.Name,
                             Duration = song.TimeLength,
                             Year = song.Album.ReleaseYear,
                             Performer = song.Album.Performer.Name
                         };

            var response = Request.CreateResponse(HttpStatusCode.OK, result);
            return response;
        }

        [HttpGet]
        [Route("api/radio/upcoming")]
        public HttpResponseMessage Upcoming(HttpRequestMessage request)
        {
            var archive = from song in RadioService.UpcomingSongs.Take(8)
                select new
                {
                    Id = song.AlbumId,
                    Name = song.Name,
                    Duration = song.TimeLength,
                    Album = song.Album.Name,
                    Performer = song.Album.Performer.Name
                };

            var response = Request.CreateResponse(HttpStatusCode.OK, archive);
            return response;
        }

        [HttpGet]
        [Route("api/radio/archive")]
        public HttpResponseMessage Archive(HttpRequestMessage request)
        {
            var archive = from song in RadioService.SongArchive
                select new
                {
                    Name = song.Name,
                    Duration = song.TimeLength,
                    Album = song.Album.Name,
                    Performer = song.Album.Performer.Name
                };

            var response = Request.CreateResponse(HttpStatusCode.OK, archive);
            return response;
        }

        [HttpGet]
        [Route("api/radio/next")]
        public async Task NextAsync()
        {
            await RadioService.MoveToNextSongAsync();
        }

        //private async static void timer_Elapsed(object sender, ElapsedEventArgs e)
        //{
        //    foreach (var client in _clients)
        //    {
        //        try
        //        {
        //            await client.WriteAsync(data);
        //            await client.FlushAsync();
        //        }
        //        catch (Exception)
        //        {
        //            StreamWriter ignore;
        //            _clients.TryTake(out ignore);
        //        }
        //    }
        //}

        //private void OnStreamAvailable(Stream stream, HttpContent content, TransportContext context)
        //{
        //    var client = new StreamWriter(stream);
        //    _clients.Add(client);
        //}
    }
}
