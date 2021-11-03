using System;

namespace BLL.Dto.DonationSession
{
    [Serializable]
    public class DonationSessionRequest
    {
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public double Target { get; set; }
        public string Description { get; set; }
        public string ProjectId { get; set; }
    }
}
