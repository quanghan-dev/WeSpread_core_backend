using System;

namespace BLL.Dto.Donate
{
    [Serializable]
    public class DonateRequest
    {
        public string DonatorId { get; set; }
        public string DonatorType { get; set; }
        public long Amount { get; set; }
        public string Message { get; set; }
        public string PayType { get; set; }
        public bool IsIncognito { get; set; }
        public string SessionId { get; set; }

        public string RedirectUrl { get; set; }
    }
}
