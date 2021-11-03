using BLL.Dto;
using BLL.Dto.Donate;
using BLL.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using System.Collections.Generic;

namespace API.Controllers
{
    [EnableCors("MyPolicy")]
    [Authorize]
    [Route("core/api/donate")]
    [ApiController]
    public class DonateController : ControllerBase
    {
        private readonly IDonateService _donateService;
        private readonly ILogger _logger;

        public DonateController(IDonateService donateService, ILogger logger)
        {
            _donateService = donateService;
            _logger = logger;
        }

        [HttpPost("create")]
        public IActionResult CreateDonateRequest([FromForm] DonateRequest donateRequest)
        {
            _logger.Information($"POST core/api/donate/create START Request: "
                + JsonConvert.SerializeObject(donateRequest));

            Stopwatch watch = new Stopwatch();
            watch.Start();

            //create donation
            BaseResponse<DonateLinkResponse> donateLinkResponse = _donateService
                .CreateLinkDonate(donateRequest,
                Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", ""));

            string json = JsonConvert.SerializeObject(donateLinkResponse);

            watch.Stop();

            _logger.Information($"POST core/api/donate/create END duration: {watch.ElapsedMilliseconds} ms -----------Response: {json}");

            return Ok(json);
        }

        [HttpGet("history")]
        public IActionResult GetHistory()
        {
            _logger.Information($"GET core/api/donate/history START Request: ");

            Stopwatch watch = new Stopwatch();
            watch.Start();

            //create donation
            BaseResponse<IEnumerable<DonateResultResponse>> donateResponse = _donateService
                .HistoryDonate(Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", ""));

            string json = JsonConvert.SerializeObject(donateResponse);

            watch.Stop();

            _logger.Information($"GET core/api/donate/history END duration: {watch.ElapsedMilliseconds} ms -----------Response: {json}");

            return Ok(json);
        }

    }
}
