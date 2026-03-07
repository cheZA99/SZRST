using Domain.Entities;
using FluentValidation;
using Infrastructure.Persistance;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SZRST.Domain.Constants;
using SZRST.Domain.Entities;

namespace SZRST.API.Controllers
{
	[Authorize(Roles = Roles.SuperAdmin)]
	[Authorize]
	[Route("api/[controller]")]
	[ApiController]
	public class TenantController :ControllerBase
	{
		private readonly SZRSTContext _context;
		private readonly UserManager<User> _userManager;
		private readonly IValidator<CreateTenantWithAdminDto> _createValidator;
		private readonly IValidator<UpdateTenantDto> _updateValidator;

		public TenantController(
			SZRSTContext context,
			UserManager<User> userManager,
			IValidator<CreateTenantWithAdminDto> createValidator,
			IValidator<UpdateTenantDto> updateValidator)
		{
			_context = context;
			_userManager = userManager;
			_createValidator = createValidator;
			_updateValidator = updateValidator;
		}

		// GET: api/tenant
		[AllowAnonymous]
		[HttpGet]
		public async Task<ActionResult<IEnumerable<TenantDto>>> GetAllTenants()
		{
			var tenants = await _context.Set<Tenant>()
				.Select(t => new TenantDto
				{
					Id = t.Id,
					Name = t.Name,
					UserCount = _context.Users.Count(u => u.TenantId == t.Id)
				})
				.ToListAsync();

			return Ok(tenants);
		}

		// GET: api/tenant/{id}
		[HttpGet("{id}")]
		public async Task<ActionResult<TenantDto>> GetTenant(int id)
		{
			var tenant = await _context.Set<Tenant>()
				.Where(t => t.Id == id)
				.Select(t => new TenantDto
				{
					Id = t.Id,
					Name = t.Name,
					UserCount = _context.Users.Count(u => u.TenantId == t.Id)
				})
				.FirstOrDefaultAsync();

			if (tenant == null)
				return NotFound();

			return Ok(tenant);
		}

		// POST: api/tenant
		[HttpPost]
		public async Task<ActionResult<TenantCreationResponse>> CreateTenantWithAdmin([FromBody] CreateTenantWithAdminDto model)
		{
			// --- FluentValidation ---
			var validation = await _createValidator.ValidateAsync(model);
			if (!validation.IsValid)
				return BadRequest(new TenantCreationResponse
				{
					IsSuccess = false,
					Message = "Podaci nisu validni",
					Errors = validation.Errors.Select(e => e.ErrorMessage)
				});

			var existingTenant = await _context.Set<Tenant>()
				.AnyAsync(t => t.Name.ToLower() == model.TenantName.ToLower());

			if (existingTenant)
				return BadRequest(new TenantCreationResponse
				{
					IsSuccess = false,
					Message = "Organizacija sa tim imenom već postoji."
				});

			var existingUserByEmail = await _userManager.FindByEmailAsync(model.AdminEmail);
			if (existingUserByEmail != null)
				return BadRequest(new TenantCreationResponse
				{
					IsSuccess = false,
					Message = "Korisnik sa tim emailom već postoji."
				});

			var existingUserByUsername = await _userManager.FindByNameAsync(model.AdminUsername);
			if (existingUserByUsername != null)
				return BadRequest(new TenantCreationResponse
				{
					IsSuccess = false,
					Message = "Korisnik sa tim korisničkim imenom već postoji."
				});

			var tenant = new Tenant
			{
				Name = model.TenantName,
				DateCreated = DateTime.UtcNow,
				DateModified = DateTime.UtcNow
			};

			_context.Set<Tenant>().Add(tenant);
			await _context.SaveChangesAsync();

			var adminUser = new User
			{
				Email = model.AdminEmail,
				UserName = model.AdminUsername,
				TenantId = tenant.Id,
				DateCreated = DateTime.UtcNow,
				DateModified = DateTime.UtcNow,
				EmailConfirmed = true
			};

			var result = await _userManager.CreateAsync(adminUser, model.AdminPassword);

			if (result.Succeeded)
			{
				await _userManager.AddToRoleAsync(adminUser, Roles.Admin);

				return Ok(new TenantCreationResponse
				{
					IsSuccess = true,
					Message = "Organizacija i admin korisnik uspješno kreirani",
					Tenant = new TenantDto { Id = tenant.Id, Name = tenant.Name, UserCount = 1 }
				});
			}

			_context.Set<Tenant>().Remove(tenant);
			await _context.SaveChangesAsync();

			var errorMessages = new IdentityErrorMessages();
			var errors = result.Errors.Select(e => errorMessages.GetErrorMessage(e.Code)).ToList();

			return BadRequest(new TenantCreationResponse
			{
				IsSuccess = false,
				Message = errors.FirstOrDefault() ?? "Greška pri kreiranju admin korisnika",
				Errors = errors
			});
		}

		// PUT: api/tenant/{id}
		[HttpPut("{id}")]
		public async Task<ActionResult<TenantDto>> UpdateTenant(int id, [FromBody] UpdateTenantDto updateDto)
		{
			// --- FluentValidation ---
			var validation = await _updateValidator.ValidateAsync(updateDto);
			if (!validation.IsValid)
				return BadRequest(validation.Errors.Select(e => e.ErrorMessage));

			var tenant = await _context.Set<Tenant>().FindAsync(id);
			if (tenant == null)
				return NotFound();

			tenant.Name = updateDto.Name;
			tenant.DateModified = DateTime.UtcNow;

			try
			{
				await _context.SaveChangesAsync();
			}
			catch (DbUpdateConcurrencyException)
			{
				if (!await TenantExists(id))
					return NotFound();
				throw;
			}

			return Ok(new TenantDto
			{
				Id = tenant.Id,
				Name = tenant.Name,
				UserCount = await _context.Users.CountAsync(u => u.TenantId == tenant.Id)
			});
		}

		// DELETE: api/tenant/{id}
		[HttpDelete("{id}")]
		public async Task<ActionResult> DeleteTenant(int id)
		{
			var tenant = await _context.Set<Tenant>().FindAsync(id);
			if (tenant == null)
				return NotFound();

			var hasUsers = await _context.Users.AnyAsync(u => u.TenantId == id);
			if (hasUsers)
				return BadRequest(new { message = "Ne možete obrisati organizaciju koja ima korisnike." });

			_context.Set<Tenant>().Remove(tenant);
			await _context.SaveChangesAsync();

			return NoContent();
		}

		private async Task<bool> TenantExists(int id)
		{
			return await _context.Set<Tenant>().AnyAsync(e => e.Id == id);
		}
	}

	public class CreateTenantWithAdminDto
	{
		public string TenantName { get; set; }
		public string AdminEmail { get; set; }
		public string AdminUsername { get; set; }
		public string AdminPassword { get; set; }
		public string AdminConfirmPassword { get; set; }
	}

	public class TenantDto
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public int UserCount { get; set; }
	}

	public class UpdateTenantDto
	{
		public string Name { get; set; }
	}

	public class TenantCreationResponse
	{
		public bool IsSuccess { get; set; }
		public string Message { get; set; }
		public TenantDto Tenant { get; set; }
		public IEnumerable<string> Errors { get; set; }
	}

	public class CreateTenantWithAdminDtoValidator :AbstractValidator<CreateTenantWithAdminDto>
	{
		public CreateTenantWithAdminDtoValidator()
		{
			RuleFor(x => x.TenantName)
				.NotEmpty().WithMessage("Naziv organizacije je obavezan.")
				.MaximumLength(100).WithMessage("Naziv organizacije ne smije biti duži od 100 karaktera.");

			RuleFor(x => x.AdminEmail)
				.NotEmpty().WithMessage("Email admina je obavezan.")
				.EmailAddress().WithMessage("Email nije u ispravnom formatu.");

			RuleFor(x => x.AdminUsername)
				.NotEmpty().WithMessage("Korisničko ime je obavezno.")
				.MinimumLength(3).WithMessage("Korisničko ime mora imati najmanje 3 karaktera.");

			RuleFor(x => x.AdminPassword)
				.NotEmpty().WithMessage("Lozinka je obavezna.")
				.MinimumLength(6).WithMessage("Lozinka mora imati najmanje 6 karaktera.");

			RuleFor(x => x.AdminConfirmPassword)
				.Equal(x => x.AdminPassword).WithMessage("Lozinke se ne podudaraju.");
		}
	}

	public class UpdateTenantDtoValidator :AbstractValidator<UpdateTenantDto>
	{
		public UpdateTenantDtoValidator()
		{
			RuleFor(x => x.Name)
				.NotEmpty().WithMessage("Naziv organizacije je obavezan.")
				.MaximumLength(100).WithMessage("Naziv ne smije biti duži od 100 karaktera.");
		}
	}
}