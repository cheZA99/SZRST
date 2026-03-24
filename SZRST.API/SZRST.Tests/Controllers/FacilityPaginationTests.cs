using AutoMapper;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.Persistance;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using SZRST.API.Controllers;
using SZRST.API.Services;
using SZRST.Domain.Entities;
using SZRST.Tests.Helpers;

namespace SZRST.Tests.Controllers
{
	public class FacilityPaginationTests
	{
		private static Mock<UserManager<User>> MockUserManager()
		{
			var store = new Mock<IUserStore<User>>();
			return new Mock<UserManager<User>>(
				store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
		}

		private static Mock<ICurrentUserService> CreateCurrentUserMock()
		{
			var mock = new Mock<ICurrentUserService>();
			mock.SetupGet(x => x.IsSuperAdmin).Returns(true);
			mock.SetupGet(x => x.IsKorisnik).Returns(false);
			mock.SetupGet(x => x.HasValidTenant).Returns(true);
			mock.SetupGet(x => x.TenantId).Returns((int?)null);
			mock.Setup(x => x.HasRole(It.IsAny<string>())).Returns(false);
			return mock;
		}

		[Fact]
		public async Task GetFacilities_ClampsPaginationValues_AndReturnsExpectedSlice()
		{
			var dbName = $"FacilityPagination_{Guid.NewGuid()}";
			await using var context = TestDbContextFactory.Create(dbName);

			var tenant = new Tenant { Id = 1, Name = "Tenant A" };
			context.Set<Tenant>().Add(tenant);

			for (var i = 1; i <= 105; i++)
			{
				context.Facility.Add(new Facility
				{
					Id = i,
					Name = $"Facility {i:D3}",
					TenantId = 1,
					Tenant = tenant,
					IsDeleted = false,
					FacilityType = new FacilityType { Id = 1000 + i, Name = $"Type {i}" },
					Location = new Location
					{
						Id = 2000 + i,
						Address = $"Address {i}",
						AddressNumber = $"{i}",
						City = new City
						{
							Id = 3000 + i,
							Name = $"City {i}",
							Country = new Country { Id = 4000 + i, Name = $"Country {i}" }
						}
					}
				});
			}

			await context.SaveChangesAsync();

			var controller = new FacilityController(
				context,
				new Mock<ILocationService>().Object,
				new Mock<IMapper>().Object,
				new Mock<IWebHostEnvironment>().Object,
				MockUserManager().Object,
				CreateCurrentUserMock().Object);

			var result = await controller.GetFacilities(new FacilityFilterDto
			{
				PageNumber = 0,
				PageSize = 1000
			});

			var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
			var page = ok.Value.Should().BeOfType<PagedResult<FacilityResponse>>().Subject;

			page.PageNumber.Should().Be(1);
			page.PageSize.Should().Be(100);
			page.TotalCount.Should().Be(105);
			page.TotalPages.Should().Be(2);
			page.Items.Should().HaveCount(100);
		}
	}
}
