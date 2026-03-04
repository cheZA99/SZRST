using Domain.Entities;

namespace SZRST.Shared.response
{
	public class FacilityResponse
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public FacilityType FacilityType { get; set; }
		public Location Location { get; set; }
		public string ImageUrl { get; set; }
		public int TenantId { get; set; }
	}
}
