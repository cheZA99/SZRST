using Hangfire.Dashboard;
using SZRST.Domain.Constants;

namespace SZRST.API.Security
{
	public class AdminDashboardAuthorizationFilter : IDashboardAuthorizationFilter
	{
		public bool Authorize(DashboardContext context)
		{
			var httpContext = context.GetHttpContext();
			return httpContext.User.Identity?.IsAuthenticated == true &&
			       (httpContext.User.IsInRole(Roles.SuperAdmin) || httpContext.User.IsInRole(Roles.Admin));
		}
	}
}
