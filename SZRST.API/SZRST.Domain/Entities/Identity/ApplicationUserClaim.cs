using Microsoft.AspNetCore.Identity;
using System;

namespace Domain.Entities
{ 
    public class ApplicationUserClaim : IdentityUserClaim<int>, IBaseEntity<int>
    {
        public DateTime DateCreated { get; set; } = DateTime.Now;
        public DateTime DateModified { get; set; }
        public bool IsDeleted { get; set; }
    }
}
