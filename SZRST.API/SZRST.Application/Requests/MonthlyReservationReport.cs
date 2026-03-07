using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SZRST.Application.Requests
{
    public class MonthlyReservationReport
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int TotalReservations { get; set; }
        public float Profit { get; set; }

        public string FacilityName { get; set; }
    }
}
