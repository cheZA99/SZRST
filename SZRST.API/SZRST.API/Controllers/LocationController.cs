using Domain.Entities;
using Infrastructure.Persistance;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SZRST.Domain.Constants;
using SZRST.Shared;

namespace SZRST.API.Controllers
{
	[Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin}, {Roles.Uposlenik}")]
	[Authorize]
	[Route("api/[controller]")]
	[ApiController]
	public class LocationController :ControllerBase
	{
		private readonly SZRSTContext _context;

		public LocationController(SZRSTContext context)
		{
			_context = context;
		}

		// GET: api/Location
		[HttpGet]
		public async Task<ActionResult<IEnumerable<Location>>> GetLocations()
		{
			return await _context.Location
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
									.FirstOrDefaultAsync(l => l.Id == id);

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
			var country = await _context.Country.FindAsync(locationDto.CountryId);
			if (country == null)
			{
				return BadRequest("Invalid CountryId");
			}

			var city = await _context.City.FindAsync(locationDto.CityId);
			if (city == null)
			{
				return BadRequest("Invalid CityId");
			}

			var location = new Location
			{
				Address = locationDto.Address,
				AddressNumber = locationDto.AddressNumber,
				Country = country,
				City = city,
				IsDeleted = false
			};

			_context.Location.Add(location);
			await _context.SaveChangesAsync();

			return CreatedAtAction(nameof(GetLocation), new { id = location.Id }, location);
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

			_context.Location.Remove(location);
			await _context.SaveChangesAsync();

			return NoContent();
		}

		private bool LocationExists(int id)
		{
			return _context.Location.Any(e => e.Id == id);
		}
	}
}