using SZRST.Domain.Entities;

namespace Domain.Entities
{
	public class Facility :BaseEntity<int>, ITenantEntity
	{
		public string Name { get; set; }
		public FacilityType FacilityType { get; set; }
		public Location Location { get; set; }

		public string ImageUrl { get; set; }
		public Tenant Tenant { get; set; }
		public int TenantId { get; set; }
	}
}