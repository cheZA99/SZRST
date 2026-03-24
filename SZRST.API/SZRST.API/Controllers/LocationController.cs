using Domain.Entities;
using Infrastructure.Persistance;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SZRST.API.Services;
using SZRST.Domain.Constants;
using SZRST.Shared;

namespace SZRST.API.Controllers
{
	[Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Uposlenik}")]
	[Route("api/[controller]")]
	[ApiController]
	public class LocationController :ControllerBase
	{
		private readonly SZRSTContext _context;
		private readonly ILocationService _locationService;

		public LocationController(SZRSTContext context, ILocationService locationService)
		{
			_context = context;
			_locationService = locationService;
		}

		// GET: api/Location
		[HttpGet]
		public async Task<ActionResult<IEnumerable<Location>>> GetLocations()
		{
			return await _context.Location
							 .Where(l => !l.IsDeleted)
							 .Include(l => l.Country)
							 .Include(l => l.City)
							 .ToListAsync();
		}

		// GET: api/Location/{id}
		[HttpGet("{id}")]
		public async Task<ActionResult<Location>> GetLocation(int id)
		{
			var location = await _context.Location
									.Include(l => l.Country)
									.Include(l => l.City)
									.FirstOrDefaultAsync(l => l.Id == id && !l.IsDeleted);

			if (location == null)
			{
				return NotFound();
			}

			return location;
		}

		// POST: api/Location
		[HttpPost]
		public async Task<ActionResult<Location>> CreateLocation([FromBody] LocationCreateDto locationDto)
		{
			var locationResult = await _locationService.CreateLocationAsync(locationDto);
			if (!locationResult.IsSuccess)
			{
				return BadRequest(locationResult.ErrorMessage);
			}

			return CreatedAtAction(nameof(GetLocation), new { id = locationResult.Location.Id }, locationResult.Location);
		}

		// PUT: api/Location/{id}
		[HttpPut("{id}")]
		public async Task<IActionResult> UpdateLocation(int id, Location location)
		{
			if (id != location.Id)
			{
				return BadRequest();
			}

			_context.Entry(location).State = EntityState.Modified;

			try
			{
				await _context.SaveChangesAsync();
			}
			catch (DbUpdateConcurrencyException)
			{
				if (!LocationExists(id))
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

		// DELETE: api/Location/{id}
		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteLocation(int id)
		{
			var location = await _context.Location.FindAsync(id);
			if (location == null)
			{
				return NotFound();
			}

			location.IsDeleted = true;

			try
			{
				await _context.SaveChangesAsync();
			}
			catch (DbUpdateException)
			{
				return BadRequest(new { message = "Nije moguće obrisati lokaciju jer se koristi u postojećim zapisima." });
			}

			return NoContent();
		}

		private bool LocationExists(int id)
		{
			return _context.Location.Any(e => e.Id == id && !e.IsDeleted);
		}
	}
}
