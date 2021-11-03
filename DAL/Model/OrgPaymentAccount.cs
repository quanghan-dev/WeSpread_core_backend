using System;
using System.Collections.Generic;

#nullable disable

namespace DAL.Model
{
    public partial class OrgPaymentAccount
    {
        public string OrgId { get; set; }
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public string BankCode { get; set; }
        public string CityCode { get; set; }
        public string BankBranch { get; set; }

        public virtual Organization Org { get; set; }
    }
}
