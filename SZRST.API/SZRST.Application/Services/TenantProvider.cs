using Application.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Application.Services
{
	public class TenantProvider :ITenantProvider
	{
		public int TenantId { get; }
		public bool IsSuperAdminOrUser { get; }

		private static readonly HashSet<string> UnrestrictedRoles = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
	   {
		  "SuperAdmin"
	   };

		public TenantProvider(IHttpContextAccessor httpContextAccessor)
		{
			var httpContext = httpContextAccessor.HttpContext;

			// Ako nema HTTP konteksta ili korisnik nije autentifikovan
			if (httpContext == null || httpContext.User?.Identity?.IsAuthenticated != true)
			{
				TenantId = 0;
				IsSuperAdminOrUser = false;
				return;
			}

			IsSuperAdminOrUser = httpContext.User.Claims
			    .Where(x => x.Type == ClaimTypes.Role)
			    .Select(x => x.Value)
			    .Any(UnrestrictedRoles.Contains);

			var tenantClaim = httpContext.User.Claims
			    .FirstOrDefault(x => x.Type == "tenantId");

			if (tenantClaim == null ||
			    !int.TryParse(tenantClaim.Value, out int tenantId) ||
			    tenantId <= 0)
			{
				TenantId = 0;
				return;
			}

			TenantId = tenantId;
		}
	}
}
