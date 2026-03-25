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
	public class FacilityTypeController :ControllerBase
	{
		private readonly SZRSTContext _context;

		public FacilityTypeController(SZRSTContext context)
		{
			_context = context;
		}

		// GET: api/FacilityType
		[HttpGet]
		public async Task<ActionResult<IEnumerable<FacilityTypeResponseDto>>> GetFacilityTypes()
		{
			var facilityTypes = await _context.FacilityType
							 .Where(ft => !ft.IsDeleted)
							 .Select(ft => new FacilityTypeResponseDto
							 {
								 Id = ft.Id,
								 Name = ft.Name,
								 Description = ft.Description
							 })
							 .ToListAsync();

			return Ok(facilityTypes);
		}

		// GET: api/FacilityType/{id}
		[HttpGet("{id}")]
		public async Task<ActionResult<FacilityTypeResponseDto>> GetFacilityType(int id)
		{
			var facilityType = await _context.FacilityType
									    .FirstOrDefaultAsync(ft => ft.Id == id && !ft.IsDeleted);

			if (facilityType == null)
			{
				return NotFound();
			}

			return new FacilityTypeResponseDto
			{
				Id = facilityType.Id,
				Name = facilityType.Name,
				Description = facilityType.Description
			};
		}

		// POST: api/FacilityType
		[HttpPost]
		public async Task<ActionResult<FacilityTypeResponseDto>> CreateFacilityType([FromBody] FacilityTypeCreateDto facilityTypeDto)
		{
			var facilityType = new FacilityType
			{
				Name = facilityTypeDto.Name,
				Description = facilityTypeDto.Description,
				IsDeleted = false
			};

			_context.FacilityType.Add(facilityType);
			await _context.SaveChangesAsync();

			return CreatedAtAction(nameof(GetFacilityType), new { id = facilityType.Id }, new FacilityTypeResponseDto
			{
				Id = facilityType.Id,
				Name = facilityType.Name,
				Description = facilityType.Description
			});
		}

		// PUT: api/FacilityType/{id}
		[HttpPut("{id}")]
		public async Task<IActionResult> UpdateFacilityType(int id, [FromBody] FacilityTypeCreateDto facilityTypeDto)
		{
			var facilityType = await _context.FacilityType.FindAsync(id);
			if (facilityType == null)
			{
				return NotFound();
			}

			facilityType.Name = facilityTypeDto.Name;
			facilityType.Description = facilityTypeDto.Description;

			_context.Entry(facilityType).State = EntityState.Modified;

			try
			{
				await _context.SaveChangesAsync();
			}
			catch (DbUpdateConcurrencyException)
			{
				if (!FacilityTypeExists(id))
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

		// DELETE: api/FacilityType/{id}
		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteFacilityType(int id)
		{
			var facilityType = await _context.FacilityType.FindAsync(id);
			if (facilityType == null)
			{
				return NotFound();
			}

			facilityType.IsDeleted = true;

			try
			{
				await _context.SaveChangesAsync();
			}
			catch (DbUpdateException)
			{
				return BadRequest(new { message = "Nije moguće obrisati tip objekta jer se koristi u postojećim zapisima." });
			}

			return NoContent();
		}

		private bool FacilityTypeExists(int id)
		{
			return _context.FacilityType.Any(e => e.Id == id && !e.IsDeleted);
		}
	}

	public class FacilityTypeCreateDto
	{
		public string Name { get; set; }
		public string Description { get; set; }
	}

	public class FacilityTypeResponseDto
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
	}
}
