using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Location : BaseEntity<int>
    {
        public string Address { get; set; }
        public string AddressNumber { get; set; }

        public Country Country { get; set; }
        public City City { get; set; }

        public override string ToString()
        {
            return $"{Address} {AddressNumber}, {City}, {Country}";
        }
    }
}
