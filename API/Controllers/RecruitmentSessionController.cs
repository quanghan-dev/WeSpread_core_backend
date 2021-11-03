using BLL.Dto;
using BLL.Dto.RecruitmentSession;
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
    [Route("core/api/recruitment")]
    [ApiController]
    public class RecruitmentSessionController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IRecruitmentSessionService _recruitmentSessionService;

        public RecruitmentSessionController(ILogger logger,
            IRecruitmentSessionService recruitmentSessionService)
        {
            _logger = logger;
            _recruitmentSessionService = recruitmentSessionService;
        }

        [HttpPost("create")]
        public IActionResult CreateRecruitment([FromForm] RecruitmentSessionRequest recruitmentSessionRequest)
        {


            _logger.Information($"POST core/api/recruitment/create START Request: "
                + JsonConvert.SerializeObject(recruitmentSessionRequest));

            Stopwatch watch = new Stopwatch();
            watch.Start();

            //create recruitment
            BaseResponse<RecruitmentSessionResponse> recruitmentSessionResponse = _recruitmentSessionService
                .CreateRecruitmentSession(
                Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", ""),
                recruitmentSessionRequest);

            string json = JsonConvert.SerializeObject(recruitmentSessionResponse);

            watch.Stop();

            _logger.Information("POST core/api/recruitment/create END duration: " +
                $"{watch.ElapsedMilliseconds} ms -----------Response: " + json);

            return Ok(json);
        }

        [AllowAnonymous]
        [HttpGet("{id}", Name = "GetRecruitmentById")]
        public IActionResult GetRecruitmentById(string id)
        {


            _logger.Information($"GET core/api/recruitment/{id} START Request: "
                + id);

            Stopwatch watch = new Stopwatch();
            watch.Start();

            string json = JsonConvert.SerializeObject(_recruitmentSessionService.GetRecruitmentSessionById(id));

            watch.Stop();

            _logger.Information($"GET core/api/recruitment/{id} END duration: " +
                $"{watch.ElapsedMilliseconds} ms -----------Response: " + json);

            return Ok(json);
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult GetAllRecruitment()
        {


            _logger.Information($"GET core/api/recruitment START Request: ");

            Stopwatch watch = new Stopwatch();
            watch.Start();

            string json = JsonConvert.SerializeObject(_recruitmentSessionService.GetAllRecruitmentSession());

            watch.Stop();

            _logger.Information($"GET core/api/recruitment END duration: " +
                $"{watch.ElapsedMilliseconds} ms -----------Response: " + json);

            return Ok(json);
        }

        [AllowAnonymous]
        [HttpGet("project/{id}", Name = "GetRecruitmentByPro")]
        public IActionResult GetRecruitmentByPro(string id)
        {


            _logger.Information($"GET core/api/recruitment/project/{id} START Request: "
                + id);

            Stopwatch watch = new Stopwatch();
            watch.Start();

            string json = JsonConvert.SerializeObject(_recruitmentSessionService.GetRecruitmentSessionByProjectId(id));

            watch.Stop();

            _logger.Information($"GET core/api/recruitment/project/{id} END duration: " +
                $"{watch.ElapsedMilliseconds} ms -----------Response: " + json);

            return Ok(json);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateRecruitment(string id, [FromForm] RecruitmentSessionRequest recruitmentSessionRequest)
        {


            _logger.Information($"PUT core/api/recruitment/{id} START Request: "
                + JsonConvert.SerializeObject(recruitmentSessionRequest));

            Stopwatch watch = new Stopwatch();
            watch.Start();

            //update recruitment
            BaseResponse<RecruitmentSessionResponse> donationSessionResponse = _recruitmentSessionService
                .UpdateRecruitmentSession(
                Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", ""), id,
                recruitmentSessionRequest);

            string json = JsonConvert.SerializeObject(donationSessionResponse);

            watch.Stop();

            _logger.Information($"PUT core/api/recruitment/{id} END duration: " +
                $"{watch.ElapsedMilliseconds} ms -----------Response: " + json);

            return Ok(json);
        }

        [HttpPut("delete/{id}")]
        public IActionResult DeleteRecruitment(string id)
        {


            _logger.Information($"PUT core/api/recruitment/delete/{id} START Request: "
                + $"id: {id}");

            Stopwatch watch = new Stopwatch();
            watch.Start();

            //delete recruitment
            BaseResponse< RecruitmentSessionResponse> donationSessionResponse = _recruitmentSessionService
                .DeleteRecruitmentSession(
                Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", ""), id);

            string json = JsonConvert.SerializeObject(donationSessionResponse);

            watch.Stop();

            _logger.Information($"PUT core/api/recruitment/delete/{id} END duration: " +
                $"{watch.ElapsedMilliseconds} ms -----------Response: " + json);

            return Ok(json);
        }
    }
}
