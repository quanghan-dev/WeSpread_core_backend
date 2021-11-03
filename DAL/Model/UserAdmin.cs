using System;
using System.Collections.Generic;

#nullable disable

namespace DAL.Model
{
    public partial class UserAdmin
    {
        public string UserId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        public virtual AppUser User { get; set; }
    }
}
