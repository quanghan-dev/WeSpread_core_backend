using System;
using System.Collections.Generic;

#nullable disable

namespace DAL.Model
{
    public partial class RegistrationForm
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string Address { get; set; }
        public string ApplyLocationId { get; set; }
        public string SessionId { get; set; }
        public int CreationCode { get; set; }
        public string CreationMessage { get; set; }
        public string CheckerId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public virtual Location ApplyLocation { get; set; }
        public virtual AppUser Checker { get; set; }
        public virtual RecruitmentSession Session { get; set; }
        public virtual AppUser User { get; set; }
    }
}
