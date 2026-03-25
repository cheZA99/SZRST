using Domain.Entities;
using FluentAssertions;
using FluentValidation;
using Infrastructure.Persistance;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SZRST.API.Controllers;
using SZRST.API.Security;
using SZRST.Domain.Entities;
using SZRST.Tests.Helpers;

namespace SZRST.Tests.Controllers
{
	public class TenantControllerTests
	{
		private static async Task SeedRoles(SZRSTContext context)
		{
			var roles = new[]
			{
			 new { Name = "SuperAdmin", Normalized = "SUPERADMIN" },
			 new { Name = "Admin",      Normalized = "ADMIN"      },
			 new { Name = "Uposlenik",  Normalized = "UPOSLENIK"  },
			 new { Name = "Korisnik",   Normalized = "KORISNIK"   },
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

		private static TenantController CreateController(SZRSTContext context,
		    Microsoft.AspNetCore.Identity.UserManager<User> userManager)
		{
			IValidator<CreateTenantWithAdminDto> createValidator = new CreateTenantWithAdminDtoValidator();
			IValidator<UpdateTenantDto> updateValidator = new UpdateTenantDtoValidator();
			var currentUserMock = new Mock<ICurrentUserService>();
			return new TenantController(context, userManager, currentUserMock.Object, createValidator, updateValidator);
		}


		[Fact]
		public async Task CreateTenantWithAdmin_ValidData_ReturnsOkWithTenant()
		{
			var dbName = $"TestDb_{Guid.NewGuid()}";
			var context = TestDbContextFactory.CreateSuperAdmin(dbName);
			var userManager = TestUserManagerFactory.Create(context);
			await SeedRoles(context);

			var controller = CreateController(context, userManager);

			var dto = new CreateTenantWithAdminDto
			{
				TenantName = "Test Organizacija",
				AdminEmail = "admin@test.com",
				AdminUsername = "testadmin",
				AdminPassword = "Password123!",
				AdminConfirmPassword = "Password123!"
			};

			var actionResult = await controller.CreateTenantWithAdmin(dto);

			var okResult = actionResult.Result.Should().BeOfType<OkObjectResult>().Subject;
			var response = okResult.Value.Should().BeOfType<TenantCreationResponse>().Subject;

			response.IsSuccess.Should().BeTrue();
			response.Message.Should().Contain("uspješno");
			response.Tenant.Should().NotBeNull();
			response.Tenant!.Name.Should().Be("Test Organizacija");
			response.Tenant.UserCount.Should().Be(1);

			var savedTenant = context.Set<Tenant>().FirstOrDefault(t => t.Name == "Test Organizacija");
			savedTenant.Should().NotBeNull();

			var savedUser = await userManager.FindByEmailAsync("admin@test.com");
			savedUser.Should().NotBeNull();
			savedUser!.TenantId.Should().Be(savedTenant!.Id);
		}

		[Fact]
		public async Task CreateTenantWithAdmin_DuplicateEmail_ReturnsBadRequest()
		{
			var dbName = $"TestDb_{Guid.NewGuid()}";
			var context = TestDbContextFactory.CreateSuperAdmin(dbName);
			var userManager = TestUserManagerFactory.Create(context);
			await SeedRoles(context);

			var existingUser = new User
			{
				UserName = "existing",
				Email = "duplicate@test.com",
				TenantId = null,
				DateCreated = DateTime.UtcNow,
				DateModified = DateTime.UtcNow
			};
			await userManager.CreateAsync(existingUser, "Password123!");

			var controller = CreateController(context, userManager);

			var dto = new CreateTenantWithAdminDto
			{
				TenantName = "Nova Organizacija",
				AdminEmail = "duplicate@test.com",
				AdminUsername = "noviAdmin",
				AdminPassword = "Password123!",
				AdminConfirmPassword = "Password123!"
			};

			var actionResult = await controller.CreateTenantWithAdmin(dto);

			var badRequest = actionResult.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
			var response = badRequest.Value.Should().BeOfType<TenantCreationResponse>().Subject;

			response.IsSuccess.Should().BeFalse();
			response.Message.Should().Contain("emailom");

			var tenant = context.Set<Tenant>().FirstOrDefault(t => t.Name == "Nova Organizacija");
			tenant.Should().BeNull();
		}
	}
}
