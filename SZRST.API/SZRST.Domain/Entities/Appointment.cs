using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Appointment : BaseEntity<int>
    {
        public DateTime AppointmentDateTime { get; set; }
        public bool IsFree { get; set; }
        public bool IsClosed { get; set; }
        public Facility Facility { get; set; }
        public AppointmentType AppointmentType { get; set; }
    }
}
