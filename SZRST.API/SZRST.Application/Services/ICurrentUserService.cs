using System;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Security.Claims;

public interface ICurrentUserService
{
	int UserId { get; }
	[Obsolete("Use HasRole(string role) instead. This property only returns the first role claim and is unreliable for multi-role users.")]
	string Role { get; }
	int? TenantId { get; }
	string Username { get; }
	bool IsAuthenticated { get; }
	bool IsSuperAdmin { get; }
	bool IsKorisnik { get; }
	bool HasValidTenant { get; }
	bool HasRole(string role);
}

public class CurrentUserService : ICurrentUserService
{
	private readonly IHttpContextAccessor _httpContextAccessor;

	public CurrentUserService(IHttpContextAccessor httpContextAccessor)
	{
		_httpContextAccessor = httpContextAccessor;
	}

	public int UserId
	{
		get
		{
			var userIdClaim = _httpContextAccessor.HttpContext?.User?
			    .FindFirst(ClaimTypes.NameIdentifier)?.Value;
			return int.TryParse(userIdClaim, out int userId) ? userId : userId;
		}
	}

	public int? TenantId
	{
		get
		{
			var tenantIdClaim = _httpContextAccessor.HttpContext?.User?
			    .FindFirst("tenantId")?.Value;
			return int.TryParse(tenantIdClaim, out var tenantId) && tenantId > 0 ? tenantId : null;
		}
	}

	[Obsolete("Use HasRole(string role) instead. This property only returns the first role claim and is unreliable for multi-role users.")]
	public string Role
	{
		get
		{
			return _httpContextAccessor.HttpContext?.User?
			    .FindFirst(ClaimTypes.Role)?.Value;
		}
	}

	public string Username => _httpContextAccessor.HttpContext?.User?
	    .FindFirst(ClaimTypes.Name)?.Value;

	public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

	public bool IsSuperAdmin => HasRole(SZRST.Domain.Constants.Roles.SuperAdmin);

	public bool IsKorisnik => HasRole(SZRST.Domain.Constants.Roles.Korisnik);

	public bool HasValidTenant => IsSuperAdmin || TenantId.HasValue;

	public bool HasRole(string role)
	{
		if (string.IsNullOrWhiteSpace(role))
		{
			return false;
		}

		return _httpContextAccessor.HttpContext?.User?.Claims
			.Any(c => c.Type == ClaimTypes.Role && c.Value == role) ?? false;
	}
}
