using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class WorkerType : BaseEntity<int>
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
