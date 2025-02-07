using Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Facility : BaseEntity<int>
    {
        public string Name { get; set; }
        public FacilityType FacilityType { get; set; }
        public Location Location { get; set; }

    }
}
