using AutoMapper;
using Domain.Entities;
using Infrastructure.Persistance;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SZRST.API.Security;
using SZRST.API.Services;
using SZRST.Domain.Constants;
using SZRST.Shared.response;

namespace SZRST.API.Controllers
{
	[Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Uposlenik},{Roles.Korisnik}")]
	[Route("api/[controller]")]
	[ApiController]
	public class FacilityController :ControllerBase
	{
		private const long MaxUploadSizeBytes = 5 * 1024 * 1024;
		private readonly SZRSTContext _context;
		private readonly ILocationService _locationService;
		private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _env;
        private readonly UserManager<User> _userManager;
        private readonly ICurrentUserService _currentUserService;

        public FacilityController(SZRSTContext context, 
			ILocationService locationService, 
			IMapper mapper, IWebHostEnvironment env,
            UserManager<User> userManager,
            ICurrentUserService currentUserService)
		{
			_context = context;
			_locationService = locationService;
			_mapper = mapper;
			_env = env;
			_currentUserService = currentUserService;
			_userManager = userManager;
		}

		// GET: api/Facility
		[HttpGet]
        /*public async Task<ActionResult<IEnumerable<FacilityResponse>>> GetFacilities([FromQuery] string filter, [FromQuery] string value)
		{
			var query = _context.Facility
							 .Include(f => f.FacilityType)  // Include related FacilityType
							 .Include(f => f.Location)      // Include related Location
							 .ThenInclude(f => f.City)
							 .ThenInclude(f => f.Country);

			if (filter == null || value == null)
			{
				return _mapper.Map<List<FacilityResponse>>(await query.OrderByDescending(x => x.Id).ToListAsync());
			}

			if (filter == "FacilityType")
			{
				return _mapper.Map<List<FacilityResponse>>(await query.Where(q => q.FacilityType.Name.Contains(value)).OrderByDescending(x => x.Id).ToListAsync());
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
		}*/

        // GET: api/Facility
		[HttpGet]
		public async Task<ActionResult<PagedResult<FacilityResponse>>> GetFacilities([FromQuery] FacilityFilterDto filter)
		{
			if (!_currentUserService.IsSuperAdmin &&
			    !_currentUserService.IsKorisnik &&
			    !_currentUserService.TenantId.HasValue)
			{
				return Forbid();
			}

			IQueryable<Facility> query = _context.Facility
				.IgnoreQueryFilters()
				.Include(f => f.FacilityType)
				.Include(f => f.Tenant)
				.Include(f => f.Location)
					.ThenInclude(f => f.City)
					.ThenInclude(c => c.Country);

			if (_currentUserService.IsSuperAdmin || _currentUserService.IsKorisnik)
			{
				if (filter.TenantId.HasValue)
					query = query.Where(u => u.TenantId == filter.TenantId.Value);
			}
			else
			{
				if (filter.TenantId.HasValue && filter.TenantId.Value != _currentUserService.TenantId.Value)
					return Forbid();

				query = query.Where(u => u.TenantId == _currentUserService.TenantId.Value);
			}

            if (!string.IsNullOrWhiteSpace(filter.Address))
            {
                var search = filter.Address.ToLower();

                query = query.Where(u =>
                    (u.Location.Address + " " + u.Location.AddressNumber)
                    .ToLower()
                    .Contains(search));
            }
            query = query.Where(u => !u.IsDeleted);
            if (!string.IsNullOrWhiteSpace(filter.Name))
                query = query.Where(u => u.Name.ToLower().Contains(filter.Name.ToLower()));

            if (filter.FacilityTypeId.HasValue)
                query = query.Where(u => u.FacilityType.Id == filter.FacilityTypeId.Value);

            if (filter.CountryId.HasValue)
                query = query.Where(u => u.Location.Country.Id == filter.CountryId.Value);

            if (filter.CityId.HasValue)
                query = query.Where(u => u.Location.City.Id == filter.CityId.Value);

            query = filter.SortColumn switch
            {
                "name" => filter.SortDirection == "asc"
                    ? query.OrderBy(f => f.Name)
                    : query.OrderByDescending(f => f.Name),

                "facilityType" => filter.SortDirection == "asc"
                    ? query.OrderBy(f => f.FacilityType.Name)
                    : query.OrderByDescending(f => f.FacilityType.Name),

                "address" => filter.SortDirection == "asc"
                    ? query.OrderBy(f => f.Location.Address)
                    : query.OrderByDescending(f => f.Location.Address),

                _ => query.OrderBy(f => f.Name)
            };

            int pageNumber = filter.PageNumber < 1 ? 1 : filter.PageNumber;
            int pageSize = filter.PageSize < 1 ? 10 : (filter.PageSize > 100 ? 100 : filter.PageSize);

            var totalCount = await query.CountAsync();

            var pagedUsers = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

			var facilityDtos = new List<FacilityResponse>();

			foreach (var facility in pagedUsers)
			{
                facilityDtos.Add(MapFacility(facility));
			}

			return Ok(new PagedResult<FacilityResponse>
			{
				Items = facilityDtos,
				TotalCount = totalCount,
				PageNumber = pageNumber,
				PageSize = pageSize,
				TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
			});
		}

		// GET: api/Facility/{id}
		[HttpGet("{id}")]
		public async Task<ActionResult<FacilityResponse>> GetFacility(int id)
		{
			var facility = await _context.Facility
								    .IgnoreQueryFilters()
								    .Include(f => f.FacilityType)  // Include related FacilityType
								    .Include(f => f.Tenant)
								    .Include(f => f.Location)      // Include related Location
								    .ThenInclude(f => f.City)
								    .ThenInclude(f => f.Country)
								    .FirstOrDefaultAsync(f => f.Id == id && !f.IsDeleted);

			if (facility == null)
			{
				return NotFound();
			}

			if (!_currentUserService.IsSuperAdmin &&
			    !_currentUserService.IsKorisnik &&
			    !_currentUserService.CanAccessTenant(facility.TenantId))
			{
				return Forbid();
			}

			return Ok(MapFacility(facility));
		}

		// POST: api/Facility
		[Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin}")]
		[HttpPost]
		public async Task<ActionResult<Facility>> CreateFacility([FromBody] FacilityCreateDto facilityDto)
		{
			if (!_currentUserService.IsSuperAdmin && !_currentUserService.HasValidTenant)
				return Forbid();

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
				ImageUrl = facilityDto.ImageUrl,
				TenantId = _currentUserService.IsSuperAdmin ? facilityDto.TenantId.GetValueOrDefault() : _currentUserService.TenantId!.Value
			};

			if (facility.TenantId <= 0)
				return BadRequest("TenantId mora biti validan.");

			_context.Facility.Add(facility);
			await _context.SaveChangesAsync();

			return CreatedAtAction(nameof(GetFacility), new { id = facility.Id }, facility);
		}

		[Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin}")]
		[RequestSizeLimit(MaxUploadSizeBytes)]
		[HttpPost("AddFacility")]
		public async Task<ActionResult<Facility>> CreateFacilityAndLocation([FromForm] FacilityLocationCreateWithImageDto facilityDto)
		{
			if (!_currentUserService.IsSuperAdmin && !_currentUserService.HasValidTenant)
				return Forbid();

			var country = await _context.Country.FindAsync(facilityDto.CountryId);
			var city = await _context.City.FindAsync(facilityDto.CityId);
			var facilityType = await _context.FacilityType.FindAsync(facilityDto.FacilityTypeId);

			if (country == null || city == null || facilityType == null)
			{
				return BadRequest("Invalid CountryId, CityId, or FacilityTypeId");
			}

			var locationResult = await _locationService.CreateLocationAsync(new SZRST.Shared.LocationCreateDto
			{
				Address = facilityDto.Address,
				AddressNumber = facilityDto.AddressNumber,
				CountryId = facilityDto.CountryId,
				CityId = facilityDto.CityId
			});
			if (!locationResult.IsSuccess)
			{
				return BadRequest(locationResult.ErrorMessage);
			}
			var imageUrl = "";

			try
			{
				if (facilityDto.File != null)
				{
					if (facilityDto.File.Length > MaxUploadSizeBytes)
					{
						return BadRequest("Maksimalna veličina fajla je 5MB.");
					}

					if (!FileSignatureValidator.IsValidImage(facilityDto.File))
					{
						return BadRequest("Dozvoljene su samo validne JPG, PNG ili WEBP slike.");
					}

					var folder = Path.Combine(_env.WebRootPath, "images");

					if (!Directory.Exists(folder))
						Directory.CreateDirectory(folder);

					var fileName = Guid.NewGuid() + Path.GetExtension(facilityDto.File.FileName);

					var filePath = Path.Combine(folder, fileName);

					using (var stream = new FileStream(filePath, FileMode.Create))
					{
						await facilityDto.File.CopyToAsync(stream);
					}

					imageUrl = "/images/" + fileName;
				}
			}
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }

            // Create Facility with new Location
			var facility = new Facility
			{
				Name = facilityDto.Name,
				FacilityType = facilityType,
				Location = locationResult.Location,
				ImageUrl = imageUrl,
				TenantId = _currentUserService.IsSuperAdmin ? facilityDto.TenantId : _currentUserService.TenantId!.Value
			};

			if (facility.TenantId <= 0)
				return BadRequest("TenantId mora biti validan.");

			_context.Facility.Add(facility);
			await _context.SaveChangesAsync();

			return CreatedAtAction("GetFacility", new { id = facility.Id }, facility);
		}

		// PUT: api/Facility/{id}
		[Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin}")]
		[RequestSizeLimit(MaxUploadSizeBytes)]
		[HttpPut("{id}")]
		public async Task<IActionResult> UpdateFacility(int id, [FromForm] FacilityLocationCreateWithImageDto facilityDto)
		{
			var facility = await _context.Facility.IgnoreQueryFilters().FirstOrDefaultAsync(f => f.Id == id);
			if (facility == null)
			{
				return NotFound();
			}

			if (!_currentUserService.CanAccessTenant(facility.TenantId))
				return Forbid();

			var facilityType = await _context.FacilityType.FindAsync(facilityDto.FacilityTypeId);
			if (facilityType == null)
			{
				return BadRequest("Invalid FacilityTypeId");
			}

			var location = await _context.Location.Include(x => x.Country)
				.Include(x => x.City).FirstOrDefaultAsync(x => x.Id == facilityDto.LocationId);
			if (location == null)
			{
				return BadRequest("Invalid LocationId");
			}

			if (location.City.Id != facilityDto.CityId ||
				location.Country.Id != facilityDto.CountryId ||
				location.Address != facilityDto.Address ||
				location.AddressNumber != facilityDto.AddressNumber)
			{
				var locationResult = await _locationService.CreateLocationAsync(
					new SZRST.Shared.LocationCreateDto
					{
						Address = facilityDto.Address,
						AddressNumber = facilityDto.AddressNumber,
						CountryId = facilityDto.CountryId,
						CityId = facilityDto.CityId
					},
					reuseExisting: true);

				if (!locationResult.IsSuccess)
				{
					return BadRequest(locationResult.ErrorMessage);
				}

				facility.Location = locationResult.Location;
            }
			try
			{
				if (facilityDto.File != null)
				{
					if (facilityDto.File.Length > MaxUploadSizeBytes)
					{
						return BadRequest("Maksimalna veličina fajla je 5MB.");
					}

					if (!FileSignatureValidator.IsValidImage(facilityDto.File))
					{
						return BadRequest("Dozvoljene su samo validne JPG, PNG ili WEBP slike.");
					}

					var folder = Path.Combine(_env.WebRootPath, "images");

					if (!Directory.Exists(folder))
						Directory.CreateDirectory(folder);

					var fileName = Guid.NewGuid() + Path.GetExtension(facilityDto.File.FileName);

					var filePath = Path.Combine(folder, fileName);

					using (var stream = new FileStream(filePath, FileMode.Create))
					{
						await facilityDto.File.CopyToAsync(stream);
					}

					facility.ImageUrl = "/images/" + fileName;
				}
				else if (facilityDto.RemoveImage)
				{
					facility.ImageUrl = null;
				}
			}
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }

            facility.Name = facilityDto.Name;
			facility.FacilityType = facilityType;
			facility.TenantId = _currentUserService.IsSuperAdmin ? facilityDto.TenantId : _currentUserService.TenantId!.Value;

			if (facility.TenantId <= 0)
				return BadRequest("TenantId mora biti validan.");

			_context.Entry(facility).State = EntityState.Modified;

			var oldImageUrl = facility.ImageUrl;

			try
			{
				await _context.SaveChangesAsync();

				if (!string.Equals(oldImageUrl, facility.ImageUrl, StringComparison.OrdinalIgnoreCase))
				{
					DeleteLocalFile(oldImageUrl);
				}
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
		[Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin}")]
		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteFacility(int id)
		{
			var facility = await _context.Facility.IgnoreQueryFilters().FirstOrDefaultAsync(f => f.Id == id);
			if (facility == null)
			{
				return NotFound();
			}

			if (!_currentUserService.CanAccessTenant(facility.TenantId))
				return Forbid();

			facility.IsDeleted = true;
			try
			{
				await _context.SaveChangesAsync();
			}
			catch (DbUpdateException)
			{
				return BadRequest("Nije moguće obrisati objekat jer se koristi u aktivnim terminima ili izvještajima.");
			}

			return NoContent();
		}

		private bool FacilityExists(int id)
		{
			return _context.Facility.Any(e => e.Id == id);
		}

		private static FacilityResponse MapFacility(Facility facility)
		{
			return new FacilityResponse
			{
				Id = facility.Id,
				Name = facility.Name,
				ImageUrl = facility.ImageUrl,
				TenantId = facility.TenantId,
				TenantName = facility.Tenant?.Name,
				FacilityType = facility.FacilityType == null ? null : new FacilityTypeSummary
				{
					Id = facility.FacilityType.Id,
					Name = facility.FacilityType.Name,
					Description = facility.FacilityType.Description
				},
				Location = facility.Location == null ? null : new LocationSummary
				{
					Id = facility.Location.Id,
					Address = facility.Location.Address,
					AddressNumber = facility.Location.AddressNumber,
					Country = facility.Location.Country == null ? null : new CountrySummary
					{
						Id = facility.Location.Country.Id,
						Name = facility.Location.Country.Name,
						ShortName = facility.Location.Country.ShortName
					},
					City = facility.Location.City == null ? null : new CitySummary
					{
						Id = facility.Location.City.Id,
						Name = facility.Location.City.Name,
						Country = facility.Location.City.Country == null ? null : new CountrySummary
						{
							Id = facility.Location.City.Country.Id,
							Name = facility.Location.City.Country.Name,
							ShortName = facility.Location.City.Country.ShortName
						}
					}
				}
			};
		}

		private void DeleteLocalFile(string imageUrl)
		{
			if (string.IsNullOrWhiteSpace(imageUrl))
				return;

			var relativePath = imageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
			var fullPath = Path.Combine(_env.WebRootPath, relativePath);

			if (System.IO.File.Exists(fullPath))
			{
				System.IO.File.Delete(fullPath);
			}
		}
	}

	public class FacilityCreateDto
	{
		public string Name { get; set; }
		public int FacilityTypeId { get; set; }
		public int LocationId { get; set; }
		public string ImageUrl { get; set; }
		public int? TenantId { get; set; }
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
		public int TenantId { get; set; }
		public int LocationId { get; set; }
    }

    public class FacilityLocationCreateWithImageDto
    {
        public string Name { get; set; }
        public int FacilityTypeId { get; set; }
        public string Address { get; set; }
        public string AddressNumber { get; set; }
        public int CountryId { get; set; }
        public int CityId { get; set; }
        public int LocationId { get; set; }
        public int TenantId { get; set; }
        public IFormFile File { get; set; }
		public bool RemoveImage { get; set; }
    }

    public class FacilityFilterDto
    {
        public string? Name { get; set; }
        public int? FacilityTypeId { get; set; }
        public string? Address { get; set; }
        public int? CountryId { get; set; }
        public int? CityId { get; set; }
        public int? TenantId { get; set; }
        public bool? IsDeleted { get; set; }

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortColumn { get; set; }
        public string? SortDirection { get; set; }
    }
}
