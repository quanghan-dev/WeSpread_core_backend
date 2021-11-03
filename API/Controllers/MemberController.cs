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
    [Route("core/api/member")]
    [ApiController]
    public class MemberController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IDonateService _donateService;
        private readonly IRegistrationFormService _registrationFormService;

        public MemberController(ILogger logger, IDonateService donateService,
            IRegistrationFormService registrationFormService)
        {
            _logger = logger;
            _donateService = donateService;
            _registrationFormService = registrationFormService;
        }

        [AllowAnonymous]
        [HttpGet("donate/{id}")]
        public IActionResult GetMemberDonateOfSession(string id)
        {
            _logger.Information($"GET core/api/member/donate/{id} START Request: ");

            Stopwatch watch = new Stopwatch();
            watch.Start();

            string json = JsonConvert.SerializeObject(_donateService.GetMemberDonateBySession(id));

            watch.Stop();

            _logger.Information($"GET core/api/member/donate/{id} END duration: " +
                $"{watch.ElapsedMilliseconds} ms -----------Response: " + json);

            return Ok(json);
        }

        [AllowAnonymous]
        [HttpGet("apply/{id}")]
        public IActionResult GetMemberApplyOfSession(string id)
        {
            _logger.Information($"GET core/api/member/apply/{id} START Request: ");

            Stopwatch watch = new Stopwatch();
            watch.Start();

            string json = JsonConvert.SerializeObject(_registrationFormService.GetMemberApplyBySession(id));

            watch.Stop();

            _logger.Information($"GET core/api/member/apply/{id} END duration: " +
                $"{watch.ElapsedMilliseconds} ms -----------Response: " + json);

            return Ok(json);
        }
    }
}
