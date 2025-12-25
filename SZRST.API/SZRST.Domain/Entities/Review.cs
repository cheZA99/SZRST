using SZRST.Domain.Entities;

namespace Domain.Entities
{
	public class Review :BaseEntity<int>, ITenantEntity
	{
		public int Rating { get; set; }
		public string Description { get; set; }

		public User User { get; set; }
		public Facility Facility { get; set; }
		public Tenant Tenant { get; set; }
		public int TenantId { get; set; }
	}
}