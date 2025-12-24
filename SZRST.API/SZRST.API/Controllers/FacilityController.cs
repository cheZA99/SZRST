using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Entities;
using Infrastructure.Persistance;
using System.Diagnostics.Metrics;
using SZRST.Shared;
using AutoMapper;
using SZRST.Shared.response;
using Microsoft.AspNetCore.Authorization;

namespace SZRST.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class FacilityController : ControllerBase
    {
        private readonly SZRSTContext _context;
        private readonly LocationController _locationController;
        private readonly IMapper _mapper;

        public FacilityController(SZRSTContext context, LocationController locationController, IMapper mapper)
        {
            _context = context;
            _locationController = locationController;
            _mapper = mapper;
        }

        // GET: api/Facility
        [HttpGet]
        public async Task<ActionResult<IEnumerable<FacilityResponse>>> GetFacilities([FromQuery] string filter, [FromQuery] string value)
        {

            var query = _context.Facility
                                 .Include(f => f.FacilityType)  // Include related FacilityType
                                 .Include(f => f.Location)      // Include related Location
                                 .ThenInclude(f => f.City)
                                 .ThenInclude(f => f.Country);

            if(filter == null || value==null)
            {
                return _mapper.Map<List<FacilityResponse>>(await query.OrderByDescending(x => x.Id).ToListAsync());
            }

                if(filter == "FacilityType")
                {
                return _mapper.Map<List<FacilityResponse>>(await query.Where(q => q.FacilityType.Name.Contains(value)).OrderByDescending(x=> x.Id).ToListAsync());
                }

            if (filter == "Facility")
            {
                return _mapper.Map<List<FacilityResponse>>(await query.Where(q => q.Name.Contains(value)).OrderByDescending(x => x.Id).ToListAsync());
            }

            if (filter == "Address")
            {
                return _mapper.Map<List<FacilityResponse>>(await query.Where(q => q.Location.Address.Contains(value)).OrderByDescending(x => x.Id).ToListAsync());
            }

            if (filter == "City")
            {
                return _mapper.Map<List<FacilityResponse>>(await query.Where(q => q.Location.City.Name.Contains(value)).OrderByDescending(x => x.Id).ToListAsync());
            }

            if (filter == "Country")
            {
                return _mapper.Map<List<FacilityResponse>>(await query.Where(q => q.Location.Country.Name.Contains(value)).OrderByDescending(x => x.Id).ToListAsync());
            }

            return _mapper.Map<List<FacilityResponse>>(await query.OrderByDescending(x => x.Id).ToListAsync()); 
        }

        // GET: api/Facility/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<FacilityResponse>> GetFacility(int id)
        {
            var facility = await _context.Facility
                                         .Include(f => f.FacilityType)  // Include related FacilityType
                                         .Include(f => f.Location)      // Include related Location
                                         .ThenInclude(f=> f.City)
                                         .ThenInclude(f=> f.Country)
                                         .FirstOrDefaultAsync(f => f.Id == id);

            if (facility == null)
            {
                return NotFound();
            }

            return _mapper.Map<FacilityResponse>(facility);
        }

        // POST: api/Facility
        [HttpPost]
        public async Task<ActionResult<Facility>> CreateFacility([FromBody] FacilityCreateDto facilityDto)
        {
            var facilityType = await _context.FacilityType.FindAsync(facilityDto.FacilityTypeId);
            if (facilityType == null)
            {
                return BadRequest("Invalid FacilityTypeId");
            }

            var location = await _context.Location.FindAsync(facilityDto.LocationId);
            if (location == null)
            {
                return BadRequest("Invalid LocationId");
            }

            var facility = new Facility
            {
                Name = facilityDto.Name,
                FacilityType = facilityType,
                Location = location,
                IsDeleted = false,
                ImageUrl = facilityDto.ImageUrl
            };

            _context.Facility.Add(facility);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetFacility), new { id = facility.Id }, facility);
        }

        [HttpPost("AddFacility")]
        public async Task<ActionResult<Facility>> CreateFacilityAndLocation([FromBody] FacilityLocationCreateDto facilityDto)
        {
            var country = await _context.Country.FindAsync(facilityDto.CountryId);
            var city = await _context.City.FindAsync(facilityDto.CityId);
            var facilityType = await _context.FacilityType.FindAsync(facilityDto.FacilityTypeId);

            if (country == null || city == null || facilityType == null)
            {
                return BadRequest("Invalid CountryId, CityId, or FacilityTypeId");
            }

            // Create Location
            var location = new Location
            {
                Address = facilityDto.Address,
                AddressNumber = facilityDto.AddressNumber,
                Country = country,
                City = city
            };

            _context.Location.Add(location);
            await _context.SaveChangesAsync();

            // Create Facility with new Location
            var facility = new Facility
            {
                Name = facilityDto.Name,
                FacilityType = facilityType,
                Location = location,
                ImageUrl = facilityDto.ImageUrl
            };

            _context.Facility.Add(facility);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetFacility", new { id = facility.Id }, facility);

        }

        // PUT: api/Facility/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateFacility(int id, [FromBody] FacilityCreateDto facilityDto)
        {
            var facility = await _context.Facility.FindAsync(id);
            if (facility == null)
            {
                return NotFound();
            }

            var facilityType = await _context.FacilityType.FindAsync(facilityDto.FacilityTypeId);
            if (facilityType == null)
            {
                return BadRequest("Invalid FacilityTypeId");
            }

            var location = await _context.Location.FindAsync(facilityDto.LocationId);
            if (location == null)
            {
                return BadRequest("Invalid LocationId");
            }

            facility.Name = facilityDto.Name;
            facility.FacilityType = facilityType;
            facility.Location = location;

            _context.Entry(facility).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FacilityExists(id))
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

        // DELETE: api/Facility/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFacility(int id)
        {
            var facility = await _context.Facility.FindAsync(id);
            if (facility == null)
            {
                return NotFound();
            }

            _context.Facility.Remove(facility);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool FacilityExists(int id)
        {
            return _context.Facility.Any(e => e.Id == id);
        }
    }

    public class FacilityCreateDto
    {
        public string Name { get; set; }
        public int FacilityTypeId { get; set; }
        public int LocationId { get; set; }
        public string ImageUrl { get; set; }
    }

    public class FacilityLocationCreateDto
    {
        public string Name { get; set; }
        public int FacilityTypeId { get; set; }
        public string Address { get; set; }
        public string AddressNumber { get; set; }
        public int CountryId { get; set; }
        public int CityId { get; set; }
        public string ImageUrl { get; set; }
    }

}
