using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SZRST.Shared
{
    public class CurrencyVM
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "You need to enter name")]
        public string Name { get; set; }
        [Required(ErrorMessage = "You need to enter short name")]
        public string ShortName { get; set; }
    }
}
