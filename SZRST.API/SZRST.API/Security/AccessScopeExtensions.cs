using Domain.Entities;

namespace SZRST.API.Security
{
	public static class AccessScopeExtensions
	{
		public static bool CanAccessTenant(this ICurrentUserService currentUser, int? tenantId)
		{
			return currentUser.IsSuperAdmin || (currentUser.TenantId.HasValue && tenantId == currentUser.TenantId.Value);
		}

		public static bool CanAccessTenant(this ICurrentUserService currentUser, int tenantId)
		{
			return currentUser.IsSuperAdmin || (currentUser.TenantId.HasValue && tenantId == currentUser.TenantId.Value);
		}

		public static bool CanAccessUser(this ICurrentUserService currentUser, User user)
		{
			return currentUser.IsSuperAdmin ||
			       user.Id == currentUser.UserId ||
			       (currentUser.TenantId.HasValue && user.TenantId == currentUser.TenantId.Value);
		}

		public static int ResolveTenantId(this ICurrentUserService currentUser, int? requestedTenantId)
		{
			if (currentUser.IsSuperAdmin)
			{
				return requestedTenantId.GetValueOrDefault();
			}

			return currentUser.TenantId ?? 0;
		}
	}
}
