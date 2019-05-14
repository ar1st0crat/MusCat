using MusCat.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using MusCat.Application.Interfaces;
using MusCat.Application.Dto;
using System.Net.Http;
using MusCat.Infrastructure.Services;

namespace MusCat.WebApi.Controllers
{
    public class AlbumsController : ApiController
    {
        private IAlbumService _albumService;

        public AlbumsController(IAlbumService albumService)
        {
            _albumService = albumService;
        }

        // GET: api/Albums
        public async Task<IEnumerable<AlbumDto>> Get()
        {
            return await _albumService.GetAllAlbumsAsync();
        }

        // GET: api/Albums/5
        public async Task<Result<AlbumDto>> Get(int id)
        {
            return await _albumService.GetAlbumAsync(id);
        }

        [Route("api/Albums/{id}/Songs")]
        public async Task<IEnumerable<SongDto>> GetAlbumSongs(int id)
        {
            return await _albumService.GetAlbumSongsAsync(id);
        }

        // POST: api/Albums
        public async Task<Result<AlbumDto>> Post([FromBody]Album album)
        {
            return await _albumService.AddAlbumAsync(album);
        }

        // PUT: api/Albums/5
        public async Task<Result<AlbumDto>> Put(int id, [FromBody]Album album)
        {
            return await _albumService.UpdateAlbumAsync(id, album);
        }

        // DELETE: api/Albums/5
        public async Task<Result<AlbumDto>> Delete(int id)
        {
            return await _albumService.RemoveAlbumAsync(id);
        }

        [HttpGet]
        [Route("api/Albums/{id}/cover")]
        public async Task<HttpResponseMessage> AlbumCover(int id)
        {
            var response = Request.CreateResponse();

            var result = await _albumService.GetAlbumAsync(id, true);

            if (result.Type != ResultType.Ok)
            {
                return null;// NotFound();
            }

            var album = new Album
            {
                Id = result.Data.Id,
                Performer = new Performer { Name = result.Data.Performer.Name }
            };

            response.Content = new PushStreamContent((stream, content, context) =>
            {
                var path = FileLocator.GetAlbumImagePath(album);
                var image = new HttpFileStream(path);

                image.WriteToStream(stream, content, context);
            },
            "image/jpeg");

            return response;
        }
    }
}
