
using Microsoft.AspNetCore.Identity;
using System;

namespace Domain.Entities
{
    public class ApplicationRoleClaim : IdentityRoleClaim<int>, IBaseEntity<int>
    {
        public DateTime DateCreated { get; set; } = DateTime.Now;
        public DateTime DateModified { get; set; }
        public bool IsDeleted { get; set; }
    }
}

