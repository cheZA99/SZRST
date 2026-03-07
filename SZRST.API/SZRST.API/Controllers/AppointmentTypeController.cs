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
using SZRST.Domain.Entities;

namespace SZRST.API.Controllers
{
	[Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin}, {Roles.Uposlenik}, {Roles.Korisnik}")]
	[Authorize]
	[Route("api/[controller]")]
	[ApiController]
	public class AppointmentTypeController :ControllerBase
	{
		private readonly SZRSTContext _context;

		public AppointmentTypeController(SZRSTContext context)
		{
			_context = context;
		}

		// GET: api/AppointmentType
		[HttpGet]
		public async Task<ActionResult<IEnumerable<AppointmentTypeDto>>> GetAppointmentTypes()
		{
			var appointmentTypes = await _context.AppointmentType
			    .Include(at => at.Tenant)
			    .Include(at => at.Currency)
			    .Where(at => !at.IsDeleted)
			    .ToListAsync();

			var dtos = appointmentTypes.Select(at => new AppointmentTypeDto
			{
				Id = at.Id,
				Name = at.Name,
				Duration = at.Duration,
				Price = at.Price,
				CurrencyId = at.CurrencyId,
				CurrencyName = at.Currency != null ? at.Currency.ShortName : null,
				TenantId = at.TenantId,
				TenantName = at.Tenant != null ? at.Tenant.Name : null,
				DateCreated = at.DateCreated,
				DateModified = at.DateModified
			});

			return Ok(dtos);
		}

		// GET: api/AppointmentType/{id}
		[HttpGet("{id}")]
		public async Task<ActionResult<AppointmentTypeDto>> GetAppointmentType(int id)
		{
			var appointmentType = await _context.AppointmentType
			    .Include(at => at.Tenant)
			    .Include(at => at.Currency)
			    .FirstOrDefaultAsync(at => at.Id == id && !at.IsDeleted);

			if (appointmentType == null)
			{
				return NotFound();
			}

			var dto = new AppointmentTypeDto
			{
				Id = appointmentType.Id,
				Name = appointmentType.Name,
				Duration = appointmentType.Duration,
				Price = appointmentType.Price,
				CurrencyId = appointmentType.CurrencyId,
				CurrencyName = appointmentType.Currency != null ? appointmentType.Currency.ShortName : null,
				TenantId = appointmentType.TenantId,
				TenantName = appointmentType.Tenant != null ? appointmentType.Tenant.Name : null,
				DateCreated = appointmentType.DateCreated,
				DateModified = appointmentType.DateModified
			};

			return dto;
		}

		// GET: api/AppointmentType/by-tenant/{tenantId}
		[HttpGet("by-tenant/{tenantId}")]
		public async Task<ActionResult<IEnumerable<AppointmentTypeDto>>> GetAppointmentTypesByTenant(int tenantId)
		{
			var appointmentTypes = await _context.AppointmentType
			    .Include(at => at.Currency)
			    .Where(at => at.TenantId == tenantId && !at.IsDeleted)
			    .ToListAsync();

			var dtos = appointmentTypes.Select(at => new AppointmentTypeDto
			{
				Id = at.Id,
				Name = at.Name,
				Duration = at.Duration,
				Price = at.Price,
				CurrencyId = at.CurrencyId,
				CurrencyName = at.Currency != null ? at.Currency.ShortName : null,
				TenantId = at.TenantId,
				DateCreated = at.DateCreated,
				DateModified = at.DateModified
			});

			return Ok(dtos);
		}

		// POST: api/AppointmentType
		[HttpPost]
		public async Task<ActionResult<AppointmentType>> CreateAppointmentType([FromBody] AppointmentTypeCreateDto appointmentTypeDto)
		{
            var currencyId = await _context.Currency
				.Where(x => x.ShortName == "BAM")
				.Select(x => x.Id)
				.FirstOrDefaultAsync();

            var appointmentType = new AppointmentType
			{
				Name = appointmentTypeDto.Name,
				Duration = appointmentTypeDto.Duration,
				Price = appointmentTypeDto.Price,
				CurrencyId = appointmentTypeDto.CurrencyId!=null ? appointmentTypeDto.CurrencyId : currencyId,
				TenantId = appointmentTypeDto.TenantId,
				DateCreated = DateTime.UtcNow,
				IsDeleted = false
			};

			_context.AppointmentType.Add(appointmentType);
			await _context.SaveChangesAsync();

			return CreatedAtAction(nameof(GetAppointmentType), new { id = appointmentType.Id }, appointmentType);
		}

		// PUT: api/AppointmentType/{id}
		[HttpPut("{id}")]
		public async Task<IActionResult> UpdateAppointmentType(int id, [FromBody] AppointmentTypeCreateDto appointmentTypeDto)
		{
			var appointmentType = await _context.AppointmentType.FindAsync(id);
			if (appointmentType == null || appointmentType.IsDeleted)
			{
				return NotFound();
			}

			appointmentType.Name = appointmentTypeDto.Name;
			appointmentType.Duration = appointmentTypeDto.Duration;
			appointmentType.Price = appointmentTypeDto.Price;
			appointmentType.CurrencyId = appointmentTypeDto.CurrencyId;
			appointmentType.TenantId = appointmentTypeDto.TenantId;
			appointmentType.DateModified = DateTime.UtcNow;

			_context.Entry(appointmentType).State = EntityState.Modified;

			try
			{
				await _context.SaveChangesAsync();
			}
			catch (DbUpdateConcurrencyException)
			{
				if (!AppointmentTypeExists(id))
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

		// DELETE: api/AppointmentType/{id}
		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteAppointmentType(int id)
		{
			var appointmentType = await _context.AppointmentType.FindAsync(id);
			if (appointmentType == null)
			{
				return NotFound();
			}

			appointmentType.IsDeleted = true;
			appointmentType.DateModified = DateTime.UtcNow;
			await _context.SaveChangesAsync();

			return NoContent();
		}

		private bool AppointmentTypeExists(int id)
		{
			return _context.AppointmentType.Any(e => e.Id == id && !e.IsDeleted);
		}
	}

	public class AppointmentTypeCreateDto
	{
		public string Name { get; set; }
		public int Duration { get; set; }
		public float Price { get; set; }
		public int? CurrencyId { get; set; }
		public int TenantId { get; set; }
	}

	public class AppointmentTypeDto
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public int Duration { get; set; }
		public float Price { get; set; }
		public int? CurrencyId { get; set; }
		public string CurrencyName { get; set; }
		public int TenantId { get; set; }
		public string TenantName { get; set; }
		public DateTime DateCreated { get; set; }
		public DateTime? DateModified { get; set; }
	}
}