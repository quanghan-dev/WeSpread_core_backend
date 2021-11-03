using System;

namespace BLL.Dto.User
{
    [Serializable]
    public class AppUserLoginRequest
    {
        public string NumberPhone { get; set; }

        public string OTP { get; set; }
    }
}
