using Domain.Entities;
using Infrastructure.Persistance;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SZRST.Domain.Constants;

namespace SZRST.API.Controllers
{
	[Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Uposlenik}")]
	[Route("api/[controller]")]
	[ApiController]
	public class WorkerTypeController :ControllerBase
	{
		private readonly SZRSTContext _context;

		public WorkerTypeController(SZRSTContext context)
		{
			_context = context;
		}

		// GET: api/WorkerType
		[HttpGet]
		public async Task<ActionResult<IEnumerable<WorkerTypeResponseDto>>> GetWorkerTypes()
		{
			var workerTypes = await _context.WorkerType
				.Where(wt => !wt.IsDeleted)
				.Select(wt => new WorkerTypeResponseDto
				{
					Id = wt.Id,
					Name = wt.Name,
					Description = wt.Description
				})
				.ToListAsync();

			return Ok(workerTypes);
		}

		// GET: api/WorkerType/{id}
		[HttpGet("{id}")]
		public async Task<ActionResult<WorkerTypeResponseDto>> GetWorkerType(int id)
		{
			var workerType = await _context.WorkerType.FirstOrDefaultAsync(wt => wt.Id == id && !wt.IsDeleted);

			if (workerType == null)
			{
				return NotFound();
			}

			return new WorkerTypeResponseDto
			{
				Id = workerType.Id,
				Name = workerType.Name,
				Description = workerType.Description
			};
		}

		// POST: api/WorkerType
		[HttpPost]
		public async Task<ActionResult<WorkerTypeResponseDto>> CreateWorkerType([FromBody] WorkerTypeCreateDto workerTypeDto)
		{
			var workerType = new WorkerType
			{
				Name = workerTypeDto.Name,
				Description = workerTypeDto.Description,
				IsDeleted = false
			};

			_context.WorkerType.Add(workerType);
			await _context.SaveChangesAsync();

			return CreatedAtAction(nameof(GetWorkerType), new { id = workerType.Id }, new WorkerTypeResponseDto
			{
				Id = workerType.Id,
				Name = workerType.Name,
				Description = workerType.Description
			});
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

			workerType.IsDeleted = true;

			try
			{
				await _context.SaveChangesAsync();
			}
			catch (DbUpdateException)
			{
				return BadRequest(new { message = "Nije moguće obrisati tip radnika jer se koristi u postojećim zapisima." });
			}

			return NoContent();
		}

		private bool WorkerTypeExists(int id)
		{
			return _context.WorkerType.Any(e => e.Id == id && !e.IsDeleted);
		}
	}

	public class WorkerTypeCreateDto
	{
		public string Name { get; set; }
		public string Description { get; set; }
	}

	public class WorkerTypeResponseDto
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
	}
}
