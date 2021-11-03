using BLL.Dto;
using BLL.Dto.DonationSession;
using BLL.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using System.Diagnostics;

namespace API.Controllers
{
    [EnableCors("MyPolicy")]
    [Authorize]
    [Route("core/api/donation")]
    [ApiController]
    public class DonationSessionController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IDonationSessionService _donationSessionService;

        public DonationSessionController(ILogger logger,
            IDonationSessionService donationSessionService)
        {
            _logger = logger;
            _donationSessionService = donationSessionService;
        }

        [HttpPost("create")]
        public IActionResult CreateDonation([FromForm] DonationSessionRequest donationSessionRequest)
        {
            _logger.Information($"POST core/api/donation/create START Request: "
                + JsonConvert.SerializeObject(donationSessionRequest));

            Stopwatch watch = new Stopwatch();
            watch.Start();

            //create donation
            BaseResponse<DonationSessionResponse> donationSessionResponse = _donationSessionService
                .CreateDonationSession(
                Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", ""),
                donationSessionRequest);

            string json = JsonConvert.SerializeObject(donationSessionResponse);

            watch.Stop();

            _logger.Information($"POST core/api/donation/create END duration: {watch.ElapsedMilliseconds} ms -----------Response: {json}");

            return Ok(json);
        }

        [AllowAnonymous]
        [HttpGet("{id}", Name = "GetDonationById")]
        public IActionResult GetDonationById(string id)
        {


            _logger.Information($"GET core/api/donation/{id} START Request: {id}");

            Stopwatch watch = new Stopwatch();
            watch.Start();

            string json = JsonConvert.SerializeObject(_donationSessionService.GetDonationSessionById(id));

            watch.Stop();

            _logger.Information($"GET core/api/donation/{id} END duration: " +
                $"{watch.ElapsedMilliseconds} ms -----------Response: " + json);

            return Ok(json);
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult GetAllDonation()
        {


            _logger.Information($"GET core/api/donation/ START Request:");

            Stopwatch watch = new Stopwatch();
            watch.Start();

            string json = JsonConvert.SerializeObject(_donationSessionService.GetDonationSession());

            watch.Stop();

            _logger.Information($"GET core/api/donation/ END duration: " +
                $"{watch.ElapsedMilliseconds} ms -----------Response: " + json);

            return Ok(json);
        }

        [AllowAnonymous]
        [HttpGet("project/{id}", Name = "GetDonationByPro")]
        public IActionResult GetDonationByPro(string id)
        {


            _logger.Information($"GET core/api/donation/project/{id} START Request: "
                + id);

            Stopwatch watch = new Stopwatch();
            watch.Start();

            string json = JsonConvert.SerializeObject(_donationSessionService.GetDonationSessionByProjectId(id));

            watch.Stop();

            _logger.Information($"GET core/api/donation/project/{id} END duration: " +
                $"{watch.ElapsedMilliseconds} ms -----------Response: " + json);

            return Ok(json);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateDonation(string id, [FromForm] DonationSessionRequest donationSessionRequest)
        {


            _logger.Information($"PUT core/api/donation/{id} START Request: "
                + JsonConvert.SerializeObject(donationSessionRequest));

            Stopwatch watch = new Stopwatch();
            watch.Start();

            //update donation
            BaseResponse<DonationSessionResponse> donationSessionResponse = _donationSessionService
                .UpdateDonationSession(id,
                Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", ""),
                donationSessionRequest);

            string json = JsonConvert.SerializeObject(donationSessionResponse);

            watch.Stop();

            _logger.Information($"PUT core/api/donation/{id} END duration: " +
                $"{watch.ElapsedMilliseconds} ms -----------Response: " + json);

            return Ok(json);
        }

        [HttpPut("delete/{id}")]
        public IActionResult DeleteDonation(string id)
        {


            _logger.Information($"PUT core/api/donation/delete/{id} START Request: " + id);

            Stopwatch watch = new Stopwatch();
            watch.Start();

            //delete donation
            BaseResponse<DonationSessionResponse> donationSessionResponse = _donationSessionService
                .DeleteDonationSession(id,
                Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", ""));

            string json = JsonConvert.SerializeObject(donationSessionResponse);

            watch.Stop();

            _logger.Information($"PUT core/api/donation/delete/{id} END duration: " +
                $"{watch.ElapsedMilliseconds} ms -----------Response: " + json);

            return Ok(json);
        }
    }
}
