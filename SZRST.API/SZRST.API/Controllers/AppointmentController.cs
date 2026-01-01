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
	[Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin}, {Roles.Uposlenik}, {Roles.Korisnik}")]
	[Authorize]
	[Route("api/[controller]")]
	[ApiController]
	public class AppointmentController :ControllerBase
	{
		private readonly SZRSTContext _context;
		private readonly ICurrentUserService _currentUserService;

		public AppointmentController(SZRSTContext context, ICurrentUserService currentUserService)
		{
			_context = context;
			_currentUserService = currentUserService;
		}

		// GET: api/Appointment
		[HttpGet]
		public async Task<ActionResult<IEnumerable<Appointment>>> GetAppointments()
		{
			return await _context.Appointment
							 .Include(a => a.Facility)        // Include related Facility
							 .Include(a => a.AppointmentType) // Include related AppointmentType
							 .ToListAsync();
		}

		// GET: api/Appointment/{id}
		[HttpGet("{id}")]
		public async Task<ActionResult<Appointment>> GetAppointment(int id)
		{
			var appointment = await _context.Appointment
									  .Include(a => a.Facility)        // Include related Facility
									  .Include(a => a.AppointmentType) // Include related AppointmentType
									  .FirstOrDefaultAsync(a => a.Id == id);

			if (appointment == null)
			{
				return NotFound();
			}

			return appointment;
		}

		// POST: api/Appointment
		[HttpPost]
		public async Task<ActionResult<Appointment>> CreateAppointment([FromBody] AppointmentCreateDto appointmentDto)
		{
			var facility = await _context.Facility.FindAsync(appointmentDto.FacilityId);
			if (facility == null)
			{
				return BadRequest("Invalid FacilityId");
			}

			var appointmentType = await _context.AppointmentType.FindAsync(appointmentDto.AppointmentTypeId);
			if (appointmentType == null)
			{
				return BadRequest("Invalid AppointmentTypeId");
			}

			var appointment = new Appointment
			{
				AppointmentDateTime = appointmentDto.AppointmentDateTime,
				IsFree = appointmentDto.IsFree,
				IsClosed = appointmentDto.IsClosed,
				Facility = facility,
				AppointmentType = appointmentType,
				IsDeleted = false,
				UserId = _currentUserService.Role != Roles.Korisnik ? appointmentDto.UserId : _currentUserService.UserId,
				TenantId = appointmentType.TenantId
			};

			var exists = await _context.Appointment.AnyAsync(a =>
	a.Facility.Id == appointmentDto.FacilityId &&
	a.AppointmentDateTime == appointmentDto.AppointmentDateTime &&
	!a.IsDeleted);

			if (exists)
			{
				return BadRequest("Appointment already exists for selected time.");
			}

			_context.Appointment.Add(appointment);
			await _context.SaveChangesAsync();

			return CreatedAtAction(nameof(GetAppointment), new { id = appointment.Id }, appointment);
		}

		// PUT: api/Appointment/{id}
		[HttpPut("{id}")]
		public async Task<IActionResult> UpdateAppointment(int id, [FromBody] AppointmentCreateDto appointmentDto)
		{
			var appointment = await _context.Appointment.FindAsync(id);
			if (appointment == null)
			{
				return NotFound();
			}

			if (appointment.IsClosed)
			{
				return BadRequest("Closed appointment cannot be edited.");
			}

			var facility = await _context.Facility.FindAsync(appointmentDto.FacilityId);
			if (facility == null)
			{
				return BadRequest("Invalid FacilityId");
			}

			var appointmentType = await _context.AppointmentType.FindAsync(appointmentDto.AppointmentTypeId);
			if (appointmentType == null)
			{
				return BadRequest("Invalid AppointmentTypeId");
			}

			var exists = await _context.Appointment.AnyAsync(a =>
	a.Facility.Id == appointmentDto.FacilityId &&
	a.AppointmentDateTime == appointmentDto.AppointmentDateTime &&
	!a.IsDeleted && a.Id != id);

			if (exists)
			{
				return BadRequest("Appointment already exists for selected time.");
			}

			appointment.AppointmentDateTime = appointmentDto.AppointmentDateTime;
			appointment.IsFree = appointmentDto.IsFree;
			appointment.IsClosed = appointmentDto.IsClosed;
			appointment.Facility = facility;
			appointment.AppointmentType = appointmentType;
			appointment.UserId = _currentUserService.Role != Roles.Korisnik ? appointmentDto.UserId : _currentUserService.UserId;
			appointment.TenantId = appointmentDto.TenantId;

			_context.Entry(appointment).State = EntityState.Modified;

			try
			{
				await _context.SaveChangesAsync();
			}
			catch (DbUpdateConcurrencyException)
			{
				if (!AppointmentExists(id))
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

		// DELETE: api/Appointment/{id}
		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteAppointment(int id)
		{
			var appointment = await _context.Appointment.FindAsync(id);
			if (appointment == null)
			{
				return NotFound();
			}

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
			var query = _context.Appointment
				.Include(a => a.Facility)
				.Include(a => a.AppointmentType)
				.Where(a =>
					a.AppointmentDateTime >= from &&
					a.AppointmentDateTime <= to &&
					!a.IsDeleted);

			if (facilityId.HasValue)
			{
				query = query.Where(a => a.Facility.Id == facilityId);
			}

			if (tenantId.HasValue)
			{
				query = query.Where(a => a.TenantId == tenantId);
			}

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
}