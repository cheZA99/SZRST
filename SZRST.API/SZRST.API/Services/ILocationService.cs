using Domain.Entities;
using Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using SZRST.Shared;

namespace SZRST.API.Services
{
    public interface ILocationService
    {
        Task<LocationOperationResult> CreateLocationAsync(LocationCreateDto locationDto, bool reuseExisting = false);
    }

    public class LocationService : ILocationService
    {
        private readonly SZRSTContext _context;

        public LocationService(SZRSTContext context)
        {
            _context = context;
        }

        public async Task<LocationOperationResult> CreateLocationAsync(LocationCreateDto locationDto, bool reuseExisting = false)
        {
            var country = await _context.Country.FindAsync(locationDto.CountryId);
            if (country == null)
            {
                return LocationOperationResult.Failure("Invalid CountryId");
            }

            var city = await _context.City.FindAsync(locationDto.CityId);
            if (city == null)
            {
                return LocationOperationResult.Failure("Invalid CityId");
            }

            if (reuseExisting)
            {
                var existingLocation = await _context.Location
                    .Include(x => x.Country)
                    .Include(x => x.City)
                    .FirstOrDefaultAsync(x =>
                        x.Country.Id == locationDto.CountryId &&
                        x.City.Id == locationDto.CityId &&
                        x.Address == locationDto.Address &&
                        x.AddressNumber == locationDto.AddressNumber);

                if (existingLocation != null)
                {
                    return LocationOperationResult.Success(existingLocation);
                }
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

            return LocationOperationResult.Success(location);
        }
    }

    public class LocationOperationResult
    {
        public bool IsSuccess { get; private set; }
        public string ErrorMessage { get; private set; }
        public Location Location { get; private set; }

        public static LocationOperationResult Success(Location location)
        {
            return new LocationOperationResult
            {
                IsSuccess = true,
                Location = location
            };
        }

        public static LocationOperationResult Failure(string errorMessage)
        {
            return new LocationOperationResult
            {
                IsSuccess = false,
                ErrorMessage = errorMessage
            };
        }
    }
}
