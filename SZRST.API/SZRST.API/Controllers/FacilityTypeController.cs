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
	public class FacilityTypeController :ControllerBase
	{
		private readonly SZRSTContext _context;

		public FacilityTypeController(SZRSTContext context)
		{
			_context = context;
		}

		// GET: api/FacilityType
		[HttpGet]
		public async Task<ActionResult<IEnumerable<FacilityType>>> GetFacilityTypes()
		{
			return await _context.FacilityType
							 .ToListAsync();
		}

		// GET: api/FacilityType/{id}
		[HttpGet("{id}")]
		public async Task<ActionResult<FacilityType>> GetFacilityType(int id)
		{
			var facilityType = await _context.FacilityType
									    .FirstOrDefaultAsync(ft => ft.Id == id);

			if (facilityType == null)
			{
				return NotFound();
			}

			return facilityType;
		}

		// POST: api/FacilityType
		[HttpPost]
		public async Task<ActionResult<FacilityType>> CreateFacilityType([FromBody] FacilityTypeCreateDto facilityTypeDto)
		{
			var facilityType = new FacilityType
			{
				Name = facilityTypeDto.Name,
				Description = facilityTypeDto.Description,
				IsDeleted = false
			};

			_context.FacilityType.Add(facilityType);
			await _context.SaveChangesAsync();

			return CreatedAtAction(nameof(GetFacilityType), new { id = facilityType.Id }, facilityType);
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

			_context.FacilityType.Remove(facilityType);
			await _context.SaveChangesAsync();

			return NoContent();
		}

		private bool FacilityTypeExists(int id)
		{
			return _context.FacilityType.Any(e => e.Id == id);
		}
	}

	public class FacilityTypeCreateDto
	{
		public string Name { get; set; }
		public string Description { get; set; }
	}
}