using BLL.Dto.User;
using System;

namespace BLL.Dto.Member
{
    [Serializable]
    public class MemberDonate
    {
        public AppUserResponse User { get; set; }

        public bool IsIncognito { get; set; }
        public double AmountDonated { get; set; }
    }
}
