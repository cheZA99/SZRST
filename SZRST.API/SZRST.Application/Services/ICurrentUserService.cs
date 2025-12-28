using Microsoft.AspNetCore.Http;
using System.Security.Claims;

public interface ICurrentUserService
{
	int? UserId { get; }
	int? TenantId { get; }
	string Username { get; }
	bool IsAuthenticated { get; }
}

public class CurrentUserService :ICurrentUserService
{
	private readonly IHttpContextAccessor _httpContextAccessor;

	public CurrentUserService(IHttpContextAccessor httpContextAccessor)
	{
		_httpContextAccessor = httpContextAccessor;
	}

	public int? UserId
	{
		get
		{
			var userIdClaim = _httpContextAccessor.HttpContext?.User?
			    .FindFirst(ClaimTypes.NameIdentifier)?.Value;
			return int.TryParse(userIdClaim, out var userId) ? userId : null;
		}
	}

	public int? TenantId
	{
		get
		{
			var tenantIdClaim = _httpContextAccessor.HttpContext?.User?
			    .FindFirst("tenantId")?.Value;
			return int.TryParse(tenantIdClaim, out var tenantId) ? tenantId : null;
		}
	}

	public string Username => _httpContextAccessor.HttpContext?.User?
	    .FindFirst(ClaimTypes.Name)?.Value;

	public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
}