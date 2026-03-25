using Domain.Entities;
using FluentAssertions;
using Infrastructure.Persistance;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using SZRST.API.Controllers;
using SZRST.Domain.Constants;
using SZRST.Domain.Entities;
using SZRST.Tests.Helpers;

namespace SZRST.Tests.Controllers
{
	public class AppointmentControllerTests
	{
		private static async Task SeedRoles(SZRSTContext context)
		{
			var roles = new[]
			{
				new { Name = Roles.SuperAdmin, Normalized = "SUPERADMIN" },
				new { Name = Roles.Admin, Normalized = "ADMIN" },
				new { Name = Roles.Uposlenik, Normalized = "UPOSLENIK" },
				new { Name = Roles.Korisnik, Normalized = "KORISNIK" },
			};

			foreach (var role in roles)
			{
				if (!context.Roles.Any(x => x.NormalizedName == role.Normalized))
				{
					context.Roles.Add(new Role
					{
						Name = role.Name,
						NormalizedName = role.Normalized,
						ConcurrencyStamp = Guid.NewGuid().ToString()
					});
				}
			}

			await context.SaveChangesAsync();
		}

		private static Mock<ICurrentUserService> CreateCurrentUserMock(
			int userId,
			int? tenantId,
			bool isSuperAdmin = false,
			bool isKorisnik = false)
		{
			var mock = new Mock<ICurrentUserService>();
			mock.SetupGet(x => x.UserId).Returns(userId);
			mock.SetupGet(x => x.TenantId).Returns(tenantId);
			mock.SetupGet(x => x.IsSuperAdmin).Returns(isSuperAdmin);
			mock.SetupGet(x => x.IsKorisnik).Returns(isKorisnik);
			mock.SetupGet(x => x.HasValidTenant).Returns(isSuperAdmin || tenantId.HasValue);
			mock.SetupGet(x => x.IsAuthenticated).Returns(true);
			mock.SetupGet(x => x.Username).Returns("test-user");
			#pragma warning disable CS0618
			mock.SetupGet(x => x.Role).Returns(isSuperAdmin ? Roles.SuperAdmin : isKorisnik ? Roles.Korisnik : Roles.Admin);
			#pragma warning restore CS0618
			mock.Setup(x => x.HasRole(It.IsAny<string>())).Returns<string>(role =>
				(isSuperAdmin && role == Roles.SuperAdmin) ||
				(isKorisnik && role == Roles.Korisnik) ||
				(!isSuperAdmin && !isKorisnik && role == Roles.Admin));
			return mock;
		}

		private static AppointmentController CreateController(
			SZRSTContext context,
			UserManager<User> userManager,
			ICurrentUserService currentUserService)
		{
			return new AppointmentController(
				context,
				currentUserService,
				userManager,
				new AppointmentCreateDtoValidator());
		}

		[Fact]
		public async Task Context_WithTenantScope_ReturnsOnlyCurrentTenantEntities()
		{
			var dbName = $"TenantScope_{Guid.NewGuid()}";

			await using (var seedContext = TestDbContextFactory.CreateSuperAdmin(dbName))
			{
				seedContext.Set<Tenant>().AddRange(
					new Tenant { Id = 1, Name = "Tenant A" },
					new Tenant { Id = 2, Name = "Tenant B" });

				seedContext.Facility.AddRange(
					new Facility
					{
						Id = 1,
						Name = "Facility A",
						TenantId = 1,
						FacilityType = new FacilityType { Id = 1, Name = "Padel" },
						Location = new Location
						{
							Id = 1,
							Address = "A",
							AddressNumber = "1",
							City = new City
							{
								Id = 1,
								Name = "Sarajevo",
								Country = new Country { Id = 1, Name = "BiH" }
							}
						}
					},
					new Facility
					{
						Id = 2,
						Name = "Facility B",
						TenantId = 2,
						FacilityType = new FacilityType { Id = 2, Name = "Tennis" },
						Location = new Location
						{
							Id = 2,
							Address = "B",
							AddressNumber = "2",
							City = new City
							{
								Id = 2,
								Name = "Mostar",
								Country = new Country { Id = 2, Name = "BiH" }
							}
						}
					});

				await seedContext.SaveChangesAsync();
			}

			await using var tenantAContext = TestDbContextFactory.Create(dbName, tenantId: 1, isSuperAdminOrUser: false);

			var facilities = await tenantAContext.Facility.ToListAsync();

			facilities.Should().HaveCount(1);
			facilities[0].TenantId.Should().Be(1);
			facilities[0].Name.Should().Be("Facility A");
		}

		[Fact]
		public async Task GetAppointment_ReturnsForbid_WhenAdminFromTenantBRequestsTenantAAppointment()
		{
			var dbName = $"AppointmentTenant_{Guid.NewGuid()}";
			await using var context = TestDbContextFactory.CreateSuperAdmin(dbName);
			var userManager = TestUserManagerFactory.Create(context);
			await SeedRoles(context);

			var tenantA = new Tenant { Id = 1, Name = "Tenant A" };
			var tenantB = new Tenant { Id = 2, Name = "Tenant B" };

			var patientA = new User
			{
				Id = 100,
				UserName = "patient-a",
				Email = "patient-a@test.com",
				Active = true,
				IsDeleted = false,
				TenantId = 1,
				DateCreated = DateTime.UtcNow
			};

			await userManager.CreateAsync(patientA, "Password123!");
			await userManager.AddToRoleAsync(patientA, Roles.Korisnik);

			var appointment = new Appointment
			{
				Id = 500,
				AppointmentDateTime = DateTime.UtcNow.AddDays(1),
				IsClosed = false,
				IsFree = false,
				IsDeleted = false,
				TenantId = 1,
				UserId = patientA.Id,
				User = patientA,
				Tenant = tenantA,
				Facility = new Facility
				{
					Id = 10,
					Name = "Facility A",
					TenantId = 1,
					Tenant = tenantA,
					FacilityType = new FacilityType { Id = 11, Name = "Padel" },
					Location = new Location
					{
						Id = 12,
						Address = "A",
						AddressNumber = "1",
						City = new City
						{
							Id = 13,
							Name = "Sarajevo",
							Country = new Country { Id = 14, Name = "BiH" }
						}
					}
				},
				AppointmentType = new AppointmentType
				{
					Id = 15,
					Name = "Standard",
					Duration = 60,
					Price = 25,
					TenantId = 1,
					Tenant = tenantA
				}
			};

			context.Set<Tenant>().AddRange(tenantA, tenantB);
			context.Appointment.Add(appointment);
			await context.SaveChangesAsync();

			var currentUser = CreateCurrentUserMock(userId: 200, tenantId: 2).Object;
			var controller = CreateController(context, userManager, currentUser);

			var result = await controller.GetAppointment(appointment.Id);

			result.Result.Should().BeOfType<ForbidResult>();
		}

		[Fact]
		public async Task DeleteAppointment_SetsIsDeletedInsteadOfRemovingRow()
		{
			var dbName = $"AppointmentDelete_{Guid.NewGuid()}";
			await using var context = TestDbContextFactory.CreateSuperAdmin(dbName);
			var userManager = TestUserManagerFactory.Create(context);
			await SeedRoles(context);

			var tenant = new Tenant { Id = 1, Name = "Tenant A" };
			var user = new User
			{
				Id = 101,
				UserName = "patient-delete",
				Email = "patient-delete@test.com",
				Active = true,
				IsDeleted = false,
				TenantId = 1,
				DateCreated = DateTime.UtcNow
			};

			await userManager.CreateAsync(user, "Password123!");
			await userManager.AddToRoleAsync(user, Roles.Korisnik);

			var appointment = new Appointment
			{
				Id = 501,
				AppointmentDateTime = DateTime.UtcNow.AddDays(1),
				IsClosed = false,
				IsFree = false,
				IsDeleted = false,
				TenantId = 1,
				UserId = user.Id,
				User = user,
				Tenant = tenant,
				Facility = new Facility
				{
					Id = 20,
					Name = "Facility Delete",
					TenantId = 1,
					Tenant = tenant,
					FacilityType = new FacilityType { Id = 21, Name = "Padel" },
					Location = new Location
					{
						Id = 22,
						Address = "A",
						AddressNumber = "1",
						City = new City
						{
							Id = 23,
							Name = "Sarajevo",
							Country = new Country { Id = 24, Name = "BiH" }
						}
					}
				},
				AppointmentType = new AppointmentType
				{
					Id = 25,
					Name = "Standard",
					Duration = 60,
					Price = 20,
					TenantId = 1,
					Tenant = tenant
				}
			};

			context.Set<Tenant>().Add(tenant);
			context.Appointment.Add(appointment);
			await context.SaveChangesAsync();

			var currentUser = CreateCurrentUserMock(userId: 201, tenantId: 1).Object;
			var controller = CreateController(context, userManager, currentUser);

			var result = await controller.DeleteAppointment(appointment.Id);

			result.Should().BeOfType<NoContentResult>();

			var deleted = await context.Appointment
				.IgnoreQueryFilters()
				.FirstOrDefaultAsync(x => x.Id == appointment.Id);

			deleted.Should().NotBeNull();
			deleted!.IsDeleted.Should().BeTrue();
		}

		[Fact]
		public async Task CreateAppointment_ReturnsBadRequest_WhenRequestedUserCannotBeAssigned()
		{
			var dbName = $"AppointmentInvalidUser_{Guid.NewGuid()}";
			await using var context = TestDbContextFactory.CreateSuperAdmin(dbName);
			var userManager = TestUserManagerFactory.Create(context);
			await SeedRoles(context);

			var tenantA = new Tenant { Id = 1, Name = "Tenant A" };
			var tenantB = new Tenant { Id = 2, Name = "Tenant B" };

			var workerFromTenantB = new User
			{
				Id = 300,
				UserName = "worker-b",
				Email = "worker-b@test.com",
				Active = true,
				IsDeleted = false,
				TenantId = 2,
				DateCreated = DateTime.UtcNow
			};

			await userManager.CreateAsync(workerFromTenantB, "Password123!");
			await userManager.AddToRoleAsync(workerFromTenantB, Roles.Uposlenik);

			context.Set<Tenant>().AddRange(tenantA, tenantB);
			context.Facility.Add(new Facility
			{
				Id = 30,
				Name = "Facility A",
				TenantId = 1,
				Tenant = tenantA,
				FacilityType = new FacilityType { Id = 31, Name = "Padel" },
				Location = new Location
				{
					Id = 32,
					Address = "A",
					AddressNumber = "1",
					City = new City
					{
						Id = 33,
						Name = "Sarajevo",
						Country = new Country { Id = 34, Name = "BiH" }
					}
				}
			});
			context.AppointmentType.Add(new AppointmentType
			{
				Id = 35,
				Name = "Standard",
				Duration = 60,
				Price = 20,
				TenantId = 1,
				Tenant = tenantA
			});
			await context.SaveChangesAsync();

			var currentUser = CreateCurrentUserMock(userId: 400, tenantId: 1).Object;
			var controller = CreateController(context, userManager, currentUser);

			var dto = new AppointmentCreateDto
			{
				AppointmentDateTime = DateTime.UtcNow.AddDays(2),
				FacilityId = 30,
				AppointmentTypeId = 35,
				UserId = workerFromTenantB.Id,
				IsFree = false,
				IsClosed = false
			};

			var result = await controller.CreateAppointment(dto);

			var badRequest = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
			badRequest.Value.Should().Be("Invalid UserId");
		}

		[Fact]
		public async Task TenantIsolation_TenantB_CannotSeeTenantA_Facilities()
		{
			var dbName = $"TenantIsolation_{Guid.NewGuid()}";

			await using (var seedContext = TestDbContextFactory.CreateSuperAdmin(dbName))
			{
				seedContext.Set<Tenant>().AddRange(
					new Tenant { Id = 1, Name = "Tenant A" },
					new Tenant { Id = 2, Name = "Tenant B" });

				seedContext.Facility.AddRange(
					new Facility
					{
						Id = 1, Name = "Facility A", TenantId = 1,
						FacilityType = new FacilityType { Id = 1, Name = "Padel" },
						Location = new Location
						{
							Id = 1, Address = "A", AddressNumber = "1",
							City = new City { Id = 1, Name = "Sarajevo", Country = new Country { Id = 1, Name = "BiH" } }
						}
					},
					new Facility
					{
						Id = 2, Name = "Facility B", TenantId = 2,
						FacilityType = new FacilityType { Id = 2, Name = "Tennis" },
						Location = new Location
						{
							Id = 2, Address = "B", AddressNumber = "2",
							City = new City { Id = 2, Name = "Mostar", Country = new Country { Id = 2, Name = "BiH2" } }
						}
					});

				await seedContext.SaveChangesAsync();
			}

			await using var tenantBContext = TestDbContextFactory.Create(dbName, tenantId: 2, isSuperAdminOrUser: false);
			var facilities = await tenantBContext.Facility.ToListAsync();

			facilities.Should().HaveCount(1);
			facilities[0].TenantId.Should().Be(2);
			facilities[0].Name.Should().Be("Facility B");
		}

		[Fact]
		public async Task TenantIsolation_TenantA_CannotSeeTenantB_Appointments()
		{
			var dbName = $"TenantIsolationAppt_{Guid.NewGuid()}";

			await using (var seedContext = TestDbContextFactory.CreateSuperAdmin(dbName))
			{
				var tenantA = new Tenant { Id = 1, Name = "Tenant A" };
				var tenantB = new Tenant { Id = 2, Name = "Tenant B" };

				seedContext.Set<Tenant>().AddRange(tenantA, tenantB);

				seedContext.Appointment.AddRange(
					new Appointment
					{
						Id = 1, AppointmentDateTime = DateTime.UtcNow, TenantId = 1, UserId = 1,
						Tenant = tenantA,
						Facility = new Facility
						{
							Id = 10, Name = "F-A", TenantId = 1, Tenant = tenantA,
							FacilityType = new FacilityType { Id = 10, Name = "Type" },
							Location = new Location
							{
								Id = 10, Address = "A", AddressNumber = "1",
								City = new City { Id = 10, Name = "City", Country = new Country { Id = 10, Name = "C" } }
							}
						},
						AppointmentType = new AppointmentType { Id = 10, Name = "Std", Duration = 60, Price = 10, TenantId = 1, Tenant = tenantA }
					},
					new Appointment
					{
						Id = 2, AppointmentDateTime = DateTime.UtcNow, TenantId = 2, UserId = 2,
						Tenant = tenantB,
						Facility = new Facility
						{
							Id = 20, Name = "F-B", TenantId = 2, Tenant = tenantB,
							FacilityType = new FacilityType { Id = 20, Name = "Type2" },
							Location = new Location
							{
								Id = 20, Address = "B", AddressNumber = "2",
								City = new City { Id = 20, Name = "City2", Country = new Country { Id = 20, Name = "C2" } }
							}
						},
						AppointmentType = new AppointmentType { Id = 20, Name = "Std2", Duration = 60, Price = 10, TenantId = 2, Tenant = tenantB }
					});

				await seedContext.SaveChangesAsync();
			}

			await using var tenantAContext = TestDbContextFactory.Create(dbName, tenantId: 1, isSuperAdminOrUser: false);
			var appointments = await tenantAContext.Appointment.ToListAsync();

			appointments.Should().HaveCount(1);
			appointments[0].TenantId.Should().Be(1);
		}
	}
}
