using MusCat.Application.Dto;
using MusCat.Application.Interfaces;
using MusCat.Core.Entities;
using MusCat.Core.Interfaces.Data;
using MusCat.Infrastructure.Services;
using System.Collections.Generic;
using System.Net.Http;
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

        // GET:api/Performers?page=0&size=10
        public async Task<PageCollection<PerformerDto>> Get(int page, int size)
        {
            return await _performerService.GetAllPerformersAsync(page, size);
        }

        // GET:api/Performers?page=0&size=10&search=Chris
        public async Task<PageCollection<PerformerDto>> GetByPattern(int page, int size, string search)
        {
            return await _performerService.GetPerformersBySubstringAsync(search, page, size);
        }

        // GET: api/Performers/5
        public async Task<Result<PerformerDto>> Get(int id)
        {
            return await _performerService.GetPerformerAsync(id);
        }

        // POST: api/Performers
        public async Task<Result<PerformerDto>> Post([FromBody]Performer performer)
        {
            return await _performerService.AddPerformerAsync(performer);
        }

        // PUT: api/Performers/5
        public async Task<Result<PerformerDto>> Put(int id, [FromBody]Performer performer)
        {
            return await _performerService.UpdatePerformerAsync(id, performer);
        }

        // DELETE: api/Performers/5
        public async Task<Result<PerformerDto>> Delete(int id)
        {
            return await _performerService.RemovePerformerAsync(id);
        }

        [Route("api/Performers/{id}/Albums")]
        public async Task<IEnumerable<AlbumDto>> GetPerformerAlbums(int id)
        {
            return await _performerService.GetPerformerAlbumsAsync(id);
        }

        [HttpGet]
        [Route("api/Performers/{id}/photo")]
        public async Task<HttpResponseMessage> PerformerPhoto(int id)
        {
            var response = Request.CreateResponse();

            var result = await _performerService.GetPerformerAsync(id);

            if (result.Type != ResultType.Ok)
            {
                return null;// NotFound();
            }

            var performer = new Performer { Name = result.Data.Name };

            response.Content = new PushStreamContent((stream, content, context) =>
            {
                var path = FileLocator.GetPerformerImagePath(performer);
                var image = new HttpFileStream(path);

                image.WriteToStream(stream, content, context);
            },
            "image/jpeg");

            return response;
        }
    }
}
