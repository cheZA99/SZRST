using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using SZRST.Application.Services.MailService;
using SZRST.Domain.Constants;
using SZRST.Shared;

namespace Application.Services
{
	public interface IAuthService
	{
		Task<UserManagerResponse> RegisterUserAsync(RegisterViewModel model);

		Task<UserManagerResponse> ConfirmEmailAsync(string userId, string token);

		Task<UserManagerResponse> ForgetPasswordAsync(string email);

		Task<UserManagerResponse> ResetPasswordAsync(ResetPasswordViewModel model);

		Task<AuthResponseDto> LoginUserAsync(LoginViewModel model, string ipAddress);

		Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequest model, string ipAddress);
	}

	public class AuthService :IAuthService
	{
		private UserManager<User> _userManager;
		private RoleManager<Role> _roleManger;
		private IConfiguration _configuration;
		private IMailService _mailService;
		private ISZRSTContext _context;

		public AuthService(UserManager<User> userManager, IConfiguration configuration, IMailService mailService, RoleManager<Role> roleManager, ISZRSTContext context)
		{
			_roleManger = roleManager;
			_userManager = userManager;
			_configuration = configuration;
			_mailService = mailService;
			_context = context;
		}

		public async Task<UserManagerResponse> RegisterUserAsync(RegisterViewModel model)
		{
			if (model == null)
				throw new NullReferenceException("Reigster Model is null");

			if (model.Password != model.ConfirmPassword)
				return new UserManagerResponse
				{
					Message = "Lozinke se ne podudaraju.",
					IsSuccess = false,
				};

			var user = new User
			{
				Email = model.Email,
				UserName = model.Username,
				DateCreated = DateTime.UtcNow,
				DateModified = DateTime.UtcNow,
			};

			var result = await _userManager.CreateAsync(user, model.Password);

			if (result.Succeeded)
			{
				await _userManager.AddToRoleAsync(user, Roles.Korisnik);
				var confirmEmailToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);

				var encodedEmailToken = Encoding.UTF8.GetBytes(confirmEmailToken);
				var validEmailToken = WebEncoders.Base64UrlEncode(encodedEmailToken);

				string url = $"{_configuration["AppUrl"]}/api/auth/confirmemail?userid={user.Id}&token={validEmailToken}";

				return new UserManagerResponse
				{
					Message = "Korisnik uspješno kreiran",
					IsSuccess = true,
				};
			}
			var errorMessages = new IdentityErrorMessages();
			foreach (var error in result.Errors)
			{
				string errorMessage = errorMessages.GetErrorMessage(error.Code);
				var response = new UserManagerResponse
				{
					Message = errorMessage,
					IsSuccess = false,
				};
				return response;
			}

			return new UserManagerResponse
			{
				Message = "Korisnika nemoguće kreirati",
				IsSuccess = false,
				Errors = result.Errors.Select(e => e.Description)
			};
		}

		private async Task<string> GenerateJwtToken(User user)
		{
			var roles = await _userManager.GetRolesAsync(user);

			var claims = new List<Claim>
    {
	   new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
	   new Claim(JwtRegisteredClaimNames.Email, user.Email),
	   new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
	   new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
	   new Claim(ClaimTypes.Name, user.UserName)
    };

			if (user.TenantId.HasValue && user.TenantId.Value > 0)
			{
				claims.Add(new Claim("tenantId", user.TenantId.Value.ToString()));
			}

			foreach (var role in roles)
			{
				claims.Add(new Claim(ClaimTypes.Role, role));
			}

			var key = new SymmetricSecurityKey(
			    Encoding.UTF8.GetBytes(_configuration["AuthSettings:Key"])
			);

			var token = new JwtSecurityToken(
			    issuer: _configuration["AuthSettings:Issuer"],
			    audience: _configuration["AuthSettings:Audience"],
			    claims: claims,
			    expires: DateTime.UtcNow.AddMinutes(
				   int.Parse(_configuration["AuthSettings:AccessTokenMinutes"])
			    ),
			    signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
			);

			return new JwtSecurityTokenHandler().WriteToken(token);
		}

		private string GenerateRefreshToken()
		{
			return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
		}

		public async Task<AuthResponseDto> LoginUserAsync(LoginViewModel model, string ipAddress)
		{
			var user = await _userManager.FindByEmailAsync(model.Email);
			if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
			{
				return new AuthResponseDto
				{
					IsSuccess = false,
					Message = "Invalid email or password"
				};
			}

			var roles = await _userManager.GetRolesAsync(user);
			if (RequiresTenantAssignment(user, roles))
			{
				return new AuthResponseDto
				{
					IsSuccess = false,
					Message = "Korisnik nema validnu organizaciju i pristup je odbijen."
				};
			}

			var accessToken = await GenerateJwtToken(user);
			var refreshToken = GenerateRefreshToken();
			var refreshTokenEntity = new RefreshToken
			{
				Token = refreshToken,
				UserId = user.Id,
				Created = DateTime.UtcNow,
				Expires = DateTime.UtcNow.AddDays(
				   int.Parse(_configuration["AuthSettings:RefreshTokenDays"])
			    ),
				CreatedByIp = ipAddress,
			};

			_context.Set<RefreshToken>().Add(refreshTokenEntity);
			await _context.SaveChangesAsync();

			return new AuthResponseDto
			{
				IsSuccess = true,
				AccessToken = accessToken,
				RefreshToken = refreshToken,
				AccessTokenExpires = DateTime.UtcNow.AddMinutes(
				   int.Parse(_configuration["AuthSettings:AccessTokenMinutes"])
			    )
			};
		}

		public async Task<UserManagerResponse> ConfirmEmailAsync(string userId, string token)
		{
			var user = await _userManager.FindByIdAsync(userId);
			if (user == null)
				return new UserManagerResponse
				{
					IsSuccess = false,
					Message = "Korisnik ne postoji"
				};

			var decodedToken = WebEncoders.Base64UrlDecode(token);
			string normalToken = Encoding.UTF8.GetString(decodedToken);

			var result = await _userManager.ConfirmEmailAsync(user, normalToken);

			if (result.Succeeded)
				return new UserManagerResponse
				{
					Message = "Email confirmed successfully!",
					IsSuccess = true,
				};

			return new UserManagerResponse
			{
				IsSuccess = false,
				Message = "Email did not confirm",
				Errors = result.Errors.Select(e => e.Description)
			};
		}

		public async Task<UserManagerResponse> ForgetPasswordAsync(string email)
		{
			var user = await _userManager.FindByEmailAsync(email);
			if (user == null)
				return new UserManagerResponse
				{
					IsSuccess = false,
					Message = "No user associated with email",
				};

			var token = await _userManager.GeneratePasswordResetTokenAsync(user);
			var encodedToken = Encoding.UTF8.GetBytes(token);
			var validToken = WebEncoders.Base64UrlEncode(encodedToken);

			string url = $"{_configuration["AppUrl"]}/ResetPassword?email={email}&token={validToken}";

			await _mailService.SendEmailAsync(email, "Reset Password", "<h1>Follow the instructions to reset your password</h1>" +
			    $"<p>To reset your password <a href='{url}'>Click here</a></p>");

			return new UserManagerResponse
			{
				IsSuccess = true,
				Message = "Reset password URL has been sent to the email successfully!"
			};
		}

		public async Task<UserManagerResponse> ResetPasswordAsync(ResetPasswordViewModel model)
		{
			var user = await _userManager.FindByEmailAsync(model.Email);
			if (user == null)
				return new UserManagerResponse
				{
					IsSuccess = false,
					Message = "No user associated with email",
				};

			if (model.NewPassword != model.ConfirmPassword)
				return new UserManagerResponse
				{
					IsSuccess = false,
					Message = "Password doesn't match its confirmation",
				};

			var decodedToken = WebEncoders.Base64UrlDecode(model.Token);
			string normalToken = Encoding.UTF8.GetString(decodedToken);

			var result = await _userManager.ResetPasswordAsync(user, normalToken, model.NewPassword);

			if (result.Succeeded)
				return new UserManagerResponse
				{
					Message = "Password has been reset successfully!",
					IsSuccess = true,
				};

			return new UserManagerResponse
			{
				Message = "Something went wrong",
				IsSuccess = false,
				Errors = result.Errors.Select(e => e.Description),
			};
		}

		private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
		{
			var tokenValidationParameters = new TokenValidationParameters
			{
				ValidateAudience = false,
				ValidateIssuer = false,
				ValidateIssuerSigningKey = true,
				IssuerSigningKey = new SymmetricSecurityKey(
				   Encoding.UTF8.GetBytes(_configuration["AuthSettings:Key"])
			    ),
				ValidateLifetime = false
			};

			var tokenHandler = new JwtSecurityTokenHandler();
			return tokenHandler.ValidateToken(token, tokenValidationParameters, out _);
		}

		public async Task<AuthResponseDto> RefreshTokenAsync(
	 RefreshTokenRequest model,
	 string ipAddress)
		{
			var principal = GetPrincipalFromExpiredToken(model.AccessToken);
			var userIdString = principal.Claims
			    .First(x => x.Type == ClaimTypes.NameIdentifier)
			    .Value;

			if (!int.TryParse(userIdString, out int userId))
				return null;

			var token = await _context.Set<RefreshToken>()
			    .FirstOrDefaultAsync(x => x.Token == model.RefreshToken);

			if (token == null ||
			    token.IsRevoked ||
			    token.Expires <= DateTime.UtcNow ||
			    token.UserId != userId)
				return null;

			token.IsRevoked = true;

			var user = await _userManager.FindByIdAsync(userIdString);
			if (user == null)
				return null;

			var roles = await _userManager.GetRolesAsync(user);
			if (RequiresTenantAssignment(user, roles))
				return null;

			var newRefreshToken = new RefreshToken
			{
				Token = GenerateRefreshToken(),
				UserId = userId,
				Created = DateTime.UtcNow,
				Expires = DateTime.UtcNow.AddDays(
				   int.Parse(_configuration["AuthSettings:RefreshTokenDays"])
			    ),
				CreatedByIp = ipAddress
			};

			_context.Set<RefreshToken>().Add(newRefreshToken);
			var newAccessToken = await GenerateJwtToken(user);
			await _context.SaveChangesAsync();

			return new AuthResponseDto
			{
				AccessToken = newAccessToken,
				RefreshToken = newRefreshToken.Token
			};
		}

		private static bool RequiresTenantAssignment(User user, IList<string> roles)
		{
			return !roles.Contains(Roles.SuperAdmin) &&
			       !roles.Contains(Roles.Korisnik) &&
			       (!user.TenantId.HasValue || user.TenantId.Value <= 0);
		}
	}

	public class AuthResponseDto
	{
		public string AccessToken { get; set; }
		public string Message { get; set; }
		public bool IsSuccess { get; set; }

		public string RefreshToken { get; set; }
		public DateTime? AccessTokenExpires { get; set; }
	}

	public class RefreshTokenRequest
	{
		public string AccessToken { get; set; }
		public string RefreshToken { get; set; }
	}
}
