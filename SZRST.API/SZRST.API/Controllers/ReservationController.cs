using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Entities;
using Infrastructure.Persistance;
using System;

namespace SZRST.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReservationController : ControllerBase
    {
        private readonly SZRSTContext _context;

        public ReservationController(SZRSTContext context)
        {
            _context = context;
        }

        // GET: api/Reservation
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Reservation>>> GetReservations()
        {
            return await _context.Reservation
                                 .Include(r => r.User)              // Include related User
                                 .Include(r => r.Appointment)       // Include related Appointment
                                 .ToListAsync();
        }

        // GET: api/Reservation/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Reservation>> GetReservation(int id)
        {
            var reservation = await _context.Reservation
                                            .Include(r => r.User)              // Include related User
                                            .Include(r => r.Appointment)       // Include related Appointment
                                            .FirstOrDefaultAsync(r => r.Id == id);

            if (reservation == null)
            {
                return NotFound();
            }

            return reservation;
        }

        // POST: api/Reservation
        [HttpPost]
        public async Task<ActionResult<Reservation>> CreateReservation([FromBody] ReservationCreateDto reservationDto)
        {
            var user = await _context.Users.FindAsync(reservationDto.UserId);
            if (user == null)
            {
                return BadRequest("Invalid UserId");
            }

            var appointment = await _context.Appointment.FindAsync(reservationDto.AppointmentId);
            if (appointment == null)
            {
                return BadRequest("Invalid AppointmentId");
            }

            var reservation = new Reservation
            {
                ReservationDateTime = reservationDto.ReservationDateTime,
                Message = reservationDto.Message,
                User = user,
                Appointment = appointment,
                IsDeleted = false
            };

            _context.Reservation.Add(reservation);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetReservation), new { id = reservation.Id }, reservation);
        }

        // PUT: api/Reservation/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateReservation(int id, [FromBody] ReservationCreateDto reservationDto)
        {
            var reservation = await _context.Reservation.FindAsync(id);
            if (reservation == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(reservationDto.UserId);
            if (user == null)
            {
                return BadRequest("Invalid UserId");
            }

            var appointment = await _context.Appointment.FindAsync(reservationDto.AppointmentId);
            if (appointment == null)
            {
                return BadRequest("Invalid AppointmentId");
            }

            reservation.ReservationDateTime = reservationDto.ReservationDateTime;
            reservation.Message = reservationDto.Message;
            reservation.User = user;
            reservation.Appointment = appointment;

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
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReservation(int id)
        {
            var reservation = await _context.Reservation.FindAsync(id);
            if (reservation == null)
            {
                return NotFound();
            }

            _context.Reservation.Remove(reservation);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ReservationExists(int id)
        {
            return _context.Reservation.Any(e => e.Id == id);
        }
    }

    public class ReservationCreateDto
    {
        public DateTime ReservationDateTime { get; set; }
        public string Message { get; set; }
        public int UserId { get; set; }
        public int AppointmentId { get; set; }
    }
}
