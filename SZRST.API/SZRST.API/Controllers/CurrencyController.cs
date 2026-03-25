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
	public class CurrencyController :ControllerBase
	{
		private readonly SZRSTContext _context;

		public CurrencyController(SZRSTContext context)
		{
			_context = context;
		}

		[HttpGet]
		public async Task<ActionResult<IEnumerable<CurrencyResponseDto>>> GetCurrencies()
		{
			var currencies = await _context.Currency
				.Where(c => !c.IsDeleted)
				.Select(c => new CurrencyResponseDto
				{
					Id = c.Id,
					Name = c.Name,
					ShortName = c.ShortName
				})
				.ToListAsync();

			return Ok(currencies);
		}

		[HttpGet("{id}")]
		public async Task<ActionResult<CurrencyResponseDto>> GetCurrency(int id)
		{
			var currency = await _context.Currency.FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

			if (currency == null)
			{
				return NotFound();
			}

			return new CurrencyResponseDto
			{
				Id = currency.Id,
				Name = currency.Name,
				ShortName = currency.ShortName
			};
		}

		[HttpPost]
		public async Task<ActionResult<CurrencyResponseDto>> PostCurrency(CurrencyCreateDto currencyDto)
		{
			var currency = new Currency
			{
				Name = currencyDto.Name,
				ShortName = currencyDto.ShortName,
				IsDeleted = false
			};

			_context.Currency.Add(currency);
			await _context.SaveChangesAsync();

			return CreatedAtAction(nameof(GetCurrencies), new { id = currency.Id }, new CurrencyResponseDto
			{
				Id = currency.Id,
				Name = currency.Name,
				ShortName = currency.ShortName
			});
		}

		[HttpPut("{id}")]
		public async Task<IActionResult> PutCurrency(int id, Currency currency)
		{
			if (id != currency.Id)
			{
				return BadRequest();
			}

			_context.Entry(currency).State = EntityState.Modified;

			try
			{
				await _context.SaveChangesAsync();
			}
			catch (DbUpdateConcurrencyException)
			{
				if (!CurrencyExists(id))
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

		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteCurrency(int id)
		{
			var currency = await _context.Currency.FindAsync(id);
			if (currency == null)
			{
				return NotFound();
			}

			currency.IsDeleted = true;

			try
			{
				await _context.SaveChangesAsync();
			}
			catch (DbUpdateException)
			{
				return BadRequest(new { message = "Nije moguće obrisati valutu jer se koristi u postojećim zapisima." });
			}

			return NoContent();
		}

		private bool CurrencyExists(int id)
		{
			return _context.Currency.Any(e => e.Id == id && !e.IsDeleted);
		}

		public class CurrencyCreateDto
		{
			public string Name { get; set; }
			public string ShortName { get; set; }
		}

		public class CurrencyResponseDto
		{
			public int Id { get; set; }
			public string Name { get; set; }
			public string ShortName { get; set; }
		}
	}
}
