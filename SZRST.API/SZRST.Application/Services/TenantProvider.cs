using Application.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace Application.Services
{
	public class TenantProvider :ITenantProvider
	{
		public int TenantId { get; }

		public TenantProvider(IHttpContextAccessor httpContextAccessor)
		{
			var httpContext = httpContextAccessor.HttpContext;

			// Ako nema HTTP konteksta ili korisnik nije autentifikovan
			if (httpContext == null || httpContext.User?.Identity?.IsAuthenticated != true)
			{
				TenantId = 0;
				return;
			}

			var claim = httpContext.User.Claims
			    .FirstOrDefault(x => x.Type == "tenantId");

			if (claim == null)
			{
				TenantId = 0;
				return;
			}

			if (!int.TryParse(claim.Value, out int tenantId))
			{
				TenantId = 0;
				return;
			}

			TenantId = tenantId;
		}
	}
}