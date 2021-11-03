using System;
using System.Collections.Generic;

#nullable disable

namespace DAL.Model
{
    public partial class UserOrganization
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string OrganizationId { get; set; }
        public string RoleId { get; set; }

        public virtual Organization Organization { get; set; }
        public virtual Role Role { get; set; }
        public virtual AppUser User { get; set; }
    }
}
