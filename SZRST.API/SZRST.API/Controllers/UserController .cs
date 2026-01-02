using Domain.Entities;
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
	[Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin}, {Roles.Uposlenik}")]
	[Route("api/[controller]")]
	[ApiController]
	public class UserController :ControllerBase
	{
		private readonly UserManager<User> _userManager;
		private readonly SZRSTContext _context;
		private readonly ICurrentUserService _currentUserService;

		public UserController(
			UserManager<User> userManager,
			SZRSTContext context,
			ICurrentUserService currentUserService)
		{
			_userManager = userManager;
			_context = context;
			_currentUserService = currentUserService;
		}

		// GET: api/User
		[HttpGet]
		public async Task<ActionResult<IEnumerable<UserListDto>>> GetUsers()
		{
			return await _userManager.Users
				.Select(u => new UserListDto
				{
					Id = u.Id,
					UserName = u.UserName,
					Email = u.Email,
					Active = u.Active,
					IsDeleted = u.IsDeleted,
					TenantId = u.TenantId
				})
				.Where(x => x.TenantId == _currentUserService.TenantId)
				.ToListAsync();
		}

		// GET: api/User/{id}
		[HttpGet("{id}")]
		public async Task<ActionResult<UserListDto>> GetUser(int id)
		{
			var user = await _userManager.Users
				.Where(u => u.Id == id)
				.Select(u => new UserListDto
				{
					Id = u.Id,
					UserName = u.UserName,
					Email = u.Email,
					Active = u.Active,
					IsDeleted = u.IsDeleted,
					TenantId = u.TenantId
				})
				.FirstOrDefaultAsync();

			if (user == null)
				return NotFound();

			return user;
		}

		// POST: api/User
		[HttpPost]
		public async Task<IActionResult> CreateUser([FromBody] UserCreateDto dto)
		{
			var user = new User
			{
				UserName = dto.UserName,
				Email = dto.Email,
				Active = dto.Active,
				TenantId = dto.TenantId,
				DateCreated = DateTime.UtcNow
			};

			var result = await _userManager.CreateAsync(user, dto.Password);

			if (!result.Succeeded)
				return BadRequest(result.Errors);

			return Ok(new
			{
				user.Id,
				user.UserName,
				user.Email
			});
		}

		// PUT: api/User/{id}
		[HttpPut("{id}")]
		public async Task<IActionResult> UpdateUser(int id, [FromBody] UserUpdateDto dto)
		{
			var user = await _userManager.FindByIdAsync(id.ToString());
			if (user == null)
				return NotFound();

			user.UserName = dto.UserName;
			user.Email = dto.Email;
			user.Active = dto.Active;
			user.IsDeleted = dto.IsDeleted;
			user.TenantId = dto.TenantId;
			user.DateModified = DateTime.UtcNow;

			var result = await _userManager.UpdateAsync(user);
			if (!result.Succeeded)
				return BadRequest(result.Errors);

			return NoContent();
		}

		// DELETE: api/User/{id}  (soft delete)
		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteUser(int id)
		{
			var user = await _userManager.FindByIdAsync(id.ToString());
			if (user == null)
				return NotFound();

			user.IsDeleted = true;
			user.Active = false;
			user.DateModified = DateTime.UtcNow;

			await _userManager.UpdateAsync(user);
			return NoContent();
		}

		[HttpGet("for-appointments")]
		public async Task<ActionResult<IEnumerable<UserListDto>>> GetUsersForAppointments()
		{
			var tenantId = _currentUserService.TenantId;

			var users = await _userManager.Users
			    .Where(u =>
				   u.TenantId == tenantId ||
				   u.TenantId == null
			    )
			    .Select(u => new UserListDto
			    {
				    Id = u.Id,
				    UserName = u.UserName,
				    Email = u.Email,
				    TenantId = u.TenantId,
				    Active = u.Active,
				    IsDeleted = u.IsDeleted
			    })
			    .ToListAsync();

			return users;
		}

		// GET: api/User/employees
		[HttpGet("employees")]
		public async Task<ActionResult<IEnumerable<UserListDto>>> GetEmployees()
		{
			var currentUser = await _userManager.FindByIdAsync(_currentUserService.UserId.ToString());
			var currentUserRoles = await _userManager.GetRolesAsync(currentUser);

			IQueryable<User> query = _userManager.Users.Include(x => x.Tenant).OrderBy(x => x.TenantId);

			if (currentUserRoles.Contains(Roles.SuperAdmin))
			{
				query = query.Where(u => u.TenantId != null);
			}
			else if (currentUserRoles.Contains(Roles.Admin))
			{
				query = query.Where(u => u.TenantId == _currentUserService.TenantId);
			}

			var employees = await query.ToListAsync();
			var employeeDtos = new List<UserListDto>();

			foreach (var employee in employees)
			{
				var roles = await _userManager.GetRolesAsync(employee);
				if (roles.Contains(Roles.Uposlenik) && !roles.Contains(Roles.SuperAdmin))
				{
					employeeDtos.Add(new UserListDto
					{
						Id = employee.Id,
						UserName = employee.UserName,
						Email = employee.Email,
						Active = employee.Active,
						IsDeleted = employee.IsDeleted,
						TenantId = employee.TenantId,
						Roles = roles.ToList(),
						TenantName = employee.Tenant.Name
					});
				}
			}

			return employeeDtos;
		}

		// POST: api/User/create-employee
		[HttpPost("create-employee")]
		public async Task<IActionResult> CreateEmployee([FromBody] EmployeeCreateDto dto)
		{
			var currentUser = await _userManager.FindByIdAsync(_currentUserService.UserId.ToString());
			var currentUserRoles = await _userManager.GetRolesAsync(currentUser);

			if (currentUserRoles.Contains(Roles.Admin) && dto.TenantId != _currentUserService.TenantId)
			{
				return Forbid("Admin može dodavati uposlenike samo u svoju organizaciju");
			}

			var existingUserByEmail = await _userManager.FindByEmailAsync(dto.Email);
			if (existingUserByEmail != null)
			{
				return BadRequest(new { message = "Korisnik sa tim emailom već postoji." });
			}

			var existingUserByUsername = await _userManager.FindByNameAsync(dto.UserName);
			if (existingUserByUsername != null)
			{
				return BadRequest(new { message = "Korisnik sa tim korisničkim imenom već postoji." });
			}

			var user = new User
			{
				UserName = dto.UserName,
				Email = dto.Email,
				Active = true,
				TenantId = dto.TenantId,
				DateCreated = DateTime.UtcNow,
				DateModified = DateTime.UtcNow
			};

			var result = await _userManager.CreateAsync(user, dto.Password);

			if (!result.Succeeded)
				return BadRequest(new
				{
					message = "Greška pri kreiranju korisnika",
					errors = result.Errors.Select(e => e.Description)
				});

			await _userManager.AddToRoleAsync(user, Roles.Uposlenik);

			return Ok(new
			{
				user.Id,
				user.UserName,
				user.Email,
				user.TenantId,
				Role = Roles.Uposlenik
			});
		}

		// PUT: api/User/update-employee/{id}
		[HttpPut("update-employee/{id}")]
		public async Task<IActionResult> UpdateEmployee(int id, [FromBody] EmployeeUpdateDto dto)
		{
			var user = await _userManager.FindByIdAsync(id.ToString());
			if (user == null)
				return NotFound();

			var currentUser = await _userManager.FindByIdAsync(_currentUserService.UserId.ToString());
			var currentUserRoles = await _userManager.GetRolesAsync(currentUser);

			if (currentUserRoles.Contains(Roles.Admin) && user.TenantId != _currentUserService.TenantId)
			{
				return Forbid("Admin može ažurirati samo uposlenike iz svoje organizacije");
			}

			var existingUserByEmail = await _userManager.FindByEmailAsync(dto.Email);
			if (existingUserByEmail != null && existingUserByEmail.Id != id)
			{
				return BadRequest(new { message = "Korisnik sa tim emailom već postoji." });
			}

			user.UserName = dto.UserName;
			user.Email = dto.Email;
			user.Active = dto.Active;
			user.DateModified = DateTime.UtcNow;

			if (currentUserRoles.Contains(Roles.SuperAdmin))
			{
				user.TenantId = dto.TenantId;
			}

			var result = await _userManager.UpdateAsync(user);

			if (!result.Succeeded)
				return BadRequest(new
				{
					message = "Greška pri ažuriranju korisnika",
					errors = result.Errors.Select(e => e.Description)
				});

			if (!string.IsNullOrEmpty(dto.NewPassword))
			{
				if (dto.NewPassword != dto.ConfirmPassword)
				{
					return BadRequest(new { message = "Lozinke se ne podudaraju." });
				}

				var token = await _userManager.GeneratePasswordResetTokenAsync(user);
				var passwordResult = await _userManager.ResetPasswordAsync(user, token, dto.NewPassword);

				if (!passwordResult.Succeeded)
				{
					return BadRequest(new
					{
						message = "Greška pri promjeni lozinke",
						errors = passwordResult.Errors.Select(e => e.Description)
					});
				}
			}

			return NoContent();
		}

		// GET: api/User/profile
		[HttpGet("profile")]
		public async Task<ActionResult<UserProfileDto>> GetCurrentUserProfile()
		{
			var userId = _currentUserService.UserId;
			var user = await _userManager.Users
			    .Include(u => u.AppMember)
				   .ThenInclude(am => am.City)
			    .Include(u => u.AppMember)
				   .ThenInclude(am => am.Country)
			    .FirstOrDefaultAsync(u => u.Id == userId);

			if (user == null)
				return NotFound();

			return new UserProfileDto
			{
				Id = user.Id,
				UserName = user.UserName,
				Email = user.Email,
				ImageUrl = user.AppMember?.ImageUrl,
				DisplayName = user.AppMember?.DisplayName,
				DateOfBirth = user.AppMember?.DateOfBirth,
				Gender = user.AppMember?.Gender,
				Description = user.AppMember?.Description,
				CityId = user.AppMember?.CityId,
				CityName = user.AppMember?.City?.Name,
				CountryId = user.AppMember?.CountryId,
				CountryName = user.AppMember?.Country?.Name
			};
		}

		// PUT: api/User/profile
		[HttpPut("profile")]
		public async Task<IActionResult> UpdateCurrentUserProfile([FromBody] UserProfileUpdateDto dto)
		{
			var userId = _currentUserService.UserId;
			var user = await _userManager.Users
			    .Include(u => u.AppMember)
			    .FirstOrDefaultAsync(u => u.Id == userId);

			if (user == null)
				return NotFound();

			// Update User basic info
			user.UserName = dto.UserName;
			user.Email = dto.Email;
			user.DateModified = DateTime.UtcNow;

			var result = await _userManager.UpdateAsync(user);
			if (!result.Succeeded)
				return BadRequest(new
				{
					message = "Greška pri ažuriranju korisnika",
					errors = result.Errors.Select(e => e.Description)
				});

			// Update or Create AppMember
			if (user.AppMember == null)
			{
				user.AppMember = new AppMember
				{
					Id = user.Id,
					DisplayName = dto.DisplayName ?? user.UserName,
					DateOfBirth = dto.DateOfBirth ?? DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-18)),
					Gender = dto.Gender ?? "Other",
					DateCreated = DateTime.UtcNow
				};
				_context.AppMembers.Add(user.AppMember);
			}
			else
			{
				user.AppMember.DisplayName = dto.DisplayName ?? user.AppMember.DisplayName;
				user.AppMember.DateOfBirth = dto.DateOfBirth ?? user.AppMember.DateOfBirth;
				user.AppMember.Gender = dto.Gender ?? user.AppMember.Gender;
				user.AppMember.Description = dto.Description;
				user.AppMember.ImageUrl = dto.ImageUrl;
				user.AppMember.CityId = dto.CityId ?? user.AppMember.CityId;
				user.AppMember.CountryId = dto.CountryId ?? user.AppMember.CountryId;
				user.AppMember.DateModified = DateTime.UtcNow;
			}

			// Handle password change if provided
			if (!string.IsNullOrEmpty(dto.NewPassword))
			{
				if (string.IsNullOrEmpty(dto.CurrentPassword))
				{
					return BadRequest(new { message = "Trenutna lozinka je obavezna za promjenu lozinke." });
				}

				var passwordCheck = await _userManager.CheckPasswordAsync(user, dto.CurrentPassword);
				if (!passwordCheck)
				{
					return BadRequest(new { message = "Trenutna lozinka nije ispravna." });
				}

				if (dto.NewPassword != dto.ConfirmPassword)
				{
					return BadRequest(new { message = "Nove lozinke se ne podudaraju." });
				}

				var passwordResult = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
				if (!passwordResult.Succeeded)
				{
					return BadRequest(new
					{
						message = "Greška pri promjeni lozinke",
						errors = passwordResult.Errors.Select(e => e.Description)
					});
				}
			}

			await _context.SaveChangesAsync();

			return NoContent();
		}

		// POST: api/User/profile/upload-image
		[HttpPost("profile/upload-image")]
		public async Task<IActionResult> UploadProfileImage([FromBody] ImageUploadDto dto)
		{
			var userId = _currentUserService.UserId;
			var user = await _userManager.Users
			    .Include(u => u.AppMember)
			    .FirstOrDefaultAsync(u => u.Id == userId);

			if (user == null)
				return NotFound();

			// Store base64 directly in database
			var imageData = dto.Base64Image;

			// Create AppMember if it doesn't exist
			if (user.AppMember == null)
			{
				user.AppMember = new AppMember
				{
					Id = user.Id,
					DisplayName = user.UserName,
					DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-18)),
					Gender = "Other",
					ImageUrl = imageData,
					DateCreated = DateTime.UtcNow,
					DateModified = DateTime.UtcNow,
				};
				_context.AppMembers.Add(user.AppMember);
			}
			else
			{
				user.AppMember.ImageUrl = imageData;
				user.AppMember.DateModified = DateTime.UtcNow;
			}

			await _context.SaveChangesAsync();

			return Ok(new { imageUrl = imageData });
		}
	}

	public class UserListDto
	{
		public int Id { get; set; }
		public string UserName { get; set; }
		public string Email { get; set; }
		public bool Active { get; set; }
		public bool IsDeleted { get; set; }
		public int? TenantId { get; set; }
		public string TenantName { get; set; }
		public List<string> Roles { get; set; } = new List<string>();
	}

	public class UserCreateDto
	{
		public string UserName { get; set; }
		public string Email { get; set; }
		public string Password { get; set; }
		public bool Active { get; set; }
		public int? TenantId { get; set; }
		public string Role { get; set; }
	}

	public class UserUpdateDto
	{
		public string UserName { get; set; }
		public string Email { get; set; }
		public bool Active { get; set; }
		public bool IsDeleted { get; set; }
		public int? TenantId { get; set; }
		public string Role { get; set; }
	}

	public class EmployeeCreateDto
	{
		public string UserName { get; set; }
		public string Email { get; set; }
		public string Password { get; set; }
		public string ConfirmPassword { get; set; }
		public int? TenantId { get; set; }
	}

	public class EmployeeUpdateDto
	{
		public string UserName { get; set; }
		public string Email { get; set; }
		public bool Active { get; set; }
		public int? TenantId { get; set; }
		public string NewPassword { get; set; }
		public string ConfirmPassword { get; set; }
	}

	public class UserProfileDto
	{
		public int Id { get; set; }
		public string UserName { get; set; }
		public string Email { get; set; }
		public string ImageUrl { get; set; }
		public string DisplayName { get; set; }
		public DateOnly? DateOfBirth { get; set; }
		public string Gender { get; set; }
		public string Description { get; set; }
		public int? CityId { get; set; }
		public string CityName { get; set; }
		public int? CountryId { get; set; }
		public string CountryName { get; set; }
	}

	public class UserProfileUpdateDto
	{
		public string UserName { get; set; }
		public string Email { get; set; }
		public string DisplayName { get; set; }
		public DateOnly? DateOfBirth { get; set; }
		public string Gender { get; set; }
		public string Description { get; set; }
		public string ImageUrl { get; set; }
		public int? CityId { get; set; }
		public int? CountryId { get; set; }
		public string CurrentPassword { get; set; }
		public string NewPassword { get; set; }
		public string ConfirmPassword { get; set; }
	}

	public class ImageUploadDto
	{
		public string Base64Image { get; set; }
	}
}