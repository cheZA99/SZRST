using Application.Services;
using FluentValidation;
using Infrastructure.Persistance;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using SZRST.Application.Services.MailService;
using SZRST.Shared;

namespace SZRST.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AuthController :ControllerBase
	{
		private readonly IAuthService _authService;
		private readonly IMailService _mailService;
		private readonly IConfiguration _configuration;
		private readonly SZRSTContext _context;

		public AuthController(
			IAuthService authService,
			IMailService mailService,
			IConfiguration configuration,
			SZRSTContext context)
		{
			_authService = authService;
			_mailService = mailService;
			_configuration = configuration;
			_context = context;
		}

		// /api/auth/register
		[AllowAnonymous]
		[HttpPost("register")]
		public async Task<IActionResult> Register([FromBody] RegisterViewModel model)
		{
			var result = await _authService.RegisterUserAsync(model);
			if (result.IsSuccess)
				return Ok(result);

			return BadRequest(result);
		}

		// /api/auth/login
		[AllowAnonymous]
		[HttpPost("login")]
		public async Task<IActionResult> LoginAsync([FromBody] LoginViewModel model)
		{
			var ipAddress = GetIpAddress();
			var result = await _authService.LoginUserAsync(model, ipAddress);

			if (result.IsSuccess)
				return Ok(result);

			return BadRequest(result);
		}

		[HttpPost("refresh-token")]
		public async Task<IActionResult> RefreshTokenAsync([FromBody] RefreshTokenRequest model)
		{
			var ipAddress = GetIpAddress();
			var result = await _authService.RefreshTokenAsync(model, ipAddress);

			if (result == null)
				return Unauthorized();

			return Ok(result);
		}

		[AllowAnonymous]
		[HttpPost("logout")]
		public async Task<IActionResult> Logout([FromBody] string refreshToken)
		{
			var token = await _context.RefreshTokens
				.FirstOrDefaultAsync(x => x.Token == refreshToken);

			if (token == null)
				return Ok();

			token.IsRevoked = true;
			await _context.SaveChangesAsync();

			return Ok();
		}

		// /api/auth/confirmemail?userid&token
		[HttpGet("ConfirmEmail")]
		public async Task<IActionResult> ConfirmEmail(string userId, string token)
		{
			if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token))
				return NotFound();

			var result = await _authService.ConfirmEmailAsync(userId, token);

			if (result.IsSuccess)
				return Redirect($"{_configuration["AppUrl"]}/ConfirmEmail.html");

			return BadRequest(result);
		}

		// api/auth/forgetpassword
		[HttpPost("ForgetPassword")]
		public async Task<IActionResult> ForgetPassword(string email)
		{
			if (string.IsNullOrEmpty(email))
				return NotFound();

			var result = await _authService.ForgetPasswordAsync(email);

			if (result.IsSuccess)
				return Ok(result);

			return BadRequest(result);
		}

		// api/auth/resetpassword
		[HttpPost("ResetPassword")]
		public async Task<IActionResult> ResetPassword([FromForm] ResetPasswordViewModel model)
		{
			if (ModelState.IsValid)
			{
				var result = await _authService.ResetPasswordAsync(model);

				if (result.IsSuccess)
					return Ok(result);

				return BadRequest(result);
			}

			return BadRequest("Some properties are not valid");
		}

		private string GetIpAddress()
		{
			if (Request.Headers.ContainsKey("X-Forwarded-For"))
				return Request.Headers["X-Forwarded-For"].ToString().Split(',')[0].Trim();

			return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
		}
	}

	public class RegisterViewModelValidator :AbstractValidator<RegisterViewModel>
	{
		public RegisterViewModelValidator()
		{
			RuleFor(x => x.Email)
				.NotEmpty().WithMessage("Email je obavezan.")
				.EmailAddress().WithMessage("Email nije u ispravnom formatu.");

			RuleFor(x => x.Password)
				.NotEmpty().WithMessage("Lozinka je obavezna.")
				.MinimumLength(6).WithMessage("Lozinka mora imati najmanje 6 karaktera.");

			RuleFor(x => x.Username)
				.NotEmpty().WithMessage("Korisničko ime je obavezno.")
				.MinimumLength(3).WithMessage("Korisničko ime mora imati najmanje 3 karaktera.");
		}
	}

	public class LoginViewModelValidator :AbstractValidator<LoginViewModel>
	{
		public LoginViewModelValidator()
		{
			RuleFor(x => x.Email)
				.NotEmpty().WithMessage("Email je obavezan.")
				.EmailAddress().WithMessage("Email nije u ispravnom formatu.");

			RuleFor(x => x.Password)
				.NotEmpty().WithMessage("Lozinka je obavezna.");
		}
	}
}
