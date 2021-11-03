using AutoMapper;
using BLL.Constant;
using BLL.Dto;
using BLL.Dto.User;
using BLL.Dto.Exception;
using DAL.Model;
using DAL.UnifOfWork;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Linq;
using System.Net;

namespace BLL.Service.Impl
{
    public class AppUserService : IAppUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;
        private readonly IDistributedCache _distributedCache;
        private readonly IJwtAuthenticationManager _jwtAuthenticationManager;
        private readonly IPersistentLoginService _persistentLoginService;
        private readonly IUtilService _utilService;
        private readonly IValidateDataService _validateDataService;
        private static readonly string ROLE_USER = "USER";
        

        public AppUserService(IUnitOfWork unitOfWork, IMapper mapper, ILogger logger, IDistributedCache distributedCache,
            IJwtAuthenticationManager jwtAuthenticationManager, IPersistentLoginService persistentLoginService,
            IUtilService utilService, IValidateDataService validateDataService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _distributedCache = distributedCache;
            _jwtAuthenticationManager = jwtAuthenticationManager;
            _persistentLoginService = persistentLoginService;
            _utilService = utilService;
            _validateDataService = validateDataService;
        }

        //Verify Phone number
        public BaseResponse<AppUserResponse> VerifyPhone(string phone)
        {
            BaseResponse<AppUserResponse> response;

            AppUserResponse appUserResponse;

            //validate phone
            if (!_validateDataService.IsValidPhone(phone))
            {
                _logger.Error($"User enter wrong phone format: {phone}");

                throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<AppUserResponse>
                    {
                        ResultCode = ResultCode.INVALID_PHONE_CODE,
                        ResultMessage = ResultCode.GetMessage(ResultCode.INVALID_PHONE_CODE),
                        Data = null
                    });
            }

            string vietNamPhone = _utilService.changeToVietnamPhoneNumber(phone);

            AppUser user;

            try
            {
                user = _unitOfWork.GetRepository<AppUser>()
                .GetAll()
                .FirstOrDefault(u =>
                u.NumberPhone.Equals(vietNamPhone));
            }
            catch (Exception e)
            {
                _logger.Error(e.Message);

                throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<AppUserResponse>
                {
                    ResultCode = ResultCode.SQL_ERROR_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.SQL_ERROR_CODE),
                    Data = default
                });
            }

            if (user == null) //user not found
            {
                appUserResponse = new AppUserResponse{ NumberPhone = vietNamPhone };

                response = new BaseResponse<AppUserResponse>
                {
                    ResultCode = ResultCode.USER_NOT_FOUND_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.USER_NOT_FOUND_CODE),
                    Data = appUserResponse
                };
            }
            else if (!user.IsActive) //user is inactive
            {
                appUserResponse = _mapper.Map<AppUserResponse>(user);

                response = new BaseResponse<AppUserResponse>
                {
                    ResultCode = ResultCode.USER_INACTIVE_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.USER_INACTIVE_CODE),
                    Data = appUserResponse
                };
            }
            else if (user.IsBlock) //user is block
            {
                appUserResponse = _mapper.Map<AppUserResponse>(user);

                response = new BaseResponse<AppUserResponse>
                {
                    ResultCode = ResultCode.USER_BLOCKED_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.USER_BLOCKED_CODE),
                    Data = appUserResponse
                };
            }
            else //default
            {
                appUserResponse = _mapper.Map<AppUserResponse>(user);

                response = new BaseResponse<AppUserResponse>
                {
                    ResultCode = ResultCode.SUCCESS_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.SUCCESS_CODE),
                    Data = appUserResponse
                };
            }

            return response;
        }

        //Register
        public BaseResponse<AppUserResponse> Register(AppUserRegisterRequest appUserRegister)
        {
            //validate name
            if (!_validateDataService.IsValidName(appUserRegister.FirstName, "name") ||
                !_validateDataService.IsValidName(appUserRegister.LastName, "name"))
            {
                _logger.Error($"User enter invalid name: Firstname: '{appUserRegister.FirstName}', Lastname: '{appUserRegister.LastName}'");

                throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<AppUserResponse>
                {
                    ResultCode = ResultCode.INVALID_NAME_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.INVALID_NAME_CODE),
                    Data = default
                });
            }

            //validate email
            if (!_validateDataService.IsValidEmail(appUserRegister.Email))
            {
                _logger.Error($"User enter invalid email: {appUserRegister.Email}");

                throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<AppUserResponse>
                {
                    ResultCode = ResultCode.INVALID_EMAIL_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.INVALID_EMAIL_CODE),
                    Data = default
                });
            }

            //validate birthday
            if (!_validateDataService.IsValidForetime(appUserRegister.Birthday))
            {
                _logger.Error($"User enter invalid birthday: {appUserRegister.Birthday}");

                throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<AppUserResponse>
                {
                    ResultCode = ResultCode.INVALID_FORETIME_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.INVALID_FORETIME_CODE),
                    Data = default
                });
            }

            //validate phone
            if (!_validateDataService.IsValidPhone(appUserRegister.NumberPhone))
            {
                _logger.Error($"User enter wrong phone format: {appUserRegister.NumberPhone}");

                throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<AppUserResponse>
                {
                    ResultCode = ResultCode.INVALID_PHONE_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.INVALID_PHONE_CODE),
                    Data = default
                });
            }

            appUserRegister.NumberPhone = _utilService.changeToVietnamPhoneNumber(appUserRegister.NumberPhone);

            AppUser appUserInfo = _mapper.Map<AppUser>(appUserRegister);

            try
            {
                appUserInfo.Id = "User_" + ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds().ToString();
                appUserInfo.IsActive = false;
                appUserInfo.CreatedAt = DateTime.Now;
                appUserInfo.UpdatedAt = DateTime.Now;
                appUserInfo.IsBlock = false;
                appUserInfo.RoleId = ROLE_USER;
                appUserInfo.NumberPhone = _utilService.changeToVietnamPhoneNumber(appUserInfo.NumberPhone);

                _unitOfWork.GetRepository<AppUser>().Add(appUserInfo);

                _unitOfWork.Commit();
            }
            catch (Exception e)
            {
                _logger.Error(e.Message);

                throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<AppUserResponse>
                {
                    ResultCode = ResultCode.SQL_ERROR_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.SQL_ERROR_CODE),
                    Data = default
                });
            }

            AppUserResponse userRegisterResponse = _mapper.Map<AppUserResponse>(appUserInfo);

            return new BaseResponse<AppUserResponse>
            {
                ResultCode = ResultCode.SUCCESS_CODE,
                ResultMessage = ResultCode.GetMessage(ResultCode.SUCCESS_CODE),
                Data = userRegisterResponse
            };
        }

        //Login
        public BaseResponse<AppUserLoginResponse> Login(AppUserLoginRequest appUserLogin)
        {
            appUserLogin.NumberPhone = _utilService.changeToVietnamPhoneNumber(appUserLogin.NumberPhone);

            AppUser user;

            try
            {
                user = _unitOfWork.GetRepository<AppUser>()
                .GetAll()
                .FirstOrDefault(u =>
                u.NumberPhone.Equals(appUserLogin.NumberPhone));
            }
            catch (Exception e)
            {
                _logger.Error(e.Message);

                throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<AppUserLoginResponse>
                {
                    ResultCode = ResultCode.SQL_ERROR_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.SQL_ERROR_CODE),
                    Data = default
                });
            }

            //check OTP and get token
            string token = _jwtAuthenticationManager.Authenticate(user, appUserLogin.OTP, _distributedCache);

            //wrong OTP inputted
            if (token == null)
            {
                _logger.Warning("User " + user.Id + " input wrong OTP");

                throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<AppUserLoginResponse>
                {
                    ResultCode = ResultCode.WRONG_OTP_INPUTTED_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.WRONG_OTP_INPUTTED_CODE),
                    Data = default
                });
            }
            //match OTP and store token
            _persistentLoginService.StoreUserToken(token, user.Id);

            //if User is inactive
            if (!user.IsActive)
            {
                user.IsActive = true;
            }

            //commit unit of work
            try
            {
                _unitOfWork.GetRepository<AppUser>().Update(user);

                _unitOfWork.Commit();
            }
            catch (Exception e)
            {
                _logger.Error(e.Message);

                throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<AppUserLoginResponse>
                {
                    ResultCode = ResultCode.SQL_ERROR_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.SQL_ERROR_CODE),
                    Data = default
                });
            }

            AppUserLoginResponse userResponse = _mapper.Map<AppUserLoginResponse>(user);
            userResponse.Token = token;

            return new BaseResponse<AppUserLoginResponse>
            {
                ResultCode = ResultCode.SUCCESS_CODE,
                ResultMessage = ResultCode.GetMessage(ResultCode.SUCCESS_CODE),
                Data = userResponse
            }; ;
        }
    }
}
