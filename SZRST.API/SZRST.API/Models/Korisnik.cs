using System;

namespace SZRST.API.Models
{
    public class Korisnik
    {
        public Guid Id { get; set; }
        public string Ime { get; set; }
        public string Prezime { get; set; }
        public DateTime GodinaRodjenja { get; set; }
        public string Email { get; set; }
        public string BrojTelefona { get; set; }
        public string KorisnickoIme { get; set; }
        public string ImeIPrezime => $"{Ime} {Prezime}";
        //navigation
        public Spol Spol { get; set; }
        public Adresa Adresa { get; set; }

    }
}
