
using Microsoft.AspNetCore.Identity;
using System;

namespace Domain.Entities
{
    public class ApplicationUserRole : IdentityUserRole<int>, IBaseEntity<int>
    {
        public int Id { get; set; }
        public ApplicationUser User { get; set; }
        public ApplicationRole Role { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
        public bool IsDeleted { get; set; }
    }
}
