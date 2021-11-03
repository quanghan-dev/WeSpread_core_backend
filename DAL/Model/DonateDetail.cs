using System;
using System.Collections.Generic;

#nullable disable

namespace DAL.Model
{
    public partial class DonateDetail
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string DonatorId { get; set; }
        public string TransId { get; set; }
        public string DonatorType { get; set; }
        public double Amount { get; set; }
        public string Message { get; set; }
        public string PayType { get; set; }
        public DateTime DonateTime { get; set; }
        public int ResultCode { get; set; }
        public string ResultMessage { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsIncognito { get; set; }
        public string SessionId { get; set; }

        public virtual AppUser Donator { get; set; }
        public virtual PaymentMethod PayTypeNavigation { get; set; }
        public virtual DonationSession Session { get; set; }
        public virtual AppUser User { get; set; }
    }
}
