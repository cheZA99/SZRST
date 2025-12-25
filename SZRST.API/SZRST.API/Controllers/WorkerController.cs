using Domain.Entities;
using Infrastructure.Persistance;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SZRST.Domain.Constants;

namespace SZRST.API.Controllers
{
	[Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin}, {Roles.Uposlenik}")]
	[Authorize]
	[Route("api/[controller]")]
	[ApiController]
	public class WorkerController :ControllerBase
	{
		private readonly SZRSTContext _context;

		public WorkerController(SZRSTContext context)
		{
			_context = context;
		}

		// GET: api/Worker
		[HttpGet]
		public async Task<ActionResult<IEnumerable<Worker>>> GetWorkers()
		{
			return await _context.Worker
							 .Include(w => w.User)          // Include related User
							 .Include(w => w.WorkerType)    // Include related WorkerType
							 .Include(w => w.Facility)      // Include related Facility
							 .ToListAsync();
		}

		// GET: api/Worker/{id}
		[HttpGet("{id}")]
		public async Task<ActionResult<Worker>> GetWorker(int id)
		{
			var worker = await _context.Worker
								  .Include(w => w.User)          // Include related User
								  .Include(w => w.WorkerType)    // Include related WorkerType
								  .Include(w => w.Facility)      // Include related Facility
								  .FirstOrDefaultAsync(w => w.Id == id);

			if (worker == null)
			{
				return NotFound();
			}

			return worker;
		}

		// POST: api/Worker
		[HttpPost]
		public async Task<ActionResult<Worker>> CreateWorker([FromBody] WorkerCreateDto workerDto)
		{
			var user = await _context.Users.FindAsync(workerDto.UserId);
			if (user == null)
			{
				return BadRequest("Invalid UserId");
			}

			var workerType = await _context.WorkerType.FindAsync(workerDto.WorkerTypeId);
			if (workerType == null)
			{
				return BadRequest("Invalid WorkerTypeId");
			}

			var facility = await _context.Facility.FindAsync(workerDto.FacilityId);
			if (facility == null)
			{
				return BadRequest("Invalid FacilityId");
			}

			var worker = new Worker
			{
				DateOfEmployment = workerDto.DateOfEmployment,
				User = user,
				WorkerType = workerType,
				Facility = facility,
				IsDeleted = false
			};

			_context.Worker.Add(worker);
			await _context.SaveChangesAsync();

			return CreatedAtAction(nameof(GetWorker), new { id = worker.Id }, worker);
		}

		// PUT: api/Worker/{id}
		[HttpPut("{id}")]
		public async Task<IActionResult> UpdateWorker(int id, [FromBody] WorkerCreateDto workerDto)
		{
			var worker = await _context.Worker.FindAsync(id);
			if (worker == null)
			{
				return NotFound();
			}

			var user = await _context.Users.FindAsync(workerDto.UserId);
			if (user == null)
			{
				return BadRequest("Invalid UserId");
			}

			var workerType = await _context.WorkerType.FindAsync(workerDto.WorkerTypeId);
			if (workerType == null)
			{
				return BadRequest("Invalid WorkerTypeId");
			}

			var facility = await _context.Facility.FindAsync(workerDto.FacilityId);
			if (facility == null)
			{
				return BadRequest("Invalid FacilityId");
			}

			worker.DateOfEmployment = workerDto.DateOfEmployment;
			worker.User = user;
			worker.WorkerType = workerType;
			worker.Facility = facility;

			_context.Entry(worker).State = EntityState.Modified;

			try
			{
				await _context.SaveChangesAsync();
			}
			catch (DbUpdateConcurrencyException)
			{
				if (!WorkerExists(id))
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

		// DELETE: api/Worker/{id}
		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteWorker(int id)
		{
			var worker = await _context.Worker.FindAsync(id);
			if (worker == null)
			{
				return NotFound();
			}

			_context.Worker.Remove(worker);
			await _context.SaveChangesAsync();

			return NoContent();
		}

		private bool WorkerExists(int id)
		{
			return _context.Worker.Any(e => e.Id == id);
		}
	}

	public class WorkerCreateDto
	{
		public DateTime DateOfEmployment { get; set; }
		public int UserId { get; set; }
		public int WorkerTypeId { get; set; }
		public int FacilityId { get; set; }
	}
}