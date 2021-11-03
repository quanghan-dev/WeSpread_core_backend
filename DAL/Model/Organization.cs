using System;
using System.Collections.Generic;

#nullable disable

namespace DAL.Model
{
    public partial class Organization
    {
        public Organization()
        {
            Comments = new HashSet<Comment>();
            ItemCategories = new HashSet<ItemCategory>();
            ItemLocations = new HashSet<ItemLocation>();
            Projects = new HashSet<Project>();
            UserOrganizations = new HashSet<UserOrganization>();
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public string EngName { get; set; }
        public string Cover { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public string Mission { get; set; }
        public string Vision { get; set; }
        public string Achievement { get; set; }
        public bool IsActive { get; set; }
        public DateTime FoundingDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string Config { get; set; }
        public int CreationCode { get; set; }
        public string CreationMessage { get; set; }
        public string Checker { get; set; }
        public string TaxCode { get; set; }
        public string Logo { get; set; }

        public virtual AppUser CheckerNavigation { get; set; }
        public virtual AppUser CreatedByNavigation { get; set; }
        public virtual OrgPaymentAccount OrgPaymentAccount { get; set; }
        public virtual OrgRepresentative OrgRepresentative { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }
        public virtual ICollection<ItemCategory> ItemCategories { get; set; }
        public virtual ICollection<ItemLocation> ItemLocations { get; set; }
        public virtual ICollection<Project> Projects { get; set; }
        public virtual ICollection<UserOrganization> UserOrganizations { get; set; }
    }
}
