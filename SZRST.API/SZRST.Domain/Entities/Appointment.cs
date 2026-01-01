using System;
using SZRST.Domain.Entities;

namespace Domain.Entities
{
	public class Appointment :BaseEntity<int>, ITenantEntity
	{
		public DateTime AppointmentDateTime { get; set; }
		public bool IsFree { get; set; }
		public bool IsClosed { get; set; }
		public Facility Facility { get; set; }
		public AppointmentType AppointmentType { get; set; }
		public int TenantId { get; set; }
		public Tenant Tenant { get; set; }
		public int UserId { get; set; }
		public User User { get; set; }
	}
}