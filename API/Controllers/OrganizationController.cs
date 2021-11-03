using BLL.Dto;
using BLL.Dto.Organization;
using BLL.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;

namespace API.Controllers
{
    [EnableCors("MyPolicy")]
    [Authorize]
    [ApiController]
    [Route("core/api/organization")]
    public class OrganizationController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IOrganizationService _organizationService;

        public OrganizationController(ILogger logger,
            IOrganizationService organizationService)
        {
            _logger = logger;
            _organizationService = organizationService;
        }

        [HttpPost("create")]
        public IActionResult CreateOrg([FromForm] OrganizationRequest organizationCreateRequest,
            [FromForm] IFormFile logo, [FromForm] List<IFormFile> covers)
        {


            _logger.Information($"POST core/api/organization/create START Request: "
                + JsonConvert.SerializeObject(organizationCreateRequest) + "\n"
                + JsonConvert.SerializeObject(logo) + "\n"
                + JsonConvert.SerializeObject(covers));

            Stopwatch watch = new Stopwatch();
            watch.Start();

            //create org
            BaseResponse<OrganizationResponse> organizationCreateResponse = _organizationService
                .Create(organizationCreateRequest,
                Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", ""),
                logo, covers);

            string json = JsonConvert.SerializeObject(organizationCreateResponse);

            watch.Stop();

            _logger.Information("POST core/api/organization/create END duration: " +
                $"{watch.ElapsedMilliseconds} ms -----------Response: " + json);

            return Ok(json);
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult GetAllOrganizations()
        {
            _logger.Information($"GET core/api/organization START Request: ");

            Stopwatch watch = new Stopwatch();
            watch.Start();

            string json = JsonConvert.SerializeObject(_organizationService.GetAll());

            watch.Stop();

            _logger.Information("GET core/api/organization END duration: " +
                $"{watch.ElapsedMilliseconds} ms -----------Response: " + json);

            return Ok(json);
        }

        [AllowAnonymous]
        [HttpGet("{id}", Name = "GetOrganizationById")]
        public IActionResult GetOrganizationById(string id)
        {


            _logger.Information($"GET core/api/organization/{id} START Request: "
                + id);

            Stopwatch watch = new Stopwatch();
            watch.Start();

            string json = JsonConvert.SerializeObject(_organizationService.GetById(id));

            watch.Stop();

            _logger.Information($"GET core/api/organization/{id} END duration: " +
                $"{watch.ElapsedMilliseconds} ms -----------Response: " + json);

            return Ok(json);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateOrg(string id, [FromForm] OrganizationRequest organizationUpdateRequest,
            [FromForm] IFormFile logo, [FromForm] List<IFormFile> covers)
        {


            _logger.Information($"PUT core/api/organization/{id} START Request: "
                + "id: " + JsonConvert.SerializeObject(id) + "\n"
                + JsonConvert.SerializeObject(organizationUpdateRequest) + "\n"
                + JsonConvert.SerializeObject(logo) + "\n"
                + JsonConvert.SerializeObject(covers));

            Stopwatch watch = new Stopwatch();
            watch.Start();

            //update org
            BaseResponse<OrganizationResponse> organizationUpdateResponse = _organizationService
                .UpdateById(id, organizationUpdateRequest,
                Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", ""),
                logo, covers);

            string json = JsonConvert.SerializeObject(organizationUpdateResponse);

            watch.Stop();

            _logger.Information($"PUT core/api/organization/{id} END duration: " +
                $"{watch.ElapsedMilliseconds} ms -----------Response: " + json);

            return Ok(json);
        }

        [HttpPut("delete/{id}")]
        public IActionResult DeleteOrg(string id)
        {


            _logger.Information($"PUT core/api/organization/delete/{id} START Request: "
                + $"id: {id}");

            Stopwatch watch = new Stopwatch();
            watch.Start();

            //delete org
            BaseResponse<OrganizationResponse> organizationResponse = _organizationService
                .DeleteById(id, Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", ""));

            string json = JsonConvert.SerializeObject(organizationResponse);

            watch.Stop();

            _logger.Information($"PUT core/api/organization/delete/{id} END duration: " +
                $"{watch.ElapsedMilliseconds} ms -----------Response: " + json);

            return Ok(json);
        }

        [AllowAnonymous]
        [HttpGet("category/{id}", Name = "GetOrgByCategory")]
        public IActionResult GetOrgByCategory(string id)
        {


            _logger.Information($"GET core/api/organization/category/{id} START Request: "
                + id);

            Stopwatch watch = new Stopwatch();
            watch.Start();

            string json = JsonConvert.SerializeObject(_organizationService.GetByCategory(id));

            watch.Stop();

            _logger.Information($"GET core/api/organization/category/{id} END duration: " +
                $"{watch.ElapsedMilliseconds} ms -----------Response: " + json);

            return Ok(json);
        }

        [AllowAnonymous]
        [HttpGet("location/{id}", Name = "GetOrgByLocation")]
        public IActionResult GetOrgByLocation(string id)
        {


            _logger.Information($"GET core/api/organization/location/{id} START Request: "
                + id);

            Stopwatch watch = new Stopwatch();
            watch.Start();

            string json = JsonConvert.SerializeObject(_organizationService.GetByLocation(id));

            watch.Stop();

            _logger.Information($"GET core/api/organization/location/{id} END duration: " +
                $"{watch.ElapsedMilliseconds} ms -----------Response: " + json);

            return Ok(json);
        }
    }
}
