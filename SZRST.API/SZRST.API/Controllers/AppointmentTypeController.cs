using Domain.Entities;
using FluentValidation;
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
	[Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Uposlenik},{Roles.Korisnik}")]
	[Route("api/[controller]")]
	[ApiController]
	public class AppointmentTypeController :ControllerBase
	{
		private readonly SZRSTContext _context;
		private readonly IValidator<AppointmentTypeCreateDto> _validator;
		private readonly ICurrentUserService _currentUserService;

		public AppointmentTypeController(
			SZRSTContext context,
			IValidator<AppointmentTypeCreateDto> validator,
			ICurrentUserService currentUserService)
		{
			_context = context;
			_validator = validator;
			_currentUserService = currentUserService;
		}

		// GET: api/AppointmentType
		[HttpGet]
		public async Task<ActionResult<IEnumerable<AppointmentTypeDto>>> GetAppointmentTypes()
		{
			if (!_currentUserService.IsSuperAdmin &&
			    !_currentUserService.IsKorisnik &&
			    !_currentUserService.TenantId.HasValue)
			{
				return Forbid();
			}

			var query = _context.AppointmentType.IgnoreQueryFilters();

			if (!_currentUserService.IsSuperAdmin && !_currentUserService.IsKorisnik)
				query = query.Where(at => at.TenantId == _currentUserService.TenantId.Value);

			var appointmentTypes = await query
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
				.IgnoreQueryFilters()
				.Include(at => at.Tenant)
				.Include(at => at.Currency)
				.FirstOrDefaultAsync(at => at.Id == id && !at.IsDeleted);

			if (appointmentType == null)
				return NotFound();

			if (!_currentUserService.IsSuperAdmin &&
			    !_currentUserService.IsKorisnik &&
			    !_currentUserService.CanAccessTenant(appointmentType.TenantId))
			{
				return Forbid();
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
			if (!_currentUserService.IsSuperAdmin &&
			    !_currentUserService.IsKorisnik &&
			    _currentUserService.TenantId != tenantId)
				return Forbid();

			var query = _context.AppointmentType.IgnoreQueryFilters();

			var appointmentTypes = await query
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
		[Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin}")]
		[HttpPost]
		public async Task<ActionResult<AppointmentType>> CreateAppointmentType([FromBody] AppointmentTypeCreateDto appointmentTypeDto)
		{
			var validationResult = await _validator.ValidateAsync(appointmentTypeDto);
			if (!validationResult.IsValid)
				return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));

			if (!_currentUserService.HasValidTenant)
				return Forbid();

			var currencyId = await _context.Currency
				.Where(x => x.ShortName == "BAM")
				.Select(x => x.Id)
				.FirstOrDefaultAsync();

			var tenantId = _currentUserService.IsSuperAdmin
				? appointmentTypeDto.TenantId
				: _currentUserService.TenantId!.Value;

			if (tenantId <= 0)
				return BadRequest("TenantId mora biti validan.");

			var appointmentType = new AppointmentType
			{
				Name = appointmentTypeDto.Name,
				Duration = appointmentTypeDto.Duration,
				Price = appointmentTypeDto.Price,
				CurrencyId = appointmentTypeDto.CurrencyId != null ? appointmentTypeDto.CurrencyId : currencyId,
				TenantId = tenantId,
				DateCreated = DateTime.UtcNow,
				IsDeleted = false
			};

			_context.AppointmentType.Add(appointmentType);
			await _context.SaveChangesAsync();

			return CreatedAtAction(nameof(GetAppointmentType), new { id = appointmentType.Id }, appointmentType);
		}

		// PUT: api/AppointmentType/{id}
		[Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin}")]
		[HttpPut("{id}")]
		public async Task<IActionResult> UpdateAppointmentType(int id, [FromBody] AppointmentTypeCreateDto appointmentTypeDto)
		{
			var validationResult = await _validator.ValidateAsync(appointmentTypeDto);
			if (!validationResult.IsValid)
				return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));

			var appointmentType = await _context.AppointmentType.IgnoreQueryFilters().FirstOrDefaultAsync(at => at.Id == id);
			if (appointmentType == null || appointmentType.IsDeleted)
				return NotFound();

			if (!_currentUserService.CanAccessTenant(appointmentType.TenantId))
				return Forbid();

			appointmentType.Name = appointmentTypeDto.Name;
			appointmentType.Duration = appointmentTypeDto.Duration;
			appointmentType.Price = appointmentTypeDto.Price;
			appointmentType.CurrencyId = appointmentTypeDto.CurrencyId;
			appointmentType.TenantId = _currentUserService.IsSuperAdmin
				? appointmentTypeDto.TenantId
				: _currentUserService.TenantId!.Value;
			appointmentType.DateModified = DateTime.UtcNow;

			_context.Entry(appointmentType).State = EntityState.Modified;

			try
			{
				await _context.SaveChangesAsync();
			}
			catch (DbUpdateConcurrencyException)
			{
				if (!AppointmentTypeExists(id))
					return NotFound();
				else
					throw;
			}

			return NoContent();
		}

		// DELETE: api/AppointmentType/{id}
		[Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin}")]
		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteAppointmentType(int id)
		{
			var appointmentType = await _context.AppointmentType.IgnoreQueryFilters().FirstOrDefaultAsync(at => at.Id == id);
			if (appointmentType == null)
				return NotFound();

			if (!_currentUserService.CanAccessTenant(appointmentType.TenantId))
				return Forbid();

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

	public class AppointmentTypeCreateDtoValidator :AbstractValidator<AppointmentTypeCreateDto>
	{
		public AppointmentTypeCreateDtoValidator()
		{
			RuleFor(x => x.Name)
				.NotEmpty().WithMessage("Naziv tipa termina je obavezan.")
				.MaximumLength(100).WithMessage("Naziv ne smije biti duži od 100 karaktera.");

			RuleFor(x => x.Duration)
				.GreaterThan(0).WithMessage("Trajanje mora biti veće od 0 minuta.")
				.LessThanOrEqualTo(480).WithMessage("Trajanje ne može biti duže od 480 minuta (8 sati).");

			RuleFor(x => x.Price)
				.GreaterThanOrEqualTo(0).WithMessage("Cijena ne može biti negativna.");

			RuleFor(x => x.TenantId)
				.GreaterThan(0).WithMessage("TenantId mora biti validan.");
		}
	}
}
