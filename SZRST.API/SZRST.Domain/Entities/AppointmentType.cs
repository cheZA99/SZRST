using SZRST.Domain.Entities;

namespace Domain.Entities
{
	public class AppointmentType :BaseEntity<int>, ITenantEntity
	{
		public string Name { get; set; }
		public int Duration { get; set; }
		public float Price { get; set; }
		public Currency Currency { get; set; }
		public Tenant Tenant { get; set; }
		public int TenantId { get; set; }
	}
}