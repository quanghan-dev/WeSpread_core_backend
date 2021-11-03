using BLL.Dto;
using BLL.Dto.RegistrationForm;
using BLL.Service;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using System.Diagnostics;

namespace API.Controllers
{
    [EnableCors("MyPolicy")]
    [Route("core/api/registrationform")]
    [ApiController]
    public class RegistrationFormController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IRegistrationFormService _registrationFormService;

        public RegistrationFormController(ILogger logger, IRegistrationFormService registrationFormService)
        {
            _logger = logger;
            _registrationFormService = registrationFormService;
        }

        [HttpPost("create")]
        public IActionResult CreateRegistrationForm(RegistrationFormRequest registrationFormRequest)
        {
            _logger.Information($"POST core/api/registrationform/create START Request: "
                + JsonConvert.SerializeObject(registrationFormRequest));

            Stopwatch watch = new Stopwatch();
            watch.Start();

            //create donation
            BaseResponse<RegistrationFormResponse> response = _registrationFormService
                .CreateRegistrationForm(registrationFormRequest,
                Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", ""));

            string json = JsonConvert.SerializeObject(response);

            watch.Stop();

            _logger.Information($"POST core/api/registrationform/create END duration: {watch.ElapsedMilliseconds} ms -----------Response: {json}");

            return Ok(json);
        }

        [HttpGet]
        public IActionResult GetRegistrationFormByUser()
        {
            _logger.Information($"GET core/api/registrationform/ START Request: ");

            Stopwatch watch = new Stopwatch();
            watch.Start();

            string json = JsonConvert.SerializeObject(_registrationFormService.GetRegistrationFormByUser(
                Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", "")));

            watch.Stop();

            _logger.Information("GET core/api/registrationform END duration: " +
                $"{watch.ElapsedMilliseconds} ms -----------Response: " + json);

            return Ok(json);
        }

        [HttpPut("cancel/{id}")]
        public IActionResult CancelRegistrationForm(string id)
        {
            _logger.Information($"PUT core/api/registrationform/cancel/{id} START Request: "
                + $"id: {id}");

            Stopwatch watch = new Stopwatch();
            watch.Start();

            //delete org
            BaseResponse<RegistrationFormResponse> response = _registrationFormService
                .CancelRegistrationForm(id, Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", ""));

            string json = JsonConvert.SerializeObject(response);

            watch.Stop();

            _logger.Information($"PUT core/api/registrationform/cancel/{id} END duration: " +
                $"{watch.ElapsedMilliseconds} ms -----------Response: " + json);

            return Ok(json);
        }

        [HttpGet("session/{id}")]
        public IActionResult GetRegistrationFormBySessionIdAndOrgAdmin(string id)
        {
            _logger.Information($"GET core/api/registrationform/orgadmin START Request: ");

            Stopwatch watch = new Stopwatch();
            watch.Start();

            string json = JsonConvert.SerializeObject(_registrationFormService.GetRegistrationFormBySessionIdAndOrgAdmin(
                id, Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", "")));

            watch.Stop();

            _logger.Information("GET core/api/registrationform/orgadmin END duration: " +
                $"{watch.ElapsedMilliseconds} ms -----------Response: " + json);

            return Ok(json);
        }

        [HttpPut("approve/{id}")]
        public IActionResult ApproveForm(string id)
        {
            _logger.Information($"PUT core/api/registrationform/approve/{id} START Request: "
                + $"id: {id}");

            Stopwatch watch = new Stopwatch();
            watch.Start();

            //delete org
            BaseResponse<RegistrationFormResponse> response = _registrationFormService
                .ApproveForm(id, Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", ""));

            string json = JsonConvert.SerializeObject(response);

            watch.Stop();

            _logger.Information($"PUT core/api/registrationform/approve/{id} END duration: " +
                $"{watch.ElapsedMilliseconds} ms -----------Response: " + json);

            return Ok(json);
        }

        [HttpPut("reject/{id}")]
        public IActionResult RejectForm(string id)
        {
            _logger.Information($"PUT core/api/registrationform/reject/{id} START Request: "
                + $"id: {id}");

            Stopwatch watch = new Stopwatch();
            watch.Start();

            //delete org
            BaseResponse<RegistrationFormResponse> response = _registrationFormService
                .RejectForm(id, Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", ""));

            string json = JsonConvert.SerializeObject(response);

            watch.Stop();

            _logger.Information($"PUT core/api/registrationform/reject/{id} END duration: " +
                $"{watch.ElapsedMilliseconds} ms -----------Response: " + json);

            return Ok(json);
        }
    }
}
