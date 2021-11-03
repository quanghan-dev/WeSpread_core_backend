using System;
using System.Collections.Generic;

#nullable disable

namespace DAL.Model
{
    public partial class RecruitmentSession
    {
        public RecruitmentSession()
        {
            RegistrationForms = new HashSet<RegistrationForm>();
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Description { get; set; }
        public string Requirement { get; set; }
        public string Benefit { get; set; }
        public string JobDescription { get; set; }
        public string ProjectId { get; set; }
        public int CreationCode { get; set; }
        public string CreationMessage { get; set; }
        public string Config { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string CreatedBy { get; set; }
        public string Checker { get; set; }
        public int Quota { get; set; }

        public virtual AppUser CheckerNavigation { get; set; }
        public virtual AppUser CreatedByNavigation { get; set; }
        public virtual Project Project { get; set; }
        public virtual ICollection<RegistrationForm> RegistrationForms { get; set; }
    }
}
