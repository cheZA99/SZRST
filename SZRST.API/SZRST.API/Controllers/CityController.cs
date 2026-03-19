using Domain.Entities;
using Infrastructure.Persistance;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SZRST.Domain.Constants;

namespace SZRST.API.Controllers
{
	[Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Uposlenik}")]
	[Route("api/[controller]")]
	[ApiController]
	public class CityController :ControllerBase
	{
		private readonly SZRSTContext _context;

		public CityController(SZRSTContext context)
		{
			_context = context;
		}

		// GET: api/City
		[HttpGet]
		public async Task<ActionResult<IEnumerable<CityDto>>> GetCities()
		{
			return Ok(await _context.City
							 .Include(c => c.Country)
							 .Select(c => new CityDto
							 {
								 Id = c.Id,
								 Name = c.Name,
								 CountryId = c.Country.Id,
								 CountryName = c.Country.Name,
								 IsDeleted = c.IsDeleted
							 })
							 .ToListAsync());
		}

		// GET: api/City/{id}
		[HttpGet("{id}")]
		public async Task<ActionResult<CityDto>> GetCity(int id)
		{
			var city = await _context.City
								 .Include(c => c.Country)
								 .Where(c => c.Id == id)
								 .Select(c => new CityDto
								 {
									 Id = c.Id,
									 Name = c.Name,
									 CountryId = c.Country.Id,
									 CountryName = c.Country.Name,
									 IsDeleted = c.IsDeleted
								 })
								 .FirstOrDefaultAsync();

			if (city == null)
			{
				return NotFound();
			}

			return city;
		}

		// POST: api/City
		[HttpPost]
		public async Task<ActionResult<City>> CreateCity([FromBody] CityCreateDto cityDto)
		{
			var country = await _context.Country.FindAsync(cityDto.CountryId);
			if (country == null)
			{
				return BadRequest("Invalid CountryId");
			}

			var city = new City
			{
				Name = cityDto.Name,
				Country = country,
				IsDeleted = cityDto.IsDeleted
			};

			_context.City.Add(city);
			await _context.SaveChangesAsync();

			return city;
		}

		// PUT: api/City/{id}
		[HttpPut("{id}")]
		public async Task<IActionResult> UpdateCity(int id, [FromBody] CityCreateDto cityDto)
		{
			var city = await _context.City.FindAsync(id);
			if (city == null)
			{
				return NotFound();
			}

			var country = await _context.Country.FindAsync(cityDto.CountryId);
			if (country == null)
			{
				return BadRequest("Invalid CountryId");
			}

			city.Name = cityDto.Name;
			city.Country = country;
			city.IsDeleted = cityDto.IsDeleted;

			_context.Entry(city).State = EntityState.Modified;

			try
			{
				await _context.SaveChangesAsync();
			}
			catch (DbUpdateConcurrencyException)
			{
				if (!CityExists(id))
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

		// DELETE: api/City/{id}
		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteCity(int id)
		{
			var city = await _context.City.FindAsync(id);
			if (city == null)
			{
				return NotFound();
			}

			_context.City.Remove(city);
			await _context.SaveChangesAsync();

			return NoContent();
		}

		private bool CityExists(int id)
		{
			return _context.City.Any(e => e.Id == id);
		}
	}

	public class CityCreateDto
	{
		public string Name { get; set; }
		public int CountryId { get; set; }
		public bool IsDeleted { get; set; }
	}

	public class CityDto
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public int CountryId { get; set; }
		public string CountryName { get; set; }
		public bool IsDeleted { get; set; }
	}
}
