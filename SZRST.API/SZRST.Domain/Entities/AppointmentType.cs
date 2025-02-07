using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class AppointmentType : BaseEntity<int>
    {
        public string Name { get; set; }
        public int Duration { get; set; }
        public float Price { get; set; }
        public Currency Currency { get; set; }
    }
}
