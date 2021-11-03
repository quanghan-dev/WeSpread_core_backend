using System;
using System.Collections.Generic;

#nullable disable

namespace DAL.Model
{
    public partial class AppUser
    {
        public AppUser()
        {
            Comments = new HashSet<Comment>();
            DonateDetailDonators = new HashSet<DonateDetail>();
            DonateDetailUsers = new HashSet<DonateDetail>();
            DonationSessionCheckerNavigations = new HashSet<DonationSession>();
            DonationSessionCreatedByNavigations = new HashSet<DonationSession>();
            OrganizationCheckerNavigations = new HashSet<Organization>();
            OrganizationCreatedByNavigations = new HashSet<Organization>();
            ProjectCheckerNavigations = new HashSet<Project>();
            ProjectCreatedByNavigations = new HashSet<Project>();
            RecruitmentSessionCheckerNavigations = new HashSet<RecruitmentSession>();
            RecruitmentSessionCreatedByNavigations = new HashSet<RecruitmentSession>();
            RegistrationFormCheckers = new HashSet<RegistrationForm>();
            RegistrationFormUsers = new HashSet<RegistrationForm>();
            UserOrganizations = new HashSet<UserOrganization>();
        }

        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Gender { get; set; }
        public string Email { get; set; }
        public string NumberPhone { get; set; }
        public DateTime Birthday { get; set; }
        public bool IsActive { get; set; }
        public bool IsBlock { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string RoleId { get; set; }

        public virtual Role Role { get; set; }
        public virtual PersistentLogin PersistentLogin { get; set; }
        public virtual UserAdmin UserAdmin { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }
        public virtual ICollection<DonateDetail> DonateDetailDonators { get; set; }
        public virtual ICollection<DonateDetail> DonateDetailUsers { get; set; }
        public virtual ICollection<DonationSession> DonationSessionCheckerNavigations { get; set; }
        public virtual ICollection<DonationSession> DonationSessionCreatedByNavigations { get; set; }
        public virtual ICollection<Organization> OrganizationCheckerNavigations { get; set; }
        public virtual ICollection<Organization> OrganizationCreatedByNavigations { get; set; }
        public virtual ICollection<Project> ProjectCheckerNavigations { get; set; }
        public virtual ICollection<Project> ProjectCreatedByNavigations { get; set; }
        public virtual ICollection<RecruitmentSession> RecruitmentSessionCheckerNavigations { get; set; }
        public virtual ICollection<RecruitmentSession> RecruitmentSessionCreatedByNavigations { get; set; }
        public virtual ICollection<RegistrationForm> RegistrationFormCheckers { get; set; }
        public virtual ICollection<RegistrationForm> RegistrationFormUsers { get; set; }
        public virtual ICollection<UserOrganization> UserOrganizations { get; set; }
    }
}
