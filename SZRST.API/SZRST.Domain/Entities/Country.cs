using Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Country : BaseEntity<int>
    {
        public string Name { get; set; }
        public string ShortName { get; set; }
        public Currency? Currency { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
