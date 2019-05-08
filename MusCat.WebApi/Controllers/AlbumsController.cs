using MusCat.Core.Entities;
using MusCat.Core.Interfaces.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using System.Linq;
using MusCat.Core.Interfaces.Domain;

namespace MusCat.WebApi.Controllers
{
    public class AlbumsController : ApiController
    {
        private IUnitOfWork _unitOfWork;
        private IAlbumService _albumService;

        public AlbumsController(IUnitOfWork unitOfWork, IAlbumService albumService)
        {
            _unitOfWork = unitOfWork;
            _albumService = albumService;
        }

        // GET: api/Albums
        public async Task<IEnumerable<Album>> GetAsync()
        {
            return await _unitOfWork.AlbumRepository.GetAllAsync();
        }

        [Route("api/performer/{id}/Albums")]
        public async Task<IEnumerable<Album>> GetPerformerAlbumsAsync(int id)
        {
            return await _unitOfWork.PerformerRepository
                                    .GetPerformerAlbumsAsync(id);
        }

        // GET: api/Albums/5
        public Album Get(int id)
        {
            return _unitOfWork.AlbumRepository.Get(a => a.Id == id).FirstOrDefault();
        }

        // POST: api/Albums
        public async Task Post([FromBody]Album album)
        {
            await _albumService.AddAlbumAsync(album);
        }

        // PUT: api/Albums/5
        public async Task Put(int id, [FromBody]Album album)
        {
            await _albumService.UpdateAlbumAsync(album);
        }

        // DELETE: api/Albums/5
        public async Task Delete(int id)
        {
            await _albumService.RemoveAlbumAsync(id);
        }
    }
}
