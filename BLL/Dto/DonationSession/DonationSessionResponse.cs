using BLL.Dto.Project;
using System;

namespace BLL.Dto.DonationSession
{
    [Serializable]
    public class DonationSessionResponse
    {
        public string Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public double Target { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ProjectId { get; set; }
        public int CreationCode { get; set; }
        public string CreationMessage { get; set; }
        public string Config { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string CreatedBy { get; set; }
        public double TotalAmountDonated { get; set; }
        public double TotalDonations { get; set; }
        public ProjectResponse Project { get; set; }
    }
}
