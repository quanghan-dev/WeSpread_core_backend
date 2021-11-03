using BLL.Dto.DonationSession;
using System;

namespace BLL.Dto.Donate
{
    [Serializable]
    public class DonateResultResponse
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string DonatorId { get; set; }
        public string DonatorType { get; set; }
        public double Amount { get; set; }
        public string Message { get; set; }
        public string PayType { get; set; }
        public bool IsIncognito { get; set; }
        public string TransId { get; set; }
        public DateTime DonateTime { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int ResultCode { get; set; }
        public string ResultMessage { get; set; }
        public DonationSessionResponse Session { get; set; }
    }
}
