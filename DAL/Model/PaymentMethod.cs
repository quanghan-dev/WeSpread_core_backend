using System;
using System.Collections.Generic;

#nullable disable

namespace DAL.Model
{
    public partial class PaymentMethod
    {
        public PaymentMethod()
        {
            DonateDetails = new HashSet<DonateDetail>();
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }

        public virtual ICollection<DonateDetail> DonateDetails { get; set; }
    }
}
