using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace Domain.Entities
{
    public class ApplicationUser : IdentityUser<int>, IBaseEntity<int>
    {
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
        public bool IsDeleted { get; set; }
        public bool Active { get; set; }
        public User User { get; set; }
        public bool IsAdministrator { get; set; }
        public bool IsEmployee { get; set; }
        public bool IsClient { get; set; }
        public ICollection<ApplicationUserRole> Roles { get; set; }

    }
}
