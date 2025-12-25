using SZRST.Domain.Entities;

namespace Domain.Entities
{
	public class WorkerType :BaseEntity<int>, ITenantEntity
	{
		public string Name { get; set; }
		public string Description { get; set; }

		public Tenant Tenant { get; set; }
		public int TenantId { get; set; }
	}
}