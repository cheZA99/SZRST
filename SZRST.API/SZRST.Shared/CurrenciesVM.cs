using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SZRST.Shared
{
    public class CurrenciesVM
    {
        public List<Row> Currencies { get; set; }
        public string StatusMessage { get; set; }

        public class Row
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string ShortName { get; set; }
        }
    }
}
