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
	[Authorize]
	[Route("api/[controller]")]
	[ApiController]
	public class ReservationController :ControllerBase
	{
		private readonly SZRSTContext _context;
		private readonly ICurrentUserService _currentUserService;

		public ReservationController(SZRSTContext context, ICurrentUserService currentUserService)
		{
			_context = context;
			_currentUserService = currentUserService;
		}

		// GET: api/Reservation
		[Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Uposlenik}")]
		[HttpGet]
		public async Task<ActionResult<IEnumerable<ReservationDto>>> GetReservations()
		{
			var reservations = await _context.Reservation
							 .Include(r => r.User)
							 .Include(r => r.Appointment)
							 .Where(r => !r.IsDeleted)
							 .Select(r => new ReservationDto
							 {
								 Id = r.Id,
								 ReservationDateTime = r.ReservationDateTime,
								 Message = r.Message,
								 UserId = r.User.Id,
								 UserName = r.User.UserName,
								 AppointmentId = r.Appointment.Id,
								 AppointmentDateTime = r.Appointment.AppointmentDateTime,
								 TenantId = r.TenantId
							 })
							 .ToListAsync();

			return Ok(reservations);
		}

		// GET: api/Reservation/{id}
		[Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Uposlenik}")]
		[HttpGet("{id}")]
		public async Task<ActionResult<ReservationDto>> GetReservation(int id)
		{
			var reservation = await _context.Reservation
									  .Include(r => r.User)
									  .Include(r => r.Appointment)
									  .FirstOrDefaultAsync(r => r.Id == id);

			if (reservation == null)
			{
				return NotFound();
			}

			return Ok(MapReservation(reservation));
		}

		// POST: api/Reservation
		[Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Uposlenik},{Roles.Korisnik}")]
		[HttpPost]
		public async Task<ActionResult<ReservationDto>> CreateReservation([FromBody] ReservationCreateDto reservationDto)
		{
			if (!_currentUserService.HasValidTenant)
				return Forbid();

			var userId = _currentUserService.IsSuperAdmin ? reservationDto.UserId : _currentUserService.UserId;

			var user = await _context.Users.FindAsync(userId);
			if (user == null)
			{
				return BadRequest("Invalid UserId");
			}

			var appointment = await _context.Appointment.FindAsync(reservationDto.AppointmentId);
			if (appointment == null)
			{
				return BadRequest("Invalid AppointmentId");
			}

			if (!_currentUserService.CanAccessTenant(user.TenantId) ||
			    !_currentUserService.CanAccessTenant(appointment.TenantId) ||
			    user.TenantId != appointment.TenantId)
			{
				return Forbid();
			}

			var reservation = new Reservation
			{
				ReservationDateTime = reservationDto.ReservationDateTime,
				Message = reservationDto.Message,
				User = user,
				Appointment = appointment,
				IsDeleted = false,
				TenantId = appointment.TenantId
			};

			_context.Reservation.Add(reservation);
			await _context.SaveChangesAsync();

			return CreatedAtAction(nameof(GetReservation), new { id = reservation.Id }, MapReservation(reservation));
		}

		// PUT: api/Reservation/{id}
		[Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Uposlenik},{Roles.Korisnik}")]
		[HttpPut("{id}")]
		public async Task<IActionResult> UpdateReservation(int id, [FromBody] ReservationCreateDto reservationDto)
		{
			var reservation = await _context.Reservation.FindAsync(id);
			if (reservation == null)
			{
				return NotFound();
			}

			if (!_currentUserService.CanAccessTenant(reservation.TenantId))
				return Forbid();

			var userId = _currentUserService.IsSuperAdmin ? reservationDto.UserId : _currentUserService.UserId;
			var user = await _context.Users.FindAsync(userId);
			if (user == null)
			{
				return BadRequest("Invalid UserId");
			}

			var appointment = await _context.Appointment.FindAsync(reservationDto.AppointmentId);
			if (appointment == null)
			{
				return BadRequest("Invalid AppointmentId");
			}

			if (!_currentUserService.CanAccessTenant(user.TenantId) ||
			    !_currentUserService.CanAccessTenant(appointment.TenantId) ||
			    user.TenantId != appointment.TenantId)
			{
				return Forbid();
			}

			reservation.ReservationDateTime = reservationDto.ReservationDateTime;
			reservation.Message = reservationDto.Message;
			reservation.User = user;
			reservation.Appointment = appointment;
			reservation.TenantId = appointment.TenantId;

			_context.Entry(reservation).State = EntityState.Modified;

			try
			{
				await _context.SaveChangesAsync();
			}
			catch (DbUpdateConcurrencyException)
			{
				if (!ReservationExists(id))
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

		// DELETE: api/Reservation/{id}
		[Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Uposlenik},{Roles.Korisnik}")]
		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteReservation(int id)
		{
			var reservation = await _context.Reservation.FindAsync(id);
			if (reservation == null)
			{
				return NotFound();
			}

			if (!_currentUserService.CanAccessTenant(reservation.TenantId))
				return Forbid();

			reservation.IsDeleted = true;
			await _context.SaveChangesAsync();

			return NoContent();
		}

		private bool ReservationExists(int id)
		{
			return _context.Reservation.Any(e => e.Id == id);
		}

		private static ReservationDto MapReservation(Reservation reservation)
		{
			return new ReservationDto
			{
				Id = reservation.Id,
				ReservationDateTime = reservation.ReservationDateTime,
				Message = reservation.Message,
				UserId = reservation.User?.Id ?? 0,
				UserName = reservation.User?.UserName,
				AppointmentId = reservation.Appointment?.Id ?? 0,
				AppointmentDateTime = reservation.Appointment?.AppointmentDateTime ?? default,
				TenantId = reservation.TenantId
			};
		}
	}

	public class ReservationDto
	{
		public int Id { get; set; }
		public DateTime ReservationDateTime { get; set; }
		public string Message { get; set; }
		public int UserId { get; set; }
		public string UserName { get; set; }
		public int AppointmentId { get; set; }
		public DateTime AppointmentDateTime { get; set; }
		public int TenantId { get; set; }
	}

	public class ReservationCreateDto
	{
		public DateTime ReservationDateTime { get; set; }
		public string Message { get; set; }
		public int UserId { get; set; }
		public int AppointmentId { get; set; }
	}
}
