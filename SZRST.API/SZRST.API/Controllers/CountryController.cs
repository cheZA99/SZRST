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
	[Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin}, {Roles.Uposlenik}")]
	[Authorize]
	[Route("api/[controller]")]
	[ApiController]
	public class CountryController :ControllerBase
	{
		private readonly SZRSTContext _context;

		public CountryController(SZRSTContext context)
		{
			_context = context;
		}

		// GET: api/Country
		[HttpGet]
		public async Task<ActionResult<IEnumerable<Country>>> GetCountries()
		{
			return await _context.Country
							 .Where(c => !c.IsDeleted)
							 .Include(c => c.Currency)
							 .ToListAsync();
		}

		// GET: api/Country/{id}
		[HttpGet("{id}")]
		public async Task<ActionResult<Country>> GetCountry(int id)
		{
			var country = await _context.Country
								    .Where(c => !c.IsDeleted)
								    .Include(c => c.Currency)
								    .FirstOrDefaultAsync(c => c.Id == id);

			if (country == null)
			{
				return NotFound();
			}

			return country;
		}

		// POST: api/Country
		[HttpPost]
		public async Task<ActionResult<Country>> CreateCountry([FromBody] CountryCreateDto countryDto)
		{
			var currency = await _context.Currency.FindAsync(countryDto.CurrencyId);
			if (countryDto.CurrencyId.HasValue && currency == null)
			{
				return BadRequest("Invalid CurrencyId");
			}

			var country = new Country
			{
				Name = countryDto.Name,
				ShortName = countryDto.ShortName,
				Currency = currency,
				IsDeleted = false
			};

			_context.Country.Add(country);
			await _context.SaveChangesAsync();

			return CreatedAtAction(nameof(GetCountry), new { id = country.Id }, country);
		}

		// PUT: api/Country/{id}
		[HttpPut("{id}")]
		public async Task<IActionResult> UpdateCountry(int id, [FromBody] CountryCreateDto countryDto)
		{
			var country = await _context.Country.FindAsync(id);
			if (country == null || country.IsDeleted)
			{
				return NotFound();
			}

			var currency = await _context.Currency.FindAsync(countryDto.CurrencyId);
			if (countryDto.CurrencyId.HasValue && currency == null)
			{
				return BadRequest("Invalid CurrencyId");
			}

			country.Name = countryDto.Name;
			country.ShortName = countryDto.ShortName;
			country.Currency = currency;

			_context.Entry(country).State = EntityState.Modified;

			try
			{
				await _context.SaveChangesAsync();
			}
			catch (DbUpdateConcurrencyException)
			{
				if (!CountryExists(id))
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

		// DELETE: api/Country/{id} (Soft Delete)
		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteCountry(int id)
		{
			var country = await _context.Country.FindAsync(id);
			if (country == null)
			{
				return NotFound();
			}

			_context.Country.Remove(country);
			await _context.SaveChangesAsync();

			return NoContent();
		}

		private bool CountryExists(int id)
		{
			return _context.Country.Any(e => e.Id == id && !e.IsDeleted);
		}
	}

	public class CountryCreateDto
	{
		public string Name { get; set; }
		public string ShortName { get; set; }
		public int? CurrencyId { get; set; }
	}
}