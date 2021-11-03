using System;

namespace BLL.Dto.RecruitmentSession
{
    [Serializable]
    public class RecruitmentSessionRequest
    {
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Description { get; set; }
        public string Requirement { get; set; }
        public string Benefit { get; set; }
        public string JobDescription { get; set; }
        public string ProjectId { get; set; }
        public int Quota { get; set; }
    }
}
