using System;

namespace BLL.Dto.RegistrationForm
{
    [Serializable]
    public class RegistrationFormRequest
    {
        public string Address { get; set; }
        public string ApplyLocationId { get; set; }
        public string SessionId { get; set; }
    }
}
