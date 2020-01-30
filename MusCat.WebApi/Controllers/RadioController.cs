using System.Net.Http;
using System.Web.Http;
using MusCat.Infrastructure.Services;
using System;
using MusCat.Core.Entities;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace MusCat.WebApi.Controllers
{
    /// <summary>
    /// Essentially we call our own win service (self-hosted web api) here
    /// </summary>
    public class RadioController : ApiController
    {
        static readonly HttpClient client = new HttpClient();

        static RadioController()
        {
            client.BaseAddress = new Uri("http://localhost:5555/");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        [HttpGet]
        public async Task<HttpResponseMessage> Stream()
        {
            var radioResponse = await client.GetAsync("radio/current");
            var songModel = await radioResponse.Content.ReadAsAsync<SongModel>();

            var song = new Song
            {
                Id = songModel.Id,
                Name = songModel.Name,
                TrackNo = songModel.TrackNo,
                Album = new Album
                {
                    Name = songModel.Album,
                    ReleaseYear = songModel.Year,
                    Performer = new Performer
                    {
                        Name = songModel.Performer
                    }
                },
            };

            var response = Request.CreateResponse();

            response.Content = new PushStreamContent((a, b, c) =>
            {
                var path = FileLocator.FindSongPath(song);

                var audio = new HttpFileStream(path);

                audio.WriteToStream(a, b, c);
            },
            "audio/mpeg");

            return response;
        }

        [HttpGet]
        [Route("api/radio/current")]
        public async Task<HttpResponseMessage> Current()
        {
            return await client.GetAsync("radio/current");
        }

        [HttpGet]
        [Route("api/radio/upcoming")]
        public async Task<HttpResponseMessage> Upcoming()
        {
            return await client.GetAsync("radio/upcoming");
        }

        [HttpGet]
        [Route("api/radio/archive")]
        public async Task<HttpResponseMessage> Archive()
        {
            return await client.GetAsync("radio/archive");
        }

        [HttpGet]
        [Route("api/radio/remove/{id}")]
        public async Task<HttpResponseMessage> Remove(int id)
        {
            await client.GetAsync($"radio/remove/{id}");
            return await client.GetAsync("radio/upcoming");
        }

        [HttpGet]
        [Route("api/radio/next")]
        public async Task Next()
        {
            await client.GetAsync("radio/next");
        }

        class SongModel
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Duration { get; set; }
            public string Album { get; set; }
            public short Year { get; set; }
            public string Performer { get; set; }
            public byte TrackNo { get; set; }
            public int Offset { get; set; }
        }
    }
}
