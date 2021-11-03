using System;
using System.Collections.Generic;

#nullable disable

namespace DAL.Model
{
    public partial class Role
    {
        public Role()
        {
            AppUsers = new HashSet<AppUser>();
            UserOrganizations = new HashSet<UserOrganization>();
        }

        public string Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<AppUser> AppUsers { get; set; }
        public virtual ICollection<UserOrganization> UserOrganizations { get; set; }
    }
}
