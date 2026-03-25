using AutoMapper;
using Domain.Entities;
using Infrastructure.Persistance;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Threading.Tasks;
using SZRST.API.Controllers;
using SZRST.API.Services;
using SZRST.Domain.Entities;
using SZRST.Shared.response;
using SZRST.Tests.Helpers;
using Xunit;

namespace SZRST.Tests.Controllers
{
    public class FacilityControllerTests
    {
        private static async Task SeedRoles(SZRSTContext context)
        {
            var roles = new[]
            {
             new { Name = "SuperAdmin",  Normalized = "SUPERADMIN"  },
             new { Name = "Admin",       Normalized = "ADMIN"       },
             new { Name = "Uposlenik",   Normalized = "UPOSLENIK"   },
             new { Name = "Korisnik",    Normalized = "KORISNIK"    },
          };

            foreach (var r in roles)
            {
                if (!context.Roles.Any(x => x.NormalizedName == r.Normalized))
                {
                    context.Roles.Add(new Role
                    {
                        Name = r.Name,
                        NormalizedName = r.Normalized,
                        ConcurrencyStamp = Guid.NewGuid().ToString()
                    });
                }
            }

            await context.SaveChangesAsync();
        }

        private SZRSTContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<SZRSTContext>()
                .UseInMemoryDatabase(databaseName: "FacilityTestDb")
                .Options;

            var dbName = $"TestDb_{Guid.NewGuid()}";
            var context = TestDbContextFactory.CreateSuperAdmin(dbName);

            context.Facility.Add(new Facility
            {
                Id = 1,
                Name = "Test Facility",
                Location = new Location
                {
                    Id = 1,
                    Address = "Test Address",
                    AddressNumber = "1",
                    City = new City
                    {
                        Id = 1,
                        Name = "Sarajevo",
                        Country = new Country
                        {
                            Id = 1,
                            Name = "Bosnia"
                        }
                    }
                },
                FacilityType = new FacilityType
                {
                    Id = 1,
                    Name = "Padel"
                }
            });

            context.SaveChanges();

            return context;
        }

        [Fact]
        public async Task GetFacility_ReturnsFacility_WhenFacilityExists()
        {
            // Arrange
            var context = GetDbContext();

            var mapperMock = new Mock<IMapper>();
            mapperMock.Setup(m => m.Map<FacilityResponse>(It.IsAny<Facility>()))
                .Returns((Facility f) => new FacilityResponse
                {
                    Id = f.Id,
                    Name = f.Name
                });

            var envMock = new Mock<IWebHostEnvironment>();
            var userManagerMock = MockUserManager();
            var currentUserMock = new Mock<ICurrentUserService>();
            var locationServiceMock = new Mock<ILocationService>();

            var controller = new FacilityController(
                context,
                locationServiceMock.Object,
                mapperMock.Object,
                envMock.Object,
                userManagerMock.Object,
                currentUserMock.Object
            );

            // Act
            var result = await controller.GetFacility(1);

            // Assert
            var okResult = Assert.IsType<FacilityResponse>(result.Value);
            Assert.Equal("Test Facility", okResult.Name);
        }

        [Fact]
        public async Task CreateFacility_ReturnsCreated()
        {
            // Arrange
            var context = GetDbContext();

            var facilityType = new FacilityType { Id = 2, Name = "Tennis" };
            var location = new Location
            {
                Id = 2,
                Address = "Test",
                AddressNumber = "1",
                City = new City { Id = 2, Name = "Mostar", Country = new Country { Id = 2, Name = "BiH" } }
            };

            context.FacilityType.Add(facilityType);
            context.Location.Add(location);
            context.SaveChanges();

            var dto = new FacilityCreateDto
            {
                Name = "New Facility",
                FacilityTypeId = facilityType.Id,
                LocationId = location.Id
            };

            var mapperMock = new Mock<IMapper>();
            var envMock = new Mock<IWebHostEnvironment>();
            var userManagerMock = MockUserManager();
            var currentUserMock = new Mock<ICurrentUserService>();
            var locationServiceMock = new Mock<ILocationService>();

            var controller = new FacilityController(
                context,
                locationServiceMock.Object,
                mapperMock.Object,
                envMock.Object,
                userManagerMock.Object,
                currentUserMock.Object
            );

            // Act
            var result = await controller.CreateFacility(dto);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var facility = Assert.IsType<Facility>(createdResult.Value);

            Assert.Equal("New Facility", facility.Name);
        }

        [Fact]
        public async Task CreateFacility_ReturnsBadRequest_WhenFacilityTypeInvalid()
        {
            var context = GetDbContext();

            var dto = new FacilityCreateDto
            {
                Name = "Test",
                FacilityTypeId = 999,
                LocationId = 1
            };

            var mapperMock = new Mock<IMapper>();
            var envMock = new Mock<IWebHostEnvironment>();
            var userManagerMock = MockUserManager();
            var currentUserMock = new Mock<ICurrentUserService>();
            var locationServiceMock = new Mock<ILocationService>();

            var controller = new FacilityController(
                context,
                locationServiceMock.Object,
                mapperMock.Object,
                envMock.Object,
                userManagerMock.Object,
                currentUserMock.Object
            );

            var result = await controller.CreateFacility(dto);

            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task DeleteFacility_RemovesFacility()
        {
            var context = GetDbContext();

            var mapperMock = new Mock<IMapper>();
            var envMock = new Mock<IWebHostEnvironment>();
            var userManagerMock = MockUserManager();
            var currentUserMock = new Mock<ICurrentUserService>();
            var locationServiceMock = new Mock<ILocationService>();

            var controller = new FacilityController(
                context,
                locationServiceMock.Object,
                mapperMock.Object,
                envMock.Object,
                userManagerMock.Object,
                currentUserMock.Object
            );

            var result = await controller.DeleteFacility(1);

            Assert.IsType<NoContentResult>(result);

            var facility = await context.Facility.FindAsync(1);
            Assert.Null(facility);
        }

        [Fact]
        public async Task GetFacility_ReturnsNotFound_WhenFacilityDoesNotExist()
        {
            var context = GetDbContext();

            var mapperMock = new Mock<IMapper>();
            var envMock = new Mock<IWebHostEnvironment>();
            var userManagerMock = MockUserManager();
            var currentUserMock = new Mock<ICurrentUserService>();
            var locationServiceMock = new Mock<ILocationService>();

            var controller = new FacilityController(
                context,
                locationServiceMock.Object,
                mapperMock.Object,
                envMock.Object,
                userManagerMock.Object,
                currentUserMock.Object
            );

            var result = await controller.GetFacility(999);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        private Mock<UserManager<User>> MockUserManager()
        {
            var store = new Mock<IUserStore<User>>();
            return new Mock<UserManager<User>>(
                store.Object, null, null, null, null, null, null, null, null);
        }
    }
}
