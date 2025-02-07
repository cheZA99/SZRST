using Microsoft.AspNetCore.Identity;
using System;

namespace Domain.Entities
{
    public class UserToken : IdentityUserToken<int>, IBaseEntity<int>
    {
        public int Id { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DateModified { get; set; }
        public bool IsDeleted { get; set; }

        public User User { get; set; }

    }
}
