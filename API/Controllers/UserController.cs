using BLL.Dto;
using BLL.Dto.User;
using BLL.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;

namespace API.Controllers
{
    [EnableCors("MyPolicy")]
    [Authorize]
    [ApiController]
    [Route("core/api/user")]
    public class UserController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IAppUserService _appUserService;

        public UserController(ILogger logger, IAppUserService appUserService)
        {
            _logger = logger;
            _appUserService = appUserService;
        }

        [AllowAnonymous]
        [HttpPost("verify")]
        public IActionResult Verify([FromForm] string numberPhone)
        {


            _logger.Information($"POST core/api/user/verify START Request: {numberPhone}");

            Stopwatch watch = new Stopwatch();
            watch.Start();

            BaseResponse<AppUserResponse> response = _appUserService.VerifyPhone(numberPhone);

            string json = JsonConvert.SerializeObject(response);

            watch.Stop();

            _logger.Information("POST core/api/user/verify END duration: " +
                $"{watch.ElapsedMilliseconds} ms -----------Response: " + json);

            return Ok(json);
        }


        [AllowAnonymous]
        [HttpPost("login")]
        public IActionResult Login([FromForm] AppUserLoginRequest appUserLogin)
        {


            _logger.Information($"POST core/api/user/login START Request: "
                + JsonConvert.SerializeObject(appUserLogin));

            Stopwatch watch = new Stopwatch();
            watch.Start();

            BaseResponse<AppUserLoginResponse> appUserLoginResponse = _appUserService.Login(appUserLogin);

            string json = JsonConvert.SerializeObject(appUserLoginResponse);

            watch.Stop();

            _logger.Information("POST core/api/user/login END duration: " +
                $"{watch.ElapsedMilliseconds} ms -----------Response: " + json);

            return Ok(json);
        }


        [AllowAnonymous]
        [HttpPost("register")]
        public IActionResult Register([FromForm] AppUserRegisterRequest appUserRegister)
        {


            _logger.Information($"POST core/api/user/register START Request: "
                + JsonConvert.SerializeObject(appUserRegister));

            Stopwatch watch = new Stopwatch();
            watch.Start();

            BaseResponse<AppUserResponse> newUser = _appUserService.Register(appUserRegister);

            string json = JsonConvert.SerializeObject(newUser);

            watch.Stop();

            _logger.Information("POST core/api/user/register END duration: " +
                $"{watch.ElapsedMilliseconds} ms -----------Response: " + json);

            return Ok(json);
        }
    }
}
