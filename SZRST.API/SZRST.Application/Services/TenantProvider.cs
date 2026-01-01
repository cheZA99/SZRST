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

		// Role koje mogu vidjeti sve bez tenant filtera
		private static readonly HashSet<string> UnrestrictedRoles = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
	   {
		  "SuperAdmin",
		  "Korisnik"
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

			// Provjeri da li korisnik ima jednu od "neograničenih" rola
			var roleClaim = httpContext.User.Claims
			    .FirstOrDefault(x => x.Type == ClaimTypes.Role);

			IsSuperAdminOrUser = roleClaim != null && UnrestrictedRoles.Contains(roleClaim.Value);

			// Dohvati TenantId
			var tenantClaim = httpContext.User.Claims
			    .FirstOrDefault(x => x.Type == "tenantId");

			if (tenantClaim == null || !int.TryParse(tenantClaim.Value, out int tenantId))
			{
				TenantId = 0;
				return;
			}

			TenantId = tenantId;
		}
	}
}