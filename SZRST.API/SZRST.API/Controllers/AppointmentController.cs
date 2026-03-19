using Domain.Entities;
using FluentValidation;
using Infrastructure.Persistance;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SZRST.API.Security;
using SZRST.Domain.Constants;
using SZRST.Domain.Entities;

namespace SZRST.API.Controllers
{
	[Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Uposlenik},{Roles.Korisnik}")]
	[Route("api/[controller]")]
	[ApiController]
	public class AppointmentController :ControllerBase
	{
		private readonly SZRSTContext _context;
		private readonly UserManager<User> _userManager;
		private readonly ICurrentUserService _currentUserService;
		private readonly IValidator<AppointmentCreateDto> _validator;

		public AppointmentController(
			SZRSTContext context,
			ICurrentUserService currentUserService,
			UserManager<User> userManager,
			IValidator<AppointmentCreateDto> validator)
		{
			_context = context;
			_currentUserService = currentUserService;
			_userManager = userManager;
			_validator = validator;
		}

		// GET: api/Appointment
		[HttpGet]
		public async Task<ActionResult<IEnumerable<AppointmentDto>>> GetAppointments()
		{
			var appointments = await _context.Appointment
							 .Include(a => a.Facility)
							 .Include(a => a.AppointmentType)
							 .Where(a => !a.IsDeleted)
							 .Select(a => new AppointmentDto
							 {
								 Id = a.Id,
								 AppointmentDateTime = a.AppointmentDateTime,
								 IsFree = a.IsFree,
								 IsClosed = a.IsClosed,
								 FacilityId = a.Facility.Id,
								 FacilityName = a.Facility.Name,
								 AppointmentTypeId = a.AppointmentType.Id,
								 AppointmentTypeName = a.AppointmentType.Name,
								 UserId = a.UserId,
								 TenantId = a.TenantId
							 })
							 .ToListAsync();

			return Ok(appointments);
		}

		// GET: api/Appointment/{id}
		[HttpGet("{id}")]
		public async Task<ActionResult<AppointmentDto>> GetAppointment(int id)
		{
			var appointment = await _context.Appointment
									  .Include(a => a.Facility)
									  .Include(a => a.AppointmentType)
									  .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);

			if (appointment == null)
				return NotFound();

			return Ok(MapAppointment(appointment));
		}

		// POST: api/Appointment
		[Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Uposlenik},{Roles.Korisnik}")]
		[HttpPost]
		public async Task<ActionResult<AppointmentDto>> CreateAppointment([FromBody] AppointmentCreateDto appointmentDto)
		{
			var validationResult = await _validator.ValidateAsync(appointmentDto);
			if (!validationResult.IsValid)
				return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));

			if (!_currentUserService.IsKorisnik && !_currentUserService.HasValidTenant)
				return Forbid();

			var facilityQuery = (_currentUserService.IsSuperAdmin || _currentUserService.IsKorisnik)
				? _context.Facility.IgnoreQueryFilters()
				: _context.Facility;

			var appointmentTypeQuery = (_currentUserService.IsSuperAdmin || _currentUserService.IsKorisnik)
				? _context.AppointmentType.IgnoreQueryFilters()
				: _context.AppointmentType;

			var facility = await facilityQuery.FirstOrDefaultAsync(f => f.Id == appointmentDto.FacilityId);
			if (facility == null)
				return BadRequest("Invalid FacilityId");

			var appointmentType = await appointmentTypeQuery.FirstOrDefaultAsync(at => at.Id == appointmentDto.AppointmentTypeId);
			if (appointmentType == null)
				return BadRequest("Invalid AppointmentTypeId");

			if (facility.TenantId != appointmentType.TenantId)
				return BadRequest("Facility i appointment type moraju pripadati istoj organizaciji.");

			if (!_currentUserService.IsSuperAdmin &&
			    !_currentUserService.IsKorisnik &&
			    (facility.TenantId != _currentUserService.TenantId || appointmentType.TenantId != _currentUserService.TenantId))
			{
				return Forbid();
			}

			var resolvedTenantId = facility.TenantId;

			var exists = await _context.Appointment.AnyAsync(a =>
				a.Facility.Id == appointmentDto.FacilityId &&
				a.AppointmentDateTime == appointmentDto.AppointmentDateTime &&
				!a.IsDeleted);

			if (exists)
				return BadRequest("Appointment already exists for selected time.");

			var assignedUserId = await ResolveAppointmentUserIdAsync(appointmentDto.UserId, resolvedTenantId);
			if (assignedUserId == null)
				return BadRequest("Invalid UserId");

			var appointment = new Appointment
			{
				AppointmentDateTime = appointmentDto.AppointmentDateTime,
				IsFree = appointmentDto.IsFree,
				IsClosed = appointmentDto.IsClosed,
				Facility = facility,
				AppointmentType = appointmentType,
				IsDeleted = false,
				UserId = assignedUserId.Value,
				TenantId = resolvedTenantId
			};

			_context.Appointment.Add(appointment);
			await _context.SaveChangesAsync();

			await _context.Entry(appointment).Reference(a => a.Facility).LoadAsync();
			await _context.Entry(appointment).Reference(a => a.AppointmentType).LoadAsync();

			return CreatedAtAction(nameof(GetAppointment), new { id = appointment.Id }, MapAppointment(appointment));
		}

		// PUT: api/Appointment/{id}
		[Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Uposlenik},{Roles.Korisnik}")]
		[HttpPut("{id}")]
		public async Task<IActionResult> UpdateAppointment(int id, [FromBody] AppointmentCreateDto appointmentDto)
		{
			var validationResult = await _validator.ValidateAsync(appointmentDto);
			if (!validationResult.IsValid)
				return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));

			if (!_currentUserService.IsKorisnik && !_currentUserService.HasValidTenant)
				return Forbid();

			var appointmentQuery = (_currentUserService.IsSuperAdmin || _currentUserService.IsKorisnik)
				? _context.Appointment.IgnoreQueryFilters()
				: _context.Appointment;

			var appointment = await appointmentQuery.FirstOrDefaultAsync(a => a.Id == id);
			if (appointment == null)
				return NotFound();

			if (appointment.IsClosed)
				return BadRequest("Closed appointment cannot be edited.");

			var facilityQuery = (_currentUserService.IsSuperAdmin || _currentUserService.IsKorisnik)
				? _context.Facility.IgnoreQueryFilters()
				: _context.Facility;

			var appointmentTypeQuery = (_currentUserService.IsSuperAdmin || _currentUserService.IsKorisnik)
				? _context.AppointmentType.IgnoreQueryFilters()
				: _context.AppointmentType;

			var facility = await facilityQuery.FirstOrDefaultAsync(f => f.Id == appointmentDto.FacilityId);
			if (facility == null)
				return BadRequest("Invalid FacilityId");

			var appointmentType = await appointmentTypeQuery.FirstOrDefaultAsync(at => at.Id == appointmentDto.AppointmentTypeId);
			if (appointmentType == null)
				return BadRequest("Invalid AppointmentTypeId");

			if (facility.TenantId != appointmentType.TenantId)
				return BadRequest("Facility i appointment type moraju pripadati istoj organizaciji.");

			if (_currentUserService.IsKorisnik && appointment.UserId != _currentUserService.UserId)
				return Forbid();

			if (!_currentUserService.IsSuperAdmin &&
			    !_currentUserService.IsKorisnik &&
			    (appointment.TenantId != _currentUserService.TenantId ||
			     facility.TenantId != _currentUserService.TenantId ||
			     appointmentType.TenantId != _currentUserService.TenantId))
			{
				return Forbid();
			}

			var resolvedTenantId = facility.TenantId;

			var exists = await _context.Appointment.AnyAsync(a =>
				a.Facility.Id == appointmentDto.FacilityId &&
				a.AppointmentDateTime == appointmentDto.AppointmentDateTime &&
				!a.IsDeleted && a.Id != id);

			if (exists)
				return BadRequest("Appointment already exists for selected time.");

			var assignedUserId = await ResolveAppointmentUserIdAsync(appointmentDto.UserId, resolvedTenantId);
			if (assignedUserId == null)
				return BadRequest("Invalid UserId");

			appointment.AppointmentDateTime = appointmentDto.AppointmentDateTime;
			appointment.IsFree = appointmentDto.IsFree;
			appointment.IsClosed = appointmentDto.IsClosed;
			appointment.Facility = facility;
			appointment.AppointmentType = appointmentType;
			appointment.UserId = assignedUserId.Value;
			appointment.TenantId = resolvedTenantId;

			_context.Entry(appointment).State = EntityState.Modified;

			try
			{
				await _context.SaveChangesAsync();
			}
			catch (DbUpdateConcurrencyException)
			{
				if (!AppointmentExists(id))
					return NotFound();
				else
					throw;
			}

			return NoContent();
		}

		// DELETE: api/Appointment/{id}
		[Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Uposlenik},{Roles.Korisnik}")]
		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteAppointment(int id)
		{
			var query = (_currentUserService.IsSuperAdmin || _currentUserService.IsKorisnik)
				? _context.Appointment.IgnoreQueryFilters()
				: _context.Appointment;

			var appointment = await query.FirstOrDefaultAsync(a => a.Id == id);
			if (appointment == null)
				return NotFound();

			if (_currentUserService.IsKorisnik && appointment.UserId != _currentUserService.UserId)
				return Forbid();

			if (!_currentUserService.IsKorisnik && !_currentUserService.CanAccessTenant(appointment.TenantId))
				return Forbid();

			appointment.IsDeleted = true;
			_context.Update(appointment);
			await _context.SaveChangesAsync();

			return NoContent();
		}

		private bool AppointmentExists(int id)
		{
			return _context.Appointment.Any(e => e.Id == id);
		}

		// GET: api/appointment/calendar
		[HttpGet("calendar")]
		public async Task<ActionResult<IEnumerable<AppointmentCalendarDto>>> GetCalendar(
			DateTime from,
			DateTime to,
			int? facilityId,
			int? tenantId)
		{
			if (_currentUserService.IsKorisnik && !tenantId.HasValue && !facilityId.HasValue)
				return BadRequest("TenantId ili facilityId su obavezni.");

			if (!_currentUserService.IsSuperAdmin &&
			    !_currentUserService.IsKorisnik &&
			    !_currentUserService.HasValidTenant)
				return Forbid();

			var query = ((_currentUserService.IsSuperAdmin || _currentUserService.IsKorisnik)
				? _context.Appointment.IgnoreQueryFilters()
				: _context.Appointment)
				.Include(a => a.Facility)
				.Include(a => a.AppointmentType)
				.Where(a =>
					a.AppointmentDateTime >= from &&
					a.AppointmentDateTime <= to &&
					!a.IsDeleted);

			if (facilityId.HasValue)
				query = query.Where(a => a.Facility.Id == facilityId);

			var effectiveTenantId = (_currentUserService.IsSuperAdmin || _currentUserService.IsKorisnik)
				? tenantId
				: _currentUserService.TenantId;
			if (effectiveTenantId.HasValue)
				query = query.Where(a => a.TenantId == effectiveTenantId.Value);

			var result = await query.Select(a => new AppointmentCalendarDto
			{
				Id = a.Id,
				Start = a.AppointmentDateTime,
				End = a.AppointmentDateTime.AddMinutes(a.AppointmentType.Duration),
				IsFree = a.IsFree,
				IsClosed = a.IsClosed,
				FacilityId = a.Facility.Id,
				FacilityName = a.Facility.Name,
				AppointmentTypeId = a.AppointmentType.Id,
				AppointmentTypeName = a.AppointmentType.Name,
				TenantId = a.TenantId,
				UserId = a.UserId,
				Price = (decimal)a.AppointmentType.Price
			}).ToListAsync();

			return Ok(result);
		}

		[Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin}")]
		[HttpGet("dashboard-stats")]
		public async Task<ActionResult<DashboardStatsDto>> GetDashboardStats([FromQuery] int? tenantId = null)
		{
			var today = DateTime.Today;
			var effectiveTenantId = _currentUserService.IsSuperAdmin ? tenantId : _currentUserService.TenantId;

			var query = _userManager.Users.AsQueryable();

			if (effectiveTenantId.HasValue)
				query = query.Where(u => u.TenantId == effectiveTenantId.Value);

			var stats = new DashboardStatsDto
			{
				TotalUsers = await query.CountAsync(),
				TotalAppointmentsToday = await _context.Appointment
				  .Where(a => a.AppointmentDateTime.Date == today.Date &&
						   !a.IsDeleted &&
						   (!effectiveTenantId.HasValue || a.TenantId == effectiveTenantId.Value))
				  .CountAsync(),
				TotalTenants = effectiveTenantId.HasValue ? 0 : await _context.Set<Tenant>().CountAsync(),
				TotalFacilities = await _context.Facility
				  .Where(f => !effectiveTenantId.HasValue || f.TenantId == effectiveTenantId.Value)
				  .CountAsync(),
				ActiveAppointments = await _context.Appointment
				  .Where(a => a.AppointmentDateTime >= DateTime.Now &&
						   !a.IsDeleted &&
						   (!effectiveTenantId.HasValue || a.TenantId == effectiveTenantId.Value))
				  .CountAsync()
			};

			return Ok(stats);
		}

		private async Task<int?> ResolveAppointmentUserIdAsync(int requestedUserId, int tenantId)
		{
			if (_currentUserService.IsKorisnik)
				return _currentUserService.UserId;

			if (requestedUserId <= 0)
				return null;

			var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == requestedUserId);
			if (user == null)
				return null;

			return user.TenantId == tenantId &&
			       (_currentUserService.IsSuperAdmin || _currentUserService.CanAccessTenant(user.TenantId))
				? user.Id
				: null;
		}

		private static AppointmentDto MapAppointment(Appointment appointment)
		{
			return new AppointmentDto
			{
				Id = appointment.Id,
				AppointmentDateTime = appointment.AppointmentDateTime,
				IsFree = appointment.IsFree,
				IsClosed = appointment.IsClosed,
				FacilityId = appointment.Facility?.Id ?? 0,
				FacilityName = appointment.Facility?.Name,
				AppointmentTypeId = appointment.AppointmentType?.Id ?? 0,
				AppointmentTypeName = appointment.AppointmentType?.Name,
				UserId = appointment.UserId,
				TenantId = appointment.TenantId
			};
		}
	}

	public class AppointmentDto
	{
		public int Id { get; set; }
		public DateTime AppointmentDateTime { get; set; }
		public bool IsFree { get; set; }
		public bool IsClosed { get; set; }
		public int FacilityId { get; set; }
		public string FacilityName { get; set; }
		public int AppointmentTypeId { get; set; }
		public string AppointmentTypeName { get; set; }
		public int UserId { get; set; }
		public int TenantId { get; set; }
	}

	public class DashboardStatsDto
	{
		public int TotalUsers { get; set; }
		public int TotalAppointmentsToday { get; set; }
		public int TotalTenants { get; set; }
		public int TotalFacilities { get; set; }
		public int ActiveAppointments { get; set; }
	}

	public class AppointmentCreateDto
	{
		public DateTime AppointmentDateTime { get; set; }
		public bool IsFree { get; set; }
		public bool IsClosed { get; set; }
		public int FacilityId { get; set; }
		public int AppointmentTypeId { get; set; }
		public int UserId { get; set; }
		public int TenantId { get; set; }
	}

	public class AppointmentCalendarDto
	{
		public int Id { get; set; }
		public DateTime Start { get; set; }
		public DateTime End { get; set; }
		public bool IsFree { get; set; }
		public bool IsClosed { get; set; }
		public int FacilityId { get; set; }
		public string FacilityName { get; set; }
		public int AppointmentTypeId { get; set; }
		public int UserId { get; set; }
		public int TenantId { get; set; }
		public string AppointmentTypeName { get; set; }
		public decimal Price { get; set; }
	}

	public class AppointmentCreateDtoValidator :AbstractValidator<AppointmentCreateDto>
	{
		public AppointmentCreateDtoValidator()
		{
			RuleFor(x => x.FacilityId)
				.GreaterThan(0).WithMessage("FacilityId mora biti validan.");

			RuleFor(x => x.AppointmentTypeId)
				.GreaterThan(0).WithMessage("AppointmentTypeId mora biti validan.");

			RuleFor(x => x.UserId)
				.GreaterThan(0).WithMessage("UserId mora biti validan.");
		}
	}
}
