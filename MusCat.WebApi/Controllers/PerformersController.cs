using MusCat.Application.Dto;
using MusCat.Application.Interfaces;
using MusCat.Core.Entities;
using MusCat.Core.Interfaces.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;

namespace MusCat.WebApi.Controllers
{
    public class PerformersController : ApiController
    {
        private IPerformerService _performerService;

        public PerformersController(IPerformerService performerService)
        {
            _performerService = performerService;
        }

        // GET:api/Performers?page=1&size=10
        public async Task<PageCollection<PerformerDto>> Get(int page, int size)
        {
            return await _performerService.GetAllPerformersAsync(page, size);
        }

        // GET: api/Performers/5
        public async Task<Result<PerformerDto>> Get(int id)
        {
            return await _performerService.GetPerformerAsync(id);
        }

        // POST: api/Performers
        public async Task Post([FromBody]Performer performer)
        {
            await _performerService.AddPerformerAsync(performer);
        }

        // PUT: api/Performers/5
        public async Task Put(int id, [FromBody]Performer performer)
        {
            await _performerService.UpdatePerformerAsync(id, performer);
        }

        // DELETE: api/Performers/5
        public async Task Delete(int id)
        {
            await _performerService.RemovePerformerAsync(id);
        }

        [Route("api/Performers/{id}/Albums")]
        public async Task<IEnumerable<AlbumDto>> GetPerformerAlbums(int id)
        {
            return await _performerService.GetPerformerAlbumsAsync(id);
        }
    }
}
