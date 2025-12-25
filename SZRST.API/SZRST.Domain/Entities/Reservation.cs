using System;
using SZRST.Domain.Entities;

namespace Domain.Entities
{
	public class Reservation :BaseEntity<int>, ITenantEntity
	{
		public DateTime ReservationDateTime { get; set; }
		public string Message { get; set; }
		public User User { get; set; }
		public Appointment Appointment { get; set; }
		public Tenant Tenant { get; set; }
		public int TenantId { get; set; }
	}
}