using BLL.Dto.RecruitmentSession;
using System;

namespace BLL.Dto.RegistrationForm
{
    [Serializable]
    public class RegistrationFormResponse
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string Address { get; set; }
        public string ApplyLocationId { get; set; }
        public string SessionId { get; set; }
        public int CreationCode { get; set; }
        public string CreationMessage { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string CheckerId { get; set; }

        public RecruitmentSessionResponse Session { get; set; }
    }
}
