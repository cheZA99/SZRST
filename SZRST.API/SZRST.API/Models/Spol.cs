using System;
using System.ComponentModel.DataAnnotations;

namespace SZRST.API.Models
{
    public class Spol
    {
        [Key]
        public Guid Id { get; set; }
        public string Naziv { get; set; }
    }
}
