using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using SZRST.Domain.Entities;

namespace Domain.Entities
{
    public class User : IdentityUser<int>, IBaseEntity<int>
    {
        public DateTime DateCreated { get; set; }
        public DateTime? DateModified { get; set; }
        public bool IsDeleted { get; set; }
        public bool Active { get; set; }
        public ICollection<UserClaim> Claims { get; set; }
        public ICollection<UserLogin> Logins { get; set; }
        public ICollection<UserToken> Tokens { get; set; }
        public ICollection<UserRole> UserRoles { get; set; }

        //Nav property
        public AppMember AppMember { get; set; } = null!;
    }
}
