using Domain.Entities;
using Infrastructure.Persistance;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SZRST.API.Security;
using SZRST.Domain.Constants;

namespace SZRST.API.Controllers
{
	[Authorize]
	[Route("api/[controller]")]
	[ApiController]
	public class ReviewController :ControllerBase
	{
		private readonly SZRSTContext _context;
		private readonly ICurrentUserService _currentUserService;

		public ReviewController(SZRSTContext context, ICurrentUserService currentUserService)
		{
			_context = context;
			_currentUserService = currentUserService;
		}

		// GET: api/Review
		[Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Uposlenik}")]
		[HttpGet]
		public async Task<ActionResult<IEnumerable<ReviewDto>>> GetReviews()
		{
			var reviews = await _context.Review
							 .Include(r => r.User)
							 .Include(r => r.Facility)
							 .Where(r => !r.IsDeleted)
							 .Select(r => new ReviewDto
							 {
								 Id = r.Id,
								 Rating = r.Rating,
								 Description = r.Description,
								 UserId = r.User.Id,
								 UserName = r.User.UserName,
								 FacilityId = r.Facility.Id,
								 FacilityName = r.Facility.Name,
								 TenantId = r.TenantId
							 })
							 .ToListAsync();

			return Ok(reviews);
		}

		// GET: api/Review/{id}
		[Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Uposlenik}")]
		[HttpGet("{id}")]
		public async Task<ActionResult<ReviewDto>> GetReview(int id)
		{
			var review = await _context.Review
								  .Include(r => r.User)
								  .Include(r => r.Facility)
								  .FirstOrDefaultAsync(r => r.Id == id);

			if (review == null)
			{
				return NotFound();
			}

			return Ok(MapReview(review));
		}

		// POST: api/Review
		[Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Uposlenik},{Roles.Korisnik}")]
		[HttpPost]
		public async Task<ActionResult<ReviewDto>> CreateReview([FromBody] ReviewCreateDto reviewDto)
		{
			if (!_currentUserService.HasValidTenant)
				return Forbid();

			var userId = _currentUserService.IsSuperAdmin ? reviewDto.UserId : _currentUserService.UserId;
			var user = await _context.Users.FindAsync(userId);
			if (user == null)
			{
				return BadRequest("Invalid UserId");
			}

			var facility = await _context.Facility.FindAsync(reviewDto.FacilityId);
			if (facility == null)
			{
				return BadRequest("Invalid FacilityId");
			}

			if (!_currentUserService.CanAccessTenant(user.TenantId) ||
			    !_currentUserService.CanAccessTenant(facility.TenantId) ||
			    user.TenantId != facility.TenantId)
			{
				return Forbid();
			}

			var review = new Review
			{
				Rating = reviewDto.Rating,
				Description = reviewDto.Description,
				User = user,
				Facility = facility,
				IsDeleted = false,
				TenantId = facility.TenantId
			};

			_context.Review.Add(review);
			await _context.SaveChangesAsync();

			return CreatedAtAction(nameof(GetReview), new { id = review.Id }, MapReview(review));
		}

		// PUT: api/Review/{id}
		[Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Uposlenik},{Roles.Korisnik}")]
		[HttpPut("{id}")]
		public async Task<IActionResult> UpdateReview(int id, [FromBody] ReviewCreateDto reviewDto)
		{
			var review = await _context.Review.FindAsync(id);
			if (review == null)
			{
				return NotFound();
			}

			if (!_currentUserService.CanAccessTenant(review.TenantId))
				return Forbid();

			var userId = _currentUserService.IsSuperAdmin ? reviewDto.UserId : _currentUserService.UserId;
			var user = await _context.Users.FindAsync(userId);
			if (user == null)
			{
				return BadRequest("Invalid UserId");
			}

			var facility = await _context.Facility.FindAsync(reviewDto.FacilityId);
			if (facility == null)
			{
				return BadRequest("Invalid FacilityId");
			}

			if (!_currentUserService.CanAccessTenant(user.TenantId) ||
			    !_currentUserService.CanAccessTenant(facility.TenantId) ||
			    user.TenantId != facility.TenantId)
			{
				return Forbid();
			}

			review.Rating = reviewDto.Rating;
			review.Description = reviewDto.Description;
			review.User = user;
			review.Facility = facility;
			review.TenantId = facility.TenantId;

			_context.Entry(review).State = EntityState.Modified;

			try
			{
				await _context.SaveChangesAsync();
			}
			catch (DbUpdateConcurrencyException)
			{
				if (!ReviewExists(id))
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

		// DELETE: api/Review/{id}
		[Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Uposlenik},{Roles.Korisnik}")]
		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteReview(int id)
		{
			var review = await _context.Review.FindAsync(id);
			if (review == null)
			{
				return NotFound();
			}

			if (!_currentUserService.CanAccessTenant(review.TenantId))
				return Forbid();

			review.IsDeleted = true;
			await _context.SaveChangesAsync();

			return NoContent();
		}

		private bool ReviewExists(int id)
		{
			return _context.Review.Any(e => e.Id == id);
		}

		private static ReviewDto MapReview(Review review)
		{
			return new ReviewDto
			{
				Id = review.Id,
				Rating = review.Rating,
				Description = review.Description,
				UserId = review.User?.Id ?? 0,
				UserName = review.User?.UserName,
				FacilityId = review.Facility?.Id ?? 0,
				FacilityName = review.Facility?.Name,
				TenantId = review.TenantId
			};
		}
	}

	public class ReviewDto
	{
		public int Id { get; set; }
		public int Rating { get; set; }
		public string Description { get; set; }
		public int UserId { get; set; }
		public string UserName { get; set; }
		public int FacilityId { get; set; }
		public string FacilityName { get; set; }
		public int TenantId { get; set; }
	}

	public class ReviewCreateDto
	{
		public int Rating { get; set; }
		public string Description { get; set; }
		public int UserId { get; set; }
		public int FacilityId { get; set; }
	}
}
