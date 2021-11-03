using BLL.Dto.Location;
using BLL.Dto.User;
using System;

namespace BLL.Dto.Member
{
    [Serializable]
    public class MemberApply
    {
        public AppUserResponse User { get; set; }
        public LocationResponse ApplyLocation { get; set; }
    }
}
