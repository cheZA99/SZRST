using Domain.Entities;
using Infrastructure.Persistance;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SZRST.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class WorkerTypeController : ControllerBase
    {
        private readonly SZRSTContext _context;

        public WorkerTypeController(SZRSTContext context)
        {
            _context = context;
        }

        // GET: api/WorkerType
        [HttpGet]
        public async Task<ActionResult<IEnumerable<WorkerType>>> GetWorkerTypes()
        {
            return await _context.WorkerType.ToListAsync();
        }

        // GET: api/WorkerType/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<WorkerType>> GetWorkerType(int id)
        {
            var workerType = await _context.WorkerType.FindAsync(id);

            if (workerType == null)
            {
                return NotFound();
            }

            return workerType;
        }

        // POST: api/WorkerType
        [HttpPost]
        public async Task<ActionResult<WorkerType>> CreateWorkerType([FromBody] WorkerTypeCreateDto workerTypeDto)
        {
            var workerType = new WorkerType
            {
                Name = workerTypeDto.Name,
                Description = workerTypeDto.Description
            };

            _context.WorkerType.Add(workerType);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetWorkerType), new { id = workerType.Id }, workerType);
        }

        // PUT: api/WorkerType/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateWorkerType(int id, [FromBody] WorkerTypeCreateDto workerTypeDto)
        {
            var workerType = await _context.WorkerType.FindAsync(id);
            if (workerType == null)
            {
                return NotFound();
            }

            workerType.Name = workerTypeDto.Name;
            workerType.Description = workerTypeDto.Description;

            _context.Entry(workerType).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!WorkerTypeExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/WorkerType/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteWorkerType(int id)
        {
            var workerType = await _context.WorkerType.FindAsync(id);
            if (workerType == null)
            {
                return NotFound();
            }

            _context.WorkerType.Remove(workerType);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool WorkerTypeExists(int id)
        {
            return _context.WorkerType.Any(e => e.Id == id);
        }
    }

    public class WorkerTypeCreateDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
