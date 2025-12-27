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
		await CreateUserIfNotExists(
		    userManager,
		    email: "superadmin@gmail.com",
		    password: defaultPassword,
		    role: Roles.SuperAdmin,
		    tenantId: null
		);

		// =====================
		// ADMIN (example tenant)
		// =====================
		await CreateUserIfNotExists(
		    userManager,
		    email: "admin@gmail.com",
		    password: defaultPassword,
		    role: Roles.Admin,
		    tenantId: 1
		);

		// =====================
		// UPOSLENIK (same tenant)
		// =====================
		await CreateUserIfNotExists(
		    userManager,
		    email: "uposlenik@gmail.com",
		    password: defaultPassword,
		    role: Roles.Uposlenik,
		    tenantId: 1
		);

		await CreateUserIfNotExists(
		    userManager,
		    email: "korisnik@gmail.com",
		    password: defaultPassword,
		    role: Roles.Korisnik,
		    tenantId: 1
		);
	}

	private static async Task CreateUserIfNotExists(
	    UserManager<User> userManager,
	    string email,
	    string password,
	    string role,
	    int? tenantId
	)
	{
		var user = await userManager.FindByEmailAsync(email);

		if (user != null)
			return;

		user = new User
		{
			UserName = email,
			Email = email,
			EmailConfirmed = true,
			TenantId = tenantId
		};

		var result = await userManager.CreateAsync(user, password);

		if (!result.Succeeded)
		{
			throw new Exception($"Failed to create user {email}: {string.Join(", ", result.Errors)}");
		}

		await userManager.AddToRoleAsync(user, role);
	}
}