using System;
using SZRST.Domain.Entities;

namespace Domain.Entities
{
	public class Worker :BaseEntity<int>, ITenantEntity
	{
		public DateTime DateOfEmployment { get; set; }
		public User User { get; set; }
		public WorkerType WorkerType { get; set; }
		public Facility Facility { get; set; }
		public Tenant Tenant { get; set; }
		public int TenantId { get; set; }
	}
}