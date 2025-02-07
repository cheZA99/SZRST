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
    public class AppointmentTypeController : ControllerBase
    {
        private readonly SZRSTContext _context;

        public AppointmentTypeController(SZRSTContext context)
        {
            _context = context;
        }

        // GET: api/AppointmentType
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AppointmentType>>> GetAppointmentTypes()
        {
            return await _context.AppointmentType
                                 .ToListAsync();
        }

        // GET: api/AppointmentType/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<AppointmentType>> GetAppointmentType(int id)
        {
            var appointmentType = await _context.AppointmentType
                                                .FirstOrDefaultAsync(at => at.Id == id);

            if (appointmentType == null)
            {
                return NotFound();
            }

            return appointmentType;
        }

        // POST: api/AppointmentType
        [HttpPost]
        public async Task<ActionResult<AppointmentType>> CreateAppointmentType([FromBody] AppointmentTypeCreateDto appointmentTypeDto)
        {
            var appointmentType = new AppointmentType
            {
                Name = appointmentTypeDto.Name,
                Duration = appointmentTypeDto.Duration,
                Price = appointmentTypeDto.Price
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
            if (appointmentType == null)
            {
                return NotFound();
            }

            appointmentType.Name = appointmentTypeDto.Name;
            appointmentType.Duration = appointmentTypeDto.Duration;
            appointmentType.Price = appointmentTypeDto.Price;

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

            _context.AppointmentType.Remove(appointmentType);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool AppointmentTypeExists(int id)
        {
            return _context.AppointmentType.Any(e => e.Id == id);
        }
    }

    public class AppointmentTypeCreateDto
    {
        public string Name { get; set; }
        public int Duration { get; set; }
        public float Price { get; set; }
    }
}
