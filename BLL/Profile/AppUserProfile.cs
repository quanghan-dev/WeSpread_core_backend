using BLL.Dto.User;
using DAL.Model;


namespace BLL.Profile
{
    public class AppUserProfile : AutoMapper.Profile
    {
        public AppUserProfile()
        {
            CreateMap<AppUserRegisterRequest, AppUser>();

            CreateMap<AppUser, AppUserResponse>();

            CreateMap<AppUser, AppUserLoginResponse>();
        }
    }
}
