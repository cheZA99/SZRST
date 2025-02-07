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
    public class AppointmentController : ControllerBase
    {
        private readonly SZRSTContext _context;

        public AppointmentController(SZRSTContext context)
        {
            _context = context;
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
                IsDeleted = false
            };

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

            appointment.AppointmentDateTime = appointmentDto.AppointmentDateTime;
            appointment.IsFree = appointmentDto.IsFree;
            appointment.IsClosed = appointmentDto.IsClosed;
            appointment.Facility = facility;
            appointment.AppointmentType = appointmentType;

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

            _context.Appointment.Remove(appointment);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool AppointmentExists(int id)
        {
            return _context.Appointment.Any(e => e.Id == id);
        }
    }

    public class AppointmentCreateDto
    {
        public DateTime AppointmentDateTime { get; set; }
        public bool IsFree { get; set; }
        public bool IsClosed { get; set; }
        public int FacilityId { get; set; }
        public int AppointmentTypeId { get; set; }
    }
}
