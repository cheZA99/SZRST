using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SZRST.Shared
{
    public class LocationCreateDto
    {
        public string Address { get; set; }
        public string AddressNumber { get; set; }
        public int CountryId { get; set; }
        public int CityId { get; set; }
    }
}
