namespace Domain.Entities
{
	public class FacilityType :BaseEntity<int>
	{
		public string Name { get; set; }
		public string Description { get; set; }

		public override string ToString()
		{
			return Name;
		}
	}
}