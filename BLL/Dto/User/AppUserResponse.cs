using System;

namespace BLL.Dto.User
{
    [Serializable]
    public class AppUserResponse
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Gender { get; set; }
        public string Email { get; set; }
        public string NumberPhone { get; set; }
        public DateTime Birthday { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string RoleId { get; set; }

        public bool IsBlock { get; set; }
    }
}
