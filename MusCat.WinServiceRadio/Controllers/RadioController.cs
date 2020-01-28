using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace MusCat.WinServiceRadio.Controllers
{
    public class RadioController : ApiController
    {
        [HttpGet]
        public HttpResponseMessage Current(HttpRequestMessage request)
        {
            var song = MainService.Radio.CurrentSong;

            var delta = DateTime.Now - MainService.StartPlayTime;

            var result = new
            {
                Id = song.AlbumId,
                Name = song.Name,
                Duration = song.TimeLength,
                Year = song.Album.ReleaseYear,
                Performer = song.Album.Performer.Name,
                Position = (int)delta.TotalSeconds
            };

            var response = Request.CreateResponse(HttpStatusCode.OK, result);
            return response;
        }

        [HttpGet]
        public HttpResponseMessage Upcoming(HttpRequestMessage request)
        {
            var upcoming = from song in MainService.Radio.UpcomingSongs
                           select new
                           {
                               Id = song.AlbumId,
                               Name = song.Name,
                               Duration = song.TimeLength,
                               Album = song.Album.Name,
                               Performer = song.Album.Performer.Name
                           };

            var response = Request.CreateResponse(HttpStatusCode.OK, upcoming);
            return response;
        }

        [HttpGet]
        public HttpResponseMessage Archive(HttpRequestMessage request)
        {
            var archive = from song in MainService.Radio.SongArchive
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
        public void Next()
        {
            MainService.MoveToNextSong();
        }

        [HttpGet]
        public void Replace(int id)
        {
            MainService.Radio.ChangeSong(id);
        }

        [HttpGet]
        public void Remove(int id)
        {
            MainService.Radio.RemoveSong(id);
        }
    }
}
