namespace SZRST.Shared.response
{
	public class FacilityResponse
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public FacilityTypeSummary FacilityType { get; set; }
		public LocationSummary Location { get; set; }
		public string ImageUrl { get; set; }
		public int TenantId { get; set; }
		public string TenantName { get; set; }
	}

	public class FacilityTypeSummary
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
	}

	public class LocationSummary
	{
		public int Id { get; set; }
		public string Address { get; set; }
		public string AddressNumber { get; set; }
		public CitySummary City { get; set; }
		public CountrySummary Country { get; set; }
	}

	public class CitySummary
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public CountrySummary Country { get; set; }
	}

	public class CountrySummary
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string ShortName { get; set; }
	}
}
