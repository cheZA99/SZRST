
using Microsoft.AspNetCore.Identity;
using System;

namespace Domain.Entities
{
    public class RoleClaim : IdentityRoleClaim<int>, IBaseEntity<int>
    {
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;
        public DateTime? DateModified { get; set; }
        public bool IsDeleted { get; set; }

        public Role Role { get; set; }

    }
}

