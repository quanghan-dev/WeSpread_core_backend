using BLL.Dto.RegistrationForm;
using DAL.Model;

namespace BLL.Profile
{
    public class RegistrationFormProfile : AutoMapper.Profile
    {
        public RegistrationFormProfile()
        {
            CreateMap<RegistrationForm, RegistrationFormResponse>();

            CreateMap<RegistrationFormRequest, RegistrationForm>();

        }
    }
}
