using Domain.Entities;
using FluentValidation;
using Infrastructure.Persistance;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SZRST.API.Security;
using SZRST.API.Services;
using SZRST.Domain.Constants;
using SZRST.Domain.Entities;

namespace SZRST.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class UserController :ControllerBase
	{
		private const int MaxProfileImageBytes = 2 * 1024 * 1024;
		private readonly UserManager<User> _userManager;
		private readonly SZRSTContext _context;
		private readonly ICurrentUserService _currentUserService;
		private readonly IValidator<EmployeeCreateDto> _employeeCreateValidator;
		private readonly IValidator<EmployeeUpdateDto> _employeeUpdateValidator;
		private readonly IWebHostEnvironment _environment;
		private readonly IEncryptionService _encryptionService;

		public UserController(
			UserManager<User> userManager,
			SZRSTContext context,
			ICurrentUserService currentUserService,
			IValidator<EmployeeCreateDto> employeeCreateValidator,
			IValidator<EmployeeUpdateDto> employeeUpdateValidator,
			IWebHostEnvironment environment,
			IEncryptionService encryptionService)
		{
			_userManager = userManager;
			_context = context;
			_currentUserService = currentUserService;
			_employeeCreateValidator = employeeCreateValidator;
			_employeeUpdateValidator = employeeUpdateValidator;
			_environment = environment;
			_encryptionService = encryptionService;
		}

		// GET: api/User
		[Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin}")]
		[HttpGet]
		public async Task<ActionResult<IEnumerable<UserListDto>>> GetUsers()
		{
			var users = await _userManager.Users
				.Where(x => _currentUserService.IsSuperAdmin || x.TenantId == _currentUserService.TenantId)
				.Select(u => new UserListDto
				{
					Id = u.Id,
					UserName = u.UserName,
					Email = u.Email,
					Active = u.Active,
					IsDeleted = u.IsDeleted,
					TenantId = u.TenantId,
					FirstName = u.FirstName,
					LastName = u.LastName,
					JMBG = u.JMBG
				})
				.ToListAsync();

			foreach (var u in users)
				u.JMBG = _encryptionService.Decrypt(u.JMBG);

			return Ok(users);
		}

		// GET: api/User/{id}
		[Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin}")]
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
					TenantId = u.TenantId,
					FirstName = u.FirstName,
					LastName = u.LastName,
					JMBG = u.JMBG
				})
				.FirstOrDefaultAsync();

			if (user == null)
				return NotFound();

			if (!_currentUserService.CanAccessTenant(user.TenantId))
				return Forbid();

			user.JMBG = _encryptionService.Decrypt(user.JMBG);
			return Ok(user);
		}

		// POST: api/User
		[Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin}")]
		[HttpPost]
		public async Task<IActionResult> CreateUser([FromBody] UserCreateDto dto)
		{
			if (!_currentUserService.HasValidTenant)
				return Forbid();

			var tenantId = _currentUserService.IsSuperAdmin ? dto.TenantId : _currentUserService.TenantId;
			if (!_currentUserService.IsSuperAdmin && tenantId != _currentUserService.TenantId)
				return Forbid();

			var user = new User
			{
				UserName = dto.UserName,
				Email = dto.Email,
				Active = dto.Active,
				TenantId = tenantId,
				FirstName = dto.FirstName,
				LastName = dto.LastName,
				JMBG = _encryptionService.Encrypt(dto.JMBG),
				DateCreated = DateTime.UtcNow
			};

			var result = await _userManager.CreateAsync(user, dto.Password);

			if (!result.Succeeded)
				return BadRequest(result.Errors);

			return Ok(new { user.Id, user.UserName, user.Email });
		}

		// PUT: api/User/{id}
		[Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin}")]
		[HttpPut("{id}")]
		public async Task<IActionResult> UpdateUser(int id, [FromBody] UserUpdateDto dto)
		{
			var user = await _userManager.FindByIdAsync(id.ToString());
			if (user == null)
				return NotFound();

			if (!_currentUserService.CanAccessUser(user))
				return Forbid();

			user.UserName = dto.UserName;
			user.Email = dto.Email;
			user.Active = dto.Active;
			user.IsDeleted = dto.IsDeleted;
			if (_currentUserService.IsSuperAdmin)
			{
				user.TenantId = dto.TenantId;
			}
			user.FirstName = dto.FirstName;
			user.LastName = dto.LastName;
			user.JMBG = _encryptionService.Encrypt(dto.JMBG);
			user.DateModified = DateTime.UtcNow;

			var result = await _userManager.UpdateAsync(user);
			if (!result.Succeeded)
				return BadRequest(result.Errors);

			return NoContent();
		}

		// DELETE: api/User/{id} (soft delete)
		[Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin}")]
		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteUser(int id)
		{
			var user = await _userManager.FindByIdAsync(id.ToString());
			if (user == null)
				return NotFound();

			if (!_currentUserService.CanAccessUser(user))
				return Forbid();

			user.IsDeleted = true;
			user.Active = false;
			user.DateModified = DateTime.UtcNow;

			await _userManager.UpdateAsync(user);
			return NoContent();
		}

		[Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Uposlenik}")]
		[HttpGet("for-appointments")]
		public async Task<ActionResult<IEnumerable<UserListDto>>> GetUsersForAppointments([FromQuery] int? tenantId = null)
		{
			var targetTenantId = tenantId;

			if (_currentUserService.IsSuperAdmin)
			{
				if (!targetTenantId.HasValue || targetTenantId.Value <= 0)
					return BadRequest(new { message = "TenantId je obavezan." });
			}
			else
			{
				if (!_currentUserService.TenantId.HasValue)
					return Forbid();

				targetTenantId ??= _currentUserService.TenantId.Value;

				if (targetTenantId != _currentUserService.TenantId.Value)
					return Forbid();
			}

			var korisnikRoleId = await _context.Roles
				.Where(r => r.Name == Roles.Korisnik)
				.Select(r => r.Id)
				.FirstOrDefaultAsync();

			var usersQuery = _userManager.Users
				.Where(u =>
					!u.IsDeleted &&
					u.Active &&
					(
						u.TenantId == targetTenantId.Value ||
						(u.TenantId == null &&
						 _context.UserRoles.Any(ur => ur.UserId == u.Id && ur.RoleId == korisnikRoleId))
					));

			var users = await usersQuery
				.Select(u => new UserListDto
				{
					Id = u.Id,
					UserName = u.UserName,
					Email = u.Email,
					TenantId = u.TenantId,
					Active = u.Active,
					IsDeleted = u.IsDeleted,
					FirstName = u.FirstName,
					LastName = u.LastName
				})
				.ToListAsync();

			return users;
		}

		// GET: api/User/employees
		[Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin}")]
		[HttpGet("employees")]
		public async Task<ActionResult<PagedResult<UserListDto>>> GetEmployees([FromQuery] EmployeeFilterDto filter)
		{
			var currentUser = await _userManager.FindByIdAsync(_currentUserService.UserId.ToString());
			var currentUserRoles = await _userManager.GetRolesAsync(currentUser);
			var employeeRoleId = await _context.Roles
				.Where(r => r.Name == Roles.Uposlenik)
				.Select(r => r.Id)
				.FirstOrDefaultAsync();

			IQueryable<User> query = _userManager.Users
				.Include(x => x.Tenant)
				.Where(u => _context.UserRoles.Any(ur => ur.UserId == u.Id && ur.RoleId == employeeRoleId))
				.OrderBy(x => x.TenantId)
				.ThenBy(x => x.UserName);

			if (currentUserRoles.Contains(Roles.SuperAdmin))
				query = query.Where(u => u.TenantId != null);
			else if (currentUserRoles.Contains(Roles.Admin))
				query = query.Where(u => u.TenantId == _currentUserService.TenantId);

			if (!string.IsNullOrWhiteSpace(filter.UserName))
				query = query.Where(u => u.UserName.ToLower().Contains(filter.UserName.ToLower()));

			if (!string.IsNullOrWhiteSpace(filter.Email))
				query = query.Where(u => u.Email.ToLower().Contains(filter.Email.ToLower()));

			if (!string.IsNullOrWhiteSpace(filter.FirstName))
				query = query.Where(u => u.FirstName != null && u.FirstName.ToLower().Contains(filter.FirstName.ToLower()));

			if (!string.IsNullOrWhiteSpace(filter.LastName))
				query = query.Where(u => u.LastName != null && u.LastName.ToLower().Contains(filter.LastName.ToLower()));

			if (currentUserRoles.Contains(Roles.SuperAdmin) && filter.TenantId.HasValue)
				query = query.Where(u => u.TenantId == filter.TenantId.Value);

			if (filter.IsDeleted.HasValue)
				query = query.Where(u => u.IsDeleted == filter.IsDeleted.Value);

			int pageNumber = filter.PageNumber < 1 ? 1 : filter.PageNumber;
			int pageSize = filter.PageSize < 1 ? 10 : (filter.PageSize > 100 ? 100 : filter.PageSize);

			var totalCount = await query.CountAsync();
			var pagedUsers = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

			var employeeDtos = pagedUsers.Select(employee => new UserListDto
			{
				Id = employee.Id,
				UserName = employee.UserName,
				Email = employee.Email,
				Active = employee.Active,
				IsDeleted = employee.IsDeleted,
				TenantId = employee.TenantId,
				FirstName = employee.FirstName,
				LastName = employee.LastName,
				JMBG = _encryptionService.Decrypt(employee.JMBG),
				Roles = new List<string> { Roles.Uposlenik },
				TenantName = employee.Tenant?.Name
			}).ToList();

			return Ok(new PagedResult<UserListDto>
			{
				Items = employeeDtos,
				TotalCount = totalCount,
				PageNumber = pageNumber,
				PageSize = pageSize,
				TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
			});
		}

		// POST: api/User/create-employee
		[Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin}")]
		[HttpPost("create-employee")]
		public async Task<IActionResult> CreateEmployee([FromBody] EmployeeCreateDto dto)
		{
			// --- FluentValidation ---
			var validation = await _employeeCreateValidator.ValidateAsync(dto);
			if (!validation.IsValid)
				return BadRequest(validation.Errors.Select(e => new { message = e.ErrorMessage }));

			var currentUser = await _userManager.FindByIdAsync(_currentUserService.UserId.ToString());
			var currentUserRoles = await _userManager.GetRolesAsync(currentUser);

			if (currentUserRoles.Contains(Roles.Admin) && dto.TenantId != _currentUserService.TenantId)
				return Forbid("Admin može dodavati uposlenike samo u svoju organizaciju");

			if (await _userManager.FindByEmailAsync(dto.Email) != null)
				return BadRequest(new { message = "Korisnik sa tim emailom već postoji." });

			if (await _userManager.FindByNameAsync(dto.UserName) != null)
				return BadRequest(new { message = "Korisnik sa tim korisničkim imenom već postoji." });

			var user = new User
			{
				UserName = dto.UserName,
				Email = dto.Email,
				FirstName = dto.FirstName,
				LastName = dto.LastName,
				Active = true,
				TenantId = dto.TenantId,
				JMBG = _encryptionService.Encrypt(dto.JMBG),
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
				user.FirstName,
				user.LastName,
				user.TenantId,
				Role = Roles.Uposlenik
			});
		}

		// PUT: api/User/update-employee/{id}
		[Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin}")]
		[HttpPut("update-employee/{id}")]
		public async Task<IActionResult> UpdateEmployee(int id, [FromBody] EmployeeUpdateDto dto)
		{
			// --- FluentValidation ---
			var validation = await _employeeUpdateValidator.ValidateAsync(dto);
			if (!validation.IsValid)
				return BadRequest(validation.Errors.Select(e => new { message = e.ErrorMessage }));

			var user = await _userManager.FindByIdAsync(id.ToString());
			if (user == null)
				return NotFound();

			var currentUser = await _userManager.FindByIdAsync(_currentUserService.UserId.ToString());
			var currentUserRoles = await _userManager.GetRolesAsync(currentUser);

			if (currentUserRoles.Contains(Roles.Admin) && user.TenantId != _currentUserService.TenantId)
				return Forbid("Admin može ažurirati samo uposlenike iz svoje organizacije");

			var existingByEmail = await _userManager.FindByEmailAsync(dto.Email);
			if (existingByEmail != null && existingByEmail.Id != id)
				return BadRequest(new { message = "Korisnik sa tim emailom već postoji." });

			var existingByUsername = await _userManager.FindByNameAsync(dto.UserName);
			if (existingByUsername != null && existingByUsername.Id != id)
				return BadRequest(new { message = "Korisnik sa tim korisničkim imenom već postoji." });

			user.UserName = dto.UserName;
			user.Email = dto.Email;
			user.FirstName = dto.FirstName;
			user.LastName = dto.LastName;
			user.Active = dto.Active;
			user.JMBG = _encryptionService.Encrypt(dto.JMBG);
			user.DateModified = DateTime.UtcNow;

			if (currentUserRoles.Contains(Roles.SuperAdmin))
				user.TenantId = dto.TenantId;

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
					return BadRequest(new { message = "Lozinke se ne podudaraju." });

				var token = await _userManager.GeneratePasswordResetTokenAsync(user);
				var passwordResult = await _userManager.ResetPasswordAsync(user, token, dto.NewPassword);

				if (!passwordResult.Succeeded)
					return BadRequest(new
					{
						message = "Greška pri promjeni lozinke",
						errors = passwordResult.Errors.Select(e => e.Description)
					});
			}

			return NoContent();
		}

		[Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Uposlenik},{Roles.Korisnik}")]
		[HttpGet("profile")]
		public async Task<ActionResult<UserProfileDto>> GetCurrentUserProfile()
		{
			var userId = _currentUserService.UserId;
			var user = await _userManager.Users
				.Include(u => u.AppMember).ThenInclude(am => am.City)
				.Include(u => u.AppMember).ThenInclude(am => am.Country)
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

		[Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Uposlenik},{Roles.Korisnik}")]
		[HttpPut("profile")]
		public async Task<IActionResult> UpdateCurrentUserProfile([FromBody] UserProfileUpdateDto dto)
		{
			var userId = _currentUserService.UserId;
			var user = await _userManager.Users
				.Include(u => u.AppMember)
				.FirstOrDefaultAsync(u => u.Id == userId);

			if (user == null)
				return NotFound();

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
				if (dto.ImageUrl != null)
				{
					user.AppMember.ImageUrl = dto.ImageUrl;
				}
				user.AppMember.CityId = dto.CityId ?? user.AppMember.CityId;
				user.AppMember.CountryId = dto.CountryId ?? user.AppMember.CountryId;
				user.AppMember.DateModified = DateTime.UtcNow;
			}

			if (!string.IsNullOrEmpty(dto.NewPassword))
			{
				if (string.IsNullOrEmpty(dto.CurrentPassword))
					return BadRequest(new { message = "Trenutna lozinka je obavezna za promjenu lozinke." });

				if (!await _userManager.CheckPasswordAsync(user, dto.CurrentPassword))
					return BadRequest(new { message = "Trenutna lozinka nije ispravna." });

				if (dto.NewPassword != dto.ConfirmPassword)
					return BadRequest(new { message = "Nove lozinke se ne podudaraju." });

				var passwordResult = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
				if (!passwordResult.Succeeded)
					return BadRequest(new
					{
						message = "Greška pri promjeni lozinke",
						errors = passwordResult.Errors.Select(e => e.Description)
					});
			}

			await _context.SaveChangesAsync();
			return NoContent();
		}

		[Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin},{Roles.Uposlenik},{Roles.Korisnik}")]
		[HttpPost("profile/upload-image")]
		public async Task<IActionResult> UploadProfileImage([FromBody] ImageUploadDto dto)
		{
			if (dto == null)
				return BadRequest(new { message = "Podaci za upload slike su obavezni." });

			var userId = _currentUserService.UserId;
			var user = await _userManager.Users
				.Include(u => u.AppMember)
				.FirstOrDefaultAsync(u => u.Id == userId);

			if (user == null)
				return NotFound();

			if (string.IsNullOrWhiteSpace(dto.Base64Image))
			{
				var previousImageUrl = user.AppMember?.ImageUrl;

				if (user.AppMember == null)
				{
					user.AppMember = new AppMember
					{
						Id = user.Id,
						DisplayName = user.UserName,
						DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-18)),
						Gender = "Other",
						ImageUrl = null,
						DateCreated = DateTime.UtcNow,
						DateModified = DateTime.UtcNow,
					};
					_context.AppMembers.Add(user.AppMember);
				}
				else
				{
					user.AppMember.ImageUrl = null;
					user.AppMember.DateModified = DateTime.UtcNow;
				}

				await _context.SaveChangesAsync();
				DeleteLocalFile(previousImageUrl);
				return Ok(new { imageUrl = (string)null });
			}

			if (!TryParseBase64Image(dto.Base64Image, out var imageBytes, out var extension))
			{
				return BadRequest(new { message = "Dozvoljene su samo validne JPG, PNG, GIF ili WEBP slike." });
			}

			if (imageBytes.Length > MaxProfileImageBytes)
			{
				return BadRequest(new { message = "Profilna slika ne smije biti veća od 2MB." });
			}

			var imageUrl = await SaveProfileImageAsync(imageBytes, extension);
			var oldImageUrl = user.AppMember?.ImageUrl;

			if (user.AppMember == null)
			{
				user.AppMember = new AppMember
				{
					Id = user.Id,
					DisplayName = user.UserName,
					DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-18)),
					Gender = "Other",
					ImageUrl = imageUrl,
					DateCreated = DateTime.UtcNow,
					DateModified = DateTime.UtcNow,
				};
				_context.AppMembers.Add(user.AppMember);
			}
			else
			{
				user.AppMember.ImageUrl = imageUrl;
				user.AppMember.DateModified = DateTime.UtcNow;
			}

			await _context.SaveChangesAsync();
			if (!string.Equals(oldImageUrl, imageUrl, StringComparison.OrdinalIgnoreCase))
			{
				DeleteLocalFile(oldImageUrl);
			}

			return Ok(new { imageUrl });
		}

		private async Task<string> SaveProfileImageAsync(byte[] imageBytes, string extension)
		{
			var folder = Path.Combine(_environment.WebRootPath, "profile-images");
			if (!Directory.Exists(folder))
			{
				Directory.CreateDirectory(folder);
			}

			var fileName = $"{Guid.NewGuid():N}{extension}";
			var filePath = Path.Combine(folder, fileName);
			await System.IO.File.WriteAllBytesAsync(filePath, imageBytes);
			return $"/profile-images/{fileName}";
		}

		private static bool TryParseBase64Image(string base64Image, out byte[] imageBytes, out string extension)
		{
			imageBytes = null;
			extension = null;

			var separatorIndex = base64Image.IndexOf(";base64,", StringComparison.OrdinalIgnoreCase);
			if (!base64Image.StartsWith("data:image/", StringComparison.OrdinalIgnoreCase) || separatorIndex <= 0)
			{
				return false;
			}

			var mimeType = base64Image.Substring("data:".Length, separatorIndex - "data:".Length);
			extension = mimeType.ToLowerInvariant() switch
			{
				"image/jpeg" => ".jpeg",
				"image/jpg" => ".jpg",
				"image/png" => ".png",
				"image/gif" => ".gif",
				"image/webp" => ".webp",
				_ => null
			};

			if (extension == null)
			{
				return false;
			}

			var rawBase64 = base64Image.Substring(separatorIndex + ";base64,".Length);
			try
			{
				imageBytes = Convert.FromBase64String(rawBase64);
			}
			catch (FormatException)
			{
				return false;
			}

			return FileSignatureValidator.IsValidImage(imageBytes, extension);
		}

		private void DeleteLocalFile(string imageUrl)
		{
			if (string.IsNullOrWhiteSpace(imageUrl) || !imageUrl.StartsWith("/", StringComparison.Ordinal))
			{
				return;
			}

			var relativePath = imageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
			var fullPath = Path.Combine(_environment.WebRootPath, relativePath);

			if (System.IO.File.Exists(fullPath))
			{
				System.IO.File.Delete(fullPath);
			}
		}
	}

	public class PagedResult<T>
	{
		public List<T> Items { get; set; } = new();
		public int TotalCount { get; set; }
		public int PageNumber { get; set; }
		public int PageSize { get; set; }
		public int TotalPages { get; set; }
	}

	public class EmployeeFilterDto
	{
		public string? UserName { get; set; }
		public string? Email { get; set; }
		public string? FirstName { get; set; }
		public string? LastName { get; set; }
		public int? TenantId { get; set; }
		public bool? IsDeleted { get; set; }
		public int PageNumber { get; set; } = 1;
		public int PageSize { get; set; } = 10;
	}

	public class UserListDto
	{
		public int Id { get; set; }
		public string UserName { get; set; }
		public string Email { get; set; }
		public string? FirstName { get; set; }
		public string? LastName { get; set; }
		public bool Active { get; set; }
		public bool IsDeleted { get; set; }
		public int? TenantId { get; set; }
		public string TenantName { get; set; }
		public string? JMBG { get; set; }
		public List<string> Roles { get; set; } = new();
	}

	public class UserCreateDto
	{
		public string UserName { get; set; }
		public string Email { get; set; }
		public string? FirstName { get; set; }
		public string? LastName { get; set; }
		public string Password { get; set; }
		public bool Active { get; set; }
		public int? TenantId { get; set; }
		public string Role { get; set; }
		public string? JMBG { get; set; }
	}

	public class UserUpdateDto
	{
		public string UserName { get; set; }
		public string Email { get; set; }
		public string? FirstName { get; set; }
		public string? LastName { get; set; }
		public bool Active { get; set; }
		public bool IsDeleted { get; set; }
		public int? TenantId { get; set; }
		public string Role { get; set; }
		public string? JMBG { get; set; }
	}

	public class EmployeeCreateDto
	{
		public string UserName { get; set; }
		public string Email { get; set; }
		public string? FirstName { get; set; }
		public string? LastName { get; set; }
		public string Password { get; set; }
		public string ConfirmPassword { get; set; }
		public int? TenantId { get; set; }
		public string? JMBG { get; set; }
	}

	public class EmployeeUpdateDto
	{
		public string UserName { get; set; }
		public string Email { get; set; }
		public string? FirstName { get; set; }
		public string? LastName { get; set; }
		public bool Active { get; set; }
		public int? TenantId { get; set; }
		public string? JMBG { get; set; }
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

	public class EmployeeCreateDtoValidator :AbstractValidator<EmployeeCreateDto>
	{
		public EmployeeCreateDtoValidator()
		{
			RuleFor(x => x.UserName)
				.NotEmpty().WithMessage("Korisničko ime je obavezno.")
				.MinimumLength(3).WithMessage("Korisničko ime mora imati najmanje 3 karaktera.");

			RuleFor(x => x.Email)
				.NotEmpty().WithMessage("Email je obavezan.")
				.EmailAddress().WithMessage("Email nije u ispravnom formatu.");

			RuleFor(x => x.Password)
				.NotEmpty().WithMessage("Lozinka je obavezna.")
				.MinimumLength(6).WithMessage("Lozinka mora imati najmanje 6 karaktera.");

			RuleFor(x => x.ConfirmPassword)
				.Equal(x => x.Password).WithMessage("Lozinke se ne podudaraju.");
		}
	}

	public class EmployeeUpdateDtoValidator :AbstractValidator<EmployeeUpdateDto>
	{
		public EmployeeUpdateDtoValidator()
		{
			RuleFor(x => x.UserName)
				.NotEmpty().WithMessage("Korisničko ime je obavezno.")
				.MinimumLength(3).WithMessage("Korisničko ime mora imati najmanje 3 karaktera.");

			RuleFor(x => x.Email)
				.NotEmpty().WithMessage("Email je obavezan.")
				.EmailAddress().WithMessage("Email nije u ispravnom formatu.");

			When(x => !string.IsNullOrEmpty(x.NewPassword), () =>
			{
				RuleFor(x => x.NewPassword)
					.MinimumLength(6).WithMessage("Nova lozinka mora imati najmanje 6 karaktera.");

				RuleFor(x => x.ConfirmPassword)
					.Equal(x => x.NewPassword).WithMessage("Lozinke se ne podudaraju.");
			});
		}
	}
}
