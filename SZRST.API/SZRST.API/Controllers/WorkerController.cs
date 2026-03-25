using Domain.Entities;
using Infrastructure.Persistance;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SZRST.API.Security;
using SZRST.Domain.Constants;

namespace SZRST.API.Controllers
{
	[Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Uposlenik}")]
	[Route("api/[controller]")]
	[ApiController]
	public class WorkerController :ControllerBase
	{
		private readonly SZRSTContext _context;
		private readonly ICurrentUserService _currentUserService;

		public WorkerController(SZRSTContext context, ICurrentUserService currentUserService)
		{
			_context = context;
			_currentUserService = currentUserService;
		}

		// GET: api/Worker
		[HttpGet]
		public async Task<ActionResult<IEnumerable<WorkerDto>>> GetWorkers()
		{
			var workers = await _context.Worker
							 .Include(w => w.User)
							 .Include(w => w.WorkerType)
							 .Include(w => w.Facility)
							 .Where(w => !w.IsDeleted &&
								 (_currentUserService.IsSuperAdmin || w.TenantId == _currentUserService.TenantId))
							 .Select(w => new WorkerDto
							 {
								 Id = w.Id,
								 DateOfEmployment = w.DateOfEmployment,
								 UserId = w.User.Id,
								 UserName = w.User.UserName,
								 WorkerTypeId = w.WorkerType.Id,
								 WorkerTypeName = w.WorkerType.Name,
								 FacilityId = w.Facility.Id,
								 FacilityName = w.Facility.Name,
								 TenantId = w.TenantId
							 })
							 .ToListAsync();
			return Ok(workers);
		}

		// GET: api/Worker/{id}
		[HttpGet("{id}")]
		public async Task<ActionResult<WorkerDto>> GetWorker(int id)
		{
			var worker = await _context.Worker
								  .Include(w => w.User)
								  .Include(w => w.WorkerType)
								  .Include(w => w.Facility)
								  .Where(w => w.Id == id && !w.IsDeleted)
								  .Select(w => new WorkerDto
								  {
									  Id = w.Id,
									  DateOfEmployment = w.DateOfEmployment,
									  UserId = w.User.Id,
									  UserName = w.User.UserName,
									  WorkerTypeId = w.WorkerType.Id,
									  WorkerTypeName = w.WorkerType.Name,
									  FacilityId = w.Facility.Id,
									  FacilityName = w.Facility.Name,
									  TenantId = w.TenantId
								  })
								  .FirstOrDefaultAsync();

			if (worker == null)
			{
				return NotFound();
			}

			if (!_currentUserService.IsSuperAdmin && !_currentUserService.CanAccessTenant(worker.TenantId))
			{
				return Forbid();
			}

			return worker;
		}

		// POST: api/Worker
		[HttpPost]
		public async Task<ActionResult<WorkerDto>> CreateWorker([FromBody] WorkerCreateDto workerDto)
		{
			if (!_currentUserService.HasValidTenant)
				return Forbid();

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

			if (!_currentUserService.IsSuperAdmin &&
			    (user.TenantId != _currentUserService.TenantId ||
			     workerType.TenantId != _currentUserService.TenantId ||
			     facility.TenantId != _currentUserService.TenantId))
			{
				return Forbid();
			}

			if (user.TenantId != facility.TenantId || workerType.TenantId != facility.TenantId)
			{
				return BadRequest("Korisnik, tip radnika i objekat moraju pripadati istoj organizaciji.");
			}

			var worker = new Worker
			{
				DateOfEmployment = workerDto.DateOfEmployment,
				User = user,
				WorkerType = workerType,
				Facility = facility,
				IsDeleted = false,
				TenantId = facility.TenantId
			};

			_context.Worker.Add(worker);
			await _context.SaveChangesAsync();

			var workerResponse = new WorkerDto
			{
				Id = worker.Id,
				DateOfEmployment = worker.DateOfEmployment,
				UserId = user.Id,
				UserName = user.UserName,
				WorkerTypeId = workerType.Id,
				WorkerTypeName = workerType.Name,
				FacilityId = facility.Id,
				FacilityName = facility.Name,
				TenantId = worker.TenantId
			};

			return CreatedAtAction(nameof(GetWorker), new { id = worker.Id }, workerResponse);
		}

		// PUT: api/Worker/{id}
		[HttpPut("{id}")]
		public async Task<IActionResult> UpdateWorker(int id, [FromBody] WorkerCreateDto workerDto)
		{
			if (!_currentUserService.HasValidTenant)
				return Forbid();

			var worker = await _context.Worker.FindAsync(id);
			if (worker == null)
			{
				return NotFound();
			}

			if (!_currentUserService.IsSuperAdmin && !_currentUserService.CanAccessTenant(worker.TenantId))
			{
				return Forbid();
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

			if (!_currentUserService.IsSuperAdmin &&
			    (user.TenantId != _currentUserService.TenantId ||
			     workerType.TenantId != _currentUserService.TenantId ||
			     facility.TenantId != _currentUserService.TenantId))
			{
				return Forbid();
			}

			if (user.TenantId != facility.TenantId || workerType.TenantId != facility.TenantId)
			{
				return BadRequest("Korisnik, tip radnika i objekat moraju pripadati istoj organizaciji.");
			}

			worker.DateOfEmployment = workerDto.DateOfEmployment;
			worker.User = user;
			worker.WorkerType = workerType;
			worker.Facility = facility;
			worker.TenantId = facility.TenantId;

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

			if (!_currentUserService.IsSuperAdmin && !_currentUserService.CanAccessTenant(worker.TenantId))
			{
				return Forbid();
			}

			worker.IsDeleted = true;

			try
			{
				await _context.SaveChangesAsync();
			}
			catch (DbUpdateException)
			{
				return BadRequest(new { message = "Nije moguće obrisati radnika jer se koristi u postojećim zapisima." });
			}

			return NoContent();
		}

		private bool WorkerExists(int id)
		{
			return _context.Worker.Any(e => e.Id == id && !e.IsDeleted);
		}
	}

	public class WorkerCreateDto
	{
		public DateTime DateOfEmployment { get; set; }
		public int UserId { get; set; }
		public int WorkerTypeId { get; set; }
		public int FacilityId { get; set; }
	}

	public class WorkerDto
	{
		public int Id { get; set; }
		public DateTime DateOfEmployment { get; set; }
		public int UserId { get; set; }
		public string UserName { get; set; }
		public int WorkerTypeId { get; set; }
		public string WorkerTypeName { get; set; }
		public int FacilityId { get; set; }
		public string FacilityName { get; set; }
		public int TenantId { get; set; }
	}
}
