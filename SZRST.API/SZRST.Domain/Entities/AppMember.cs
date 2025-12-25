using Domain.Entities;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SZRST.Domain.Entities
{
	public class AppMember :BaseEntity<int>, ITenantEntity
	{
		public DateOnly DateOfBirth { get; set; }
		public string? ImageUrl { get; set; }
		public required string DisplayName { get; set; }
		public DateTime LastActive { get; set; } = DateTime.UtcNow;
		public required string Gender { get; set; }
		public string? Description { get; set; }
		public required string City { get; set; }
		public required string Country { get; set; }

		//Navigation property
		[ForeignKey(nameof(Id))]
		public User User { get; set; } = null!;

		public int TenantId { get; set; }
		public Tenant Tenant { get; set; }
	}
}