using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Review : BaseEntity<int>
    {
        public int Rating { get; set; }
        public string Description { get; set; }

        public User User { get; set; } 
        public Facility Facility { get; set; }
    }
}
