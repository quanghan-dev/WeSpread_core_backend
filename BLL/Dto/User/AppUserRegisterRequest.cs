using System;

namespace BLL.Dto.User
{
    [Serializable]
    public class AppUserRegisterRequest
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Gender { get; set; }
        public string Email { get; set; }
        public string NumberPhone { get; set; }
        public DateTime Birthday { get; set; }
    }
}
