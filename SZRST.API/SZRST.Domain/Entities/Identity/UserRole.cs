
using Microsoft.AspNetCore.Identity;
using System;

namespace Domain.Entities
{
    public class UserRole : IdentityUserRole<int>
    {
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
        public bool IsDeleted { get; set; }

        public User User { get; set; }
        public Role Role { get; set; }
    }
}
