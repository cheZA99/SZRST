using System;
using System.ComponentModel.DataAnnotations;

namespace SZRST.API.Models
{
    public class Korisnik
    {
        [Key]
        public Guid Id { get; set; }
        public string Ime { get; set; }
        public string Prezime { get; set; }
        public string KorisnickoIme { get; set; }
        public string Lozinka { get; set; }
        public string Email { get; set; }
        public string Token { get; set; }
        //dodat rola model
        public string Rola { get; set; }
        //public string ImeIPrezime => $"{Ime} {Prezime}";

        //navigation
        //public Spol Spol { get; set; }
        //public Adresa Adresa { get; set; }

    }
}
