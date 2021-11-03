using BLL.Dto;
using BLL.Dto.User;

namespace BLL.Service
{
    public interface IAppUserService
    {
        BaseResponse<AppUserResponse> Register(AppUserRegisterRequest appUserRegister);
        BaseResponse<AppUserResponse> VerifyPhone(string phone);
        BaseResponse<AppUserLoginResponse> Login(AppUserLoginRequest appUserLogin);
    }
}
