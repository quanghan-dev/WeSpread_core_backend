using System;
using System.Collections.Generic;

#nullable disable

namespace DAL.Model
{
    public partial class Project
    {
        public Project()
        {
            Comments = new HashSet<Comment>();
            DonationSessions = new HashSet<DonationSession>();
            ItemCategories = new HashSet<ItemCategory>();
            ItemLocations = new HashSet<ItemLocation>();
            RecruitmentSessions = new HashSet<RecruitmentSession>();
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public string EngName { get; set; }
        public string Description { get; set; }
        public string OrgId { get; set; }
        public string Cover { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string Config { get; set; }
        public int CreationCode { get; set; }
        public string CreationMessage { get; set; }
        public string Checker { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Logo { get; set; }
        public bool IsActive { get; set; }

        public virtual AppUser CheckerNavigation { get; set; }
        public virtual AppUser CreatedByNavigation { get; set; }
        public virtual Organization Org { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }
        public virtual ICollection<DonationSession> DonationSessions { get; set; }
        public virtual ICollection<ItemCategory> ItemCategories { get; set; }
        public virtual ICollection<ItemLocation> ItemLocations { get; set; }
        public virtual ICollection<RecruitmentSession> RecruitmentSessions { get; set; }
    }
}
