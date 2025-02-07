using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace Domain.Entities
{
    public class Role : IdentityRole<int>, IBaseEntity<int>
    {
        
        public DateTime DateCreated { get; set; } = DateTime.Now;
        public DateTime? DateModified { get; set; }
        public bool IsDeleted { get; set; }

        public ICollection<UserRole> UserRoles { get; set; }

        public ICollection<RoleClaim> RoleClaims { get; set; }  

    }
}
