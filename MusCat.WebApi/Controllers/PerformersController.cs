using MusCat.Core.Entities;
using MusCat.Core.Interfaces.Data;
using MusCat.Core.Interfaces.Domain;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

namespace MusCat.WebApi.Controllers
{
    public class PerformersController : ApiController
    {
        private IUnitOfWork _unitOfWork;
        private IPerformerService _performerService;

        public PerformersController(IUnitOfWork unitOfWork, IPerformerService performerService)
        {
            _unitOfWork = unitOfWork;
            _performerService = performerService;
        }

        // GET: api/Performers
        public async Task<IEnumerable<Performer>> GetAsync()
        {
            return await _unitOfWork.PerformerRepository.GetAllAsync();
        }

        // GET: api/Performers/5
        public Performer Get(int id)
        {
            return _unitOfWork.PerformerRepository.Get(p => p.Id == id).FirstOrDefault();
        }

        // POST: api/Performers
        public async Task Post([FromBody]Performer performer)
        {
            await _performerService.AddPerformerAsync(performer);
        }

        // PUT: api/Performers/5
        public async Task Put(int id, [FromBody]Performer performer)
        {
            await _performerService.UpdatePerformerAsync(performer);
        }

        // DELETE: api/Performers/5
        public async Task Delete(int id)
        {
            await _performerService.RemovePerformerAsync(id);
        }
    }
}
