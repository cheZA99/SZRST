using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class City : BaseEntity<int>
    {
        public string Name { get; set; }
        public Country Country { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
