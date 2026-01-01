namespace Application.Interfaces
{
	public interface ITenantProvider
	{
		int TenantId { get; }
		bool IsSuperAdminOrUser { get; }
	}
}