using System;
using System.ComponentModel.DataAnnotations;

namespace SZRST.API.Models
{
    public class Adresa
    {
        [Key]
        public Guid Id { get; set; }
        public string NazivAdrese { get; set; }
        public string PostanskiBroj { get; set; }
        public Korisnik Korisnik { get; set; }

    }
}
