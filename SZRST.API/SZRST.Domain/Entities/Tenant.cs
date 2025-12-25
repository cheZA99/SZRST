using Domain.Entities;
using System.Collections.Generic;

namespace SZRST.Domain.Entities
{
	public class Tenant :BaseEntity<int>
	{
		public required string Name { get; set; }
		public ICollection<User> Users { get; set; }
	}
}