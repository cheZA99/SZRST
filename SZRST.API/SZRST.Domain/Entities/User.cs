using System;

namespace Domain.Entities
{
    public class User : BaseEntity<int>
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime BirthDate { get; set; }
        public string ProfilePhoto { get; set; }
        public string Address { get; set; }
        public string PostCode { get; set; }
        public string? Biography { get; set; }
        public int ApplicationUserId { get; set; }
    }
}
