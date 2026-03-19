using Microsoft.AspNetCore.Http;
using System.Security.Claims;

public interface ICurrentUserService
{
	int UserId { get; }
	string Role { get; }
	int? TenantId { get; }
	string Username { get; }
	bool IsAuthenticated { get; }
	bool IsSuperAdmin { get; }
	bool IsKorisnik { get; }
	bool HasValidTenant { get; }
}

public class CurrentUserService :ICurrentUserService
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

	public bool IsSuperAdmin => _httpContextAccessor.HttpContext?.User?.IsInRole(SZRST.Domain.Constants.Roles.SuperAdmin) ?? false;

	public bool IsKorisnik => _httpContextAccessor.HttpContext?.User?.IsInRole(SZRST.Domain.Constants.Roles.Korisnik) ?? false;

	public bool HasValidTenant => IsSuperAdmin || TenantId.HasValue;
}
