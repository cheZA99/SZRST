using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using SZRST.Domain.Constants;

public static class IdentitySeed
{
	public static async Task SeedAsync(IServiceProvider services)
	{
		var roleManager = services.GetRequiredService<RoleManager<Role>>();
		var userManager = services.GetRequiredService<UserManager<User>>();

		const string defaultPassword = "Password1!";

		// =====================
		// ROLES
		// =====================
		string[] roles =
		{
		  Roles.SuperAdmin,
		  Roles.Admin,
		  Roles.Uposlenik,
		  Roles.Korisnik
	   };

		foreach (var role in roles)
		{
			if (!await roleManager.RoleExistsAsync(role))
			{
				await roleManager.CreateAsync(new Role { Name = role });
			}
		}

		// =====================
		// SUPER ADMIN
		// =====================
		await EnsureSeedUser(
		    userManager,
		    email: "superadmin@gmail.com",
		    password: defaultPassword,
		    role: Roles.SuperAdmin,
		    tenantId: null
		);

		// =====================
		// ADMIN (example tenant)
		// =====================
		await EnsureSeedUser(
		    userManager,
		    email: "admin@gmail.com",
		    password: defaultPassword,
		    role: Roles.Admin,
		    tenantId: 1
		);

		// =====================
		// UPOSLENIK (same tenant)
		// =====================
		await EnsureSeedUser(
		    userManager,
		    email: "uposlenik@gmail.com",
		    password: defaultPassword,
		    role: Roles.Uposlenik,
		    tenantId: 1
		);

		await EnsureSeedUser(
		    userManager,
		    email: "korisnik@gmail.com",
		    password: defaultPassword,
		    role: Roles.Korisnik,
		    tenantId: null
		);

		// =====================
		// ADMIN (example tenant)
		// =====================
		await EnsureSeedUser(
		    userManager,
		    email: "admin2@gmail.com",
		    password: defaultPassword,
		    role: Roles.Admin,
		    tenantId: 1
		);
	}

	private static async Task EnsureSeedUser(
	    UserManager<User> userManager,
	    string email,
	    string password,
	    string role,
	    int? tenantId
	)
	{
		var user = await userManager.FindByEmailAsync(email);
		if (user == null)
		{
			user = new User
			{
				UserName = email,
				Email = email,
				EmailConfirmed = true,
				TenantId = tenantId,
				Active = true,
				IsDeleted = false
			};

			var createResult = await userManager.CreateAsync(user, password);

			if (!createResult.Succeeded)
			{
				throw new Exception($"Failed to create user {email}: {string.Join(", ", createResult.Errors)}");
			}
		}
		else
		{
			var requiresUpdate =
				user.UserName != email ||
				user.Email != email ||
				user.EmailConfirmed != true ||
				user.TenantId != tenantId ||
				!user.Active ||
				user.IsDeleted;

			if (requiresUpdate)
			{
				user.UserName = email;
				user.Email = email;
				user.EmailConfirmed = true;
				user.TenantId = tenantId;
				user.Active = true;
				user.IsDeleted = false;

				var updateResult = await userManager.UpdateAsync(user);
				if (!updateResult.Succeeded)
				{
					throw new Exception($"Failed to update seeded user {email}: {string.Join(", ", updateResult.Errors)}");
				}
			}
		}

		if (!await userManager.IsInRoleAsync(user, role))
		{
			await userManager.AddToRoleAsync(user, role);
		}

	}
}
