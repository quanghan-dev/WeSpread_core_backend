using BLL.Dto;
using BLL.Dto.Project;
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
    [Route("core/api/project")]
    public class ProjectController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IProjectService _projectService;

        public ProjectController(ILogger logger,
            IProjectService projectService)
        {
            _logger = logger;
            _projectService = projectService;
        }

        [HttpPost("create")]
        public IActionResult CreateProject([FromForm] ProjectRequest projectRequest,
            [FromForm] IFormFile logo, [FromForm] List<IFormFile> covers)
        {


            _logger.Information($"POST core/api/project/create START Request: {JsonConvert.SerializeObject(projectRequest)}, {JsonConvert.SerializeObject(logo)}, {JsonConvert.SerializeObject(covers)}");

            Stopwatch watch = new Stopwatch();
            watch.Start();

            //create project
            BaseResponse<ProjectResponse> projectResponse = _projectService
                .CreateProject(projectRequest,
                Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", ""),
                logo, covers);

            string json = JsonConvert.SerializeObject(projectResponse);

            watch.Stop();

            _logger.Information("POST core/api/project/create END duration: " +
                $"{watch.ElapsedMilliseconds} ms -----------Response: " + json);

            return Ok(json);
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult GetAllProjects()
        {
            _logger.Information($"GET core/api/project/ START Request: ");

            Stopwatch watch = new Stopwatch();
            watch.Start();

            string json = JsonConvert.SerializeObject(_projectService.GetAllProject());

            watch.Stop();

            _logger.Information("GET core/api/project END duration: " +
                $"{watch.ElapsedMilliseconds} ms -----------Response: " + json);

            return Ok(json);
        }

        [AllowAnonymous]
        [HttpGet("{id}", Name = "GetProjectById")]
        public IActionResult GetProjectById(string id)
        {


            _logger.Information($"GET core/api/project/{id} START Request: "
                + id);

            Stopwatch watch = new Stopwatch();
            watch.Start();

            string json = JsonConvert.SerializeObject(_projectService.GetProjectById(id));

            watch.Stop();

            _logger.Information($"GET core/api/project/{id} END duration: " +
                $"{watch.ElapsedMilliseconds} ms -----------Response: " + json);

            return Ok(json);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateProject(string id, [FromForm] ProjectRequest projectRequest,
            [FromForm] IFormFile logo, [FromForm] List<IFormFile> covers)
        {


            _logger.Information($"PUT core/api/project/{id} START Request: "
                + "id: " + JsonConvert.SerializeObject(id) + "\n"
                + JsonConvert.SerializeObject(projectRequest) + "\n"
                + JsonConvert.SerializeObject(logo) + "\n"
                + JsonConvert.SerializeObject(covers));

            Stopwatch watch = new Stopwatch();
            watch.Start();

            //update project
            BaseResponse<ProjectResponse> projectResponse = _projectService
                .UpdateProject(id, projectRequest,
                Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", ""),
                logo, covers);

            string json = JsonConvert.SerializeObject(projectResponse);

            watch.Stop();

            _logger.Information($"PUT core/api/project/{id} END duration: " +
                $"{watch.ElapsedMilliseconds} ms -----------Response: " + json);

            return Ok(json);
        }

        [HttpPut("delete/{id}")]
        public IActionResult DeleteProject(string id)
        {


            _logger.Information($"PUT core/api/project/delete/{id} START Request: " +
                $"id: {id}");

            Stopwatch watch = new Stopwatch();
            watch.Start();

            //delete project
            BaseResponse<ProjectResponse> projectResponse = _projectService
                .DeleteProject(id,
                Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", ""));

            string json = JsonConvert.SerializeObject(projectResponse);

            watch.Stop();

            _logger.Information($"PUT core/api/project/delete/{id} END duration: " +
                $"{watch.ElapsedMilliseconds} ms -----------Response: " + json);

            return Ok(json);
        }

        [AllowAnonymous]
        [HttpGet("organization/{id}", Name = "GetProjectByOrg")]
        public IActionResult GetProjectByOrg(string id)
        {


            _logger.Information($"GET core/api/project/organization/{id} START Request: "
                + id);

            Stopwatch watch = new Stopwatch();
            watch.Start();

            string json = JsonConvert.SerializeObject(_projectService.GetProjectByOrg(id));

            watch.Stop();

            _logger.Information($"GET core/api/project/organization/{id} END duration: " +
                $"{watch.ElapsedMilliseconds} ms -----------Response: " + json);

            return Ok(json);
        }

        [AllowAnonymous]
        [HttpGet("category/{id}", Name = "GetProjectByCate")]
        public IActionResult GetProjectByCate(string id)
        {


            _logger.Information($"GET core/api/project/category/{id} START Request: "
                + id);

            Stopwatch watch = new Stopwatch();
            watch.Start();

            string json = JsonConvert.SerializeObject(_projectService.GetProjectByCategory(id));

            watch.Stop();

            _logger.Information($"GET core/api/project/category/{id} END duration: " +
                $"{watch.ElapsedMilliseconds} ms -----------Response: " + json);

            return Ok(json);
        }

        [AllowAnonymous]
        [HttpGet("location/{id}", Name = "GetProjectByLocation")]
        public IActionResult GetProjectByCity(string id)
        {


            _logger.Information($"GET core/api/project/location/{id} START Request: "
                + id);

            Stopwatch watch = new Stopwatch();
            watch.Start();

            string json = JsonConvert.SerializeObject(_projectService.GetProjectByLocation(id));

            watch.Stop();

            _logger.Information($"GET core/api/project/location/{id} END duration: " +
                $"{watch.ElapsedMilliseconds} ms -----------Response: " + json);

            return Ok(json);
        }
    }
}
