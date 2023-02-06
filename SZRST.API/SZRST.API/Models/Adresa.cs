using System;

namespace SZRST.API.Models
{
    public class Adresa
    {
        public Guid Id { get; set; }
        public string NazivAdrese { get; set; }
        public string PostanskiBroj { get; set; }
        public Guid KorisnikId { get; set; }

    }
}
