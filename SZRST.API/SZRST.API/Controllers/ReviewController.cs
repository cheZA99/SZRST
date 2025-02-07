using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Entities;
using Infrastructure.Persistance;

namespace SZRST.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewController : ControllerBase
    {
        private readonly SZRSTContext _context;

        public ReviewController(SZRSTContext context)
        {
            _context = context;
        }

        // GET: api/Review
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Review>>> GetReviews()
        {
            return await _context.Review
                                 .Include(r => r.User)        // Include related User
                                 .Include(r => r.Facility)    // Include related Facility
                                 .ToListAsync();
        }

        // GET: api/Review/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Review>> GetReview(int id)
        {
            var review = await _context.Review
                                       .Include(r => r.User)        // Include related User
                                       .Include(r => r.Facility)    // Include related Facility
                                       .FirstOrDefaultAsync(r => r.Id == id);

            if (review == null)
            {
                return NotFound();
            }

            return review;
        }

        // POST: api/Review
        [HttpPost]
        public async Task<ActionResult<Review>> CreateReview([FromBody] ReviewCreateDto reviewDto)
        {
            var user = await _context.Users.FindAsync(reviewDto.UserId);
            if (user == null)
            {
                return BadRequest("Invalid UserId");
            }

            var facility = await _context.Facility.FindAsync(reviewDto.FacilityId);
            if (facility == null)
            {
                return BadRequest("Invalid FacilityId");
            }

            var review = new Review
            {
                Rating = reviewDto.Rating,
                Description = reviewDto.Description,
                User = user,
                Facility = facility,
                IsDeleted = false
            };

            _context.Review.Add(review);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetReview), new { id = review.Id }, review);
        }

        // PUT: api/Review/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateReview(int id, [FromBody] ReviewCreateDto reviewDto)
        {
            var review = await _context.Review.FindAsync(id);
            if (review == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(reviewDto.UserId);
            if (user == null)
            {
                return BadRequest("Invalid UserId");
            }

            var facility = await _context.Facility.FindAsync(reviewDto.FacilityId);
            if (facility == null)
            {
                return BadRequest("Invalid FacilityId");
            }

            review.Rating = reviewDto.Rating;
            review.Description = reviewDto.Description;
            review.User = user;
            review.Facility = facility;

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
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReview(int id)
        {
            var review = await _context.Review.FindAsync(id);
            if (review == null)
            {
                return NotFound();
            }

            _context.Review.Remove(review);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ReviewExists(int id)
        {
            return _context.Review.Any(e => e.Id == id);
        }
    }

    public class ReviewCreateDto
    {
        public int Rating { get; set; }
        public string Description { get; set; }
        public int UserId { get; set; }
        public int FacilityId { get; set; }
    }
}
