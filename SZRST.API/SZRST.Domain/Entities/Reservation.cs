using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;

namespace Domain.Entities
{
    public class Reservation : BaseEntity<int>
    {
        public DateTime ReservationDateTime { get; set; }
        public string Message { get; set; }
        public User User { get; set; }
        public Appointment Appointment { get; set; }
    }
}
