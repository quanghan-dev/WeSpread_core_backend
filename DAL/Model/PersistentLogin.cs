using System;
using System.Collections.Generic;

#nullable disable

namespace DAL.Model
{
    public partial class PersistentLogin
    {
        public string UserId { get; set; }
        public DateTime ExpirationDate { get; set; }
        public string Token { get; set; }
        public DateTime LastLogin { get; set; }

        public virtual AppUser User { get; set; }
    }
}
