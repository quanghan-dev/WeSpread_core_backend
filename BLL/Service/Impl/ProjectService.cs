using AutoMapper;
using BLL.Constant;
using BLL.Dto;
using BLL.Dto.Exception;
using BLL.Dto.Location;
using BLL.Dto.Organization;
using BLL.Dto.Project;
using DAL.Model;
using DAL.UnifOfWork;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace BLL.Service.Impl
{
    public class ProjectService : IProjectService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly IDistributedCache _distributedCache;
        private readonly IPersistentLoginService _persistentLoginService;
        private readonly IUploadFirebaseService _uploadFirebaseService;
        private readonly IValidateDataService _validateDataService;
        private readonly IUtilService _utilService;
        private readonly IOrganizationService _organizationService;
        private readonly IRedisService _redisService;
        private readonly ILocationService _locationService;
        private const string TYPE = "Project";
        private const string ORG_ADMIN = "ORG_ADMIN";
        private const string PREFIX = "Pro_";
        private const string PATTERN = "Pro_*";

        public ProjectService(IUnitOfWork unitOfWork, ILogger logger, IMapper mapper,
            IDistributedCache distributedCache, IPersistentLoginService persistentLoginService,
            IUploadFirebaseService uploadFirebaseService, IValidateDataService validateDataService,
            IUtilService utilService, IOrganizationService organizationService, IRedisService redisService,
            ILocationService locationService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _mapper = mapper;
            _distributedCache = distributedCache;
            _persistentLoginService = persistentLoginService;
            _uploadFirebaseService = uploadFirebaseService;
            _validateDataService = validateDataService;
            _utilService = utilService;
            _organizationService = organizationService;
            _redisService = redisService;
            _locationService = locationService;
        }


        public BaseResponse<ProjectResponse> CreateProject(ProjectRequest projectRequest, string token, IFormFile logo, List<IFormFile> cover)
        {
            //validate token
            string userId = _persistentLoginService.GetUserIDByToken(token);
            if (String.IsNullOrEmpty(userId))
            {
                _logger.Error($"User with token {token} is unauthorized.");

                throw new HttpStatusException(HttpStatusCode.Unauthorized, new BaseResponse<ProjectResponse>
                {
                    ResultCode = ResultCode.USER_UNAUTHORIZED_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.USER_UNAUTHORIZED_CODE),
                    Data = default
                });
            }

            //validate role
            OrganizationResponse org = _organizationService.GetByIdAndUser(userId, projectRequest.OrgId, ORG_ADMIN);

            if (org == null)
            {
                _logger.Error($"User {userId} does not have permission to create project for Org {projectRequest.OrgId}");

                throw new HttpStatusException(HttpStatusCode.Unauthorized, new BaseResponse<ProjectResponse>
                {
                    ResultCode = ResultCode.USER_UNAUTHORIZED_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.USER_UNAUTHORIZED_CODE),
                    Data = default
                });
            }

            //validate status org
            if (org.CreationCode != ResultCode.SUCCESS_CODE)
            {
                _logger.Error($"Org {org.Id} is unverified");

                throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<ProjectResponse>
                {
                    ResultCode = org.CreationCode,
                    ResultMessage = org.CreationMessage,
                    Data = default
                });
            }

            //validate name
            if (!_validateDataService.IsValidName(projectRequest.ProjectName, null))
            {
                _logger.Error($"{projectRequest.ProjectName} is invalid name.");

                throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<ProjectResponse>
                {
                    ResultCode = ResultCode.INVALID_NAME_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.INVALID_NAME_CODE),
                    Data = default
                });
            }

            if (projectRequest.EngName != null)
            {
                if (!_validateDataService.IsValidName(projectRequest.EngName, null))
                {
                    _logger.Error($"{projectRequest.EngName} is invalid name.");

                    throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<ProjectResponse>
                    {
                        ResultCode = ResultCode.INVALID_NAME_CODE,
                        ResultMessage = ResultCode.GetMessage(ResultCode.INVALID_NAME_CODE),
                        Data = default
                    });
                }
            }

            //compare date
            if (!_validateDataService.Is15DaysBefore(projectRequest.StartDate, projectRequest.EndDate))
            {
                _logger.Error($"Startdate {projectRequest.StartDate} is not 15 days before enddate {projectRequest.EndDate}");

                throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<ProjectResponse>
                {
                    ResultCode = ResultCode.INVALID_STARTTIME_ENDTIME_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.INVALID_STARTTIME_ENDTIME_CODE),
                    Data = default
                });
            }

            //upload file to Firebase Storage
            string coverUrl = _uploadFirebaseService
                .UploadFilesToFirebase(cover, TYPE, projectRequest.ProjectName, "cover").Result;

            string logoUrl = _uploadFirebaseService
                .UploadFileToFirebase(logo, TYPE, projectRequest.ProjectName, "logo").Result;

            if (coverUrl is null || logoUrl is null)
            {
                throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<ProjectResponse>
                {
                    ResultCode = ResultCode.UPLOAD_FILE_FIREBASE_ERROR_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.UPLOAD_FILE_FIREBASE_ERROR_CODE),
                    Data = default
                });
            }

            Project projectModel = _mapper.Map<Project>(projectRequest);
            ICollection<LocationResponse> location;
            try
            {
                projectModel.Id = PREFIX + ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds().ToString();
                projectModel.Name = projectRequest.ProjectName;
                projectModel.CreatedBy = userId;
                projectModel.CreatedAt = DateTime.Now;
                projectModel.UpdatedAt = DateTime.Now;
                projectModel.CreationCode = ResultCode.UNVERIFIED_CREATE_PROJECT_CODE;
                projectModel.CreationMessage = ResultCode.GetMessage(ResultCode.UNVERIFIED_CREATE_PROJECT_CODE);
                projectModel.Cover = coverUrl;
                projectModel.Logo = logoUrl;
                projectModel.IsActive = true;

                location = _locationService.CreateLocation(projectRequest.Location);
                CreateItemCategory(projectRequest.Category, projectModel.Id);
                CreateItemLocation(location, projectModel.Id);

                _unitOfWork.GetRepository<Project>().Add(projectModel);

                _unitOfWork.Commit();
            }
            catch (Exception e)
            {
                _logger.Error(e.Message);

                throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<ProjectResponse>
                {
                    ResultCode = ResultCode.SQL_ERROR_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.SQL_ERROR_CODE),
                    Data = default
                });
            }

            ProjectResponse projectResponse = _mapper.Map<ProjectResponse>(projectModel);
            projectResponse.Org = org;
            projectResponse.Location = location;

            //store project Info to Redis
            string cacheKey = projectResponse.Id;
            string cacheValue = _distributedCache.GetString(cacheKey);

            if (!string.IsNullOrEmpty(cacheValue))
            {
                _distributedCache.Remove(cacheKey);
            }

            _distributedCache.SetString(cacheKey, JsonConvert.SerializeObject(projectResponse).ToString());

            return new BaseResponse<ProjectResponse>
            {
                ResultCode = ResultCode.SUCCESS_CODE,
                ResultMessage = ResultCode.GetMessage(ResultCode.SUCCESS_CODE),
                Data = projectResponse
            };
        }

        public BaseResponse<IEnumerable<ProjectResponse>> GetAllProject()
        {
            List<ProjectResponse> projectResponses = new List<ProjectResponse>();

            IEnumerable<string> keys = _redisService.GetKeysByPattern(PATTERN);

            //get from redis
            //if (!_utilService.IsNullOrEmpty(keys))
            //    foreach (string key in keys)
            //    {
            //        ProjectResponse project = JsonConvert.DeserializeObject<ProjectResponse>(_distributedCache.GetString(key));

            //        projectResponses.Add(project);
            //    }
            //get from db
            if (_utilService.IsNullOrEmpty(projectResponses))
            {
                try
                {
                    using (var context = new WeSpreadCoreContext())
                    {

                        var projectJoinOrg =
                            from p in context.Projects
                            join o in context.Organizations
                            on p.OrgId equals o.Id
                            select new
                            {
                                ProId = p.Id,
                                ProName = p.Name,
                                ProEngName = p.EngName,
                                ProDescription = p.Description,
                                p.OrgId,
                                ProLogo = p.Logo,
                                ProCover = p.Cover,
                                ProCreatedBy = p.CreatedBy,
                                ProCreatedAt = p.CreatedAt,
                                ProUpdatedAt = p.UpdatedAt,
                                ProConfig = p.Config,
                                ProCreationCode = p.CreationCode,
                                ProCreationMessage = p.CreationMessage,
                                ProIsActive = p.IsActive,
                                p.StartDate,
                                p.EndDate,
                                o.Id,
                                o.Name,
                                o.EngName,
                                o.Logo,
                                o.Cover,
                                o.Type,
                                o.Description,
                                o.Mission,
                                o.Vision,
                                o.Achievement,
                                o.FoundingDate,
                                o.IsActive,
                                o.CreatedBy,
                                o.CreatedAt,
                                o.UpdatedAt,
                                o.Config,
                                o.CreationCode,
                                o.CreationMessage,
                                o.TaxCode
                            };

                        foreach (var pJO in projectJoinOrg)
                        {
                            ProjectResponse pro = new ProjectResponse
                            {
                                Id = pJO.ProId,
                                Name = pJO.ProName,
                                EngName = pJO.ProEngName,
                                Description = pJO.ProDescription,
                                OrgId = pJO.OrgId,
                                Logo = pJO.ProLogo,
                                Cover = pJO.ProCover,
                                CreatedAt = pJO.ProCreatedAt,
                                CreatedBy = pJO.ProCreatedBy,
                                UpdatedAt = pJO.ProUpdatedAt,
                                Config = pJO.ProConfig,
                                IsActive = pJO.ProIsActive,
                                CreationCode = pJO.ProCreationCode,
                                CreationMessage = pJO.ProCreationMessage,
                                StartDate = pJO.StartDate,
                                EndDate = pJO.EndDate,
                                Org = new OrganizationResponse
                                {
                                    Id = pJO.Id,
                                    Name = pJO.Name,
                                    EngName = pJO.EngName,
                                    Logo = pJO.Logo,
                                    Cover = pJO.Cover,
                                    Type = pJO.Type,
                                    Description = pJO.Description,
                                    Mission = pJO.Mission,
                                    Vision = pJO.Vision,
                                    Achievement = pJO.Achievement,
                                    IsActive = pJO.IsActive,
                                    FoundingDate = pJO.FoundingDate,
                                    Config = pJO.Config,
                                    CreationCode = pJO.CreationCode,
                                    CreationMessage = pJO.CreationMessage,
                                    TaxCode = pJO.TaxCode,
                                    Location = _locationService.GetLocationByOrgId(pJO.Id)
                                }
                            };
                            projectResponses.Add(pro);
                        };
                    }
                }
                catch (Exception e)
                {
                    _logger.Error(e.Message);

                    throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<ProjectResponse>
                    {
                        ResultCode = ResultCode.SQL_ERROR_CODE,
                        ResultMessage = ResultCode.GetMessage(ResultCode.SQL_ERROR_CODE),
                        Data = default
                    });
                }
            }

            foreach (ProjectResponse response in projectResponses)
            {
                response.Location = _locationService.GetLocationByProjectId(response.Id);
            }

            projectResponses.Sort((x, y) => x.CreatedAt.CompareTo(y.CreatedAt));

            return new BaseResponse<IEnumerable<ProjectResponse>>
            {
                ResultCode = ResultCode.SUCCESS_CODE,
                ResultMessage = ResultCode.GetMessage(ResultCode.SUCCESS_CODE),
                Data = projectResponses
            };
        }

        public BaseResponse<ProjectResponse> GetProjectById(string id)
        {
            ProjectResponse response = null;
            //get from redis
            //if (_distributedCache.GetString(id) != null)
            //    response = JsonConvert
            //        .DeserializeObject<ProjectResponse>(_distributedCache.GetString(id));

            if (response == null)
            {
                try
                {
                    using (var context = new WeSpreadCoreContext())
                    {

                        var projectJoinOrg =
                            from p in context.Projects
                            join o in context.Organizations
                            on p.OrgId equals o.Id
                            where p.Id == id
                            select new
                            {
                                ProId = p.Id,
                                ProName = p.Name,
                                ProEngName = p.EngName,
                                ProDescription = p.Description,
                                p.OrgId,
                                ProLogo = p.Logo,
                                ProCover = p.Cover,
                                ProCreatedBy = p.CreatedBy,
                                ProCreatedAt = p.CreatedAt,
                                ProUpdatedAt = p.UpdatedAt,
                                ProConfig = p.Config,
                                ProIsActive = p.IsActive,
                                ProCreationCode = p.CreationCode,
                                ProCreationMessage = p.CreationMessage,
                                p.StartDate,
                                p.EndDate,
                                o.Id,
                                o.Name,
                                o.EngName,
                                o.Logo,
                                o.Cover,
                                o.Type,
                                o.Description,
                                o.Mission,
                                o.Vision,
                                o.Achievement,
                                o.FoundingDate,
                                o.IsActive,
                                o.CreatedBy,
                                o.CreatedAt,
                                o.UpdatedAt,
                                o.Config,
                                o.CreationCode,
                                o.CreationMessage,
                                o.TaxCode
                            };

                        foreach (var pJO in projectJoinOrg)
                        {
                            response = new ProjectResponse
                            {
                                Id = pJO.ProId,
                                Name = pJO.ProName,
                                EngName = pJO.ProEngName,
                                Description = pJO.ProDescription,
                                OrgId = pJO.OrgId,
                                Logo = pJO.ProLogo,
                                Cover = pJO.ProCover,
                                CreatedAt = pJO.ProCreatedAt,
                                CreatedBy = pJO.ProCreatedBy,
                                UpdatedAt = pJO.ProUpdatedAt,
                                IsActive = pJO.ProIsActive,
                                Config = pJO.ProConfig,
                                CreationCode = pJO.ProCreationCode,
                                CreationMessage = pJO.ProCreationMessage,
                                StartDate = pJO.StartDate,
                                EndDate = pJO.EndDate,
                                Org = new OrganizationResponse
                                {
                                    Id = pJO.Id,
                                    Name = pJO.Name,
                                    EngName = pJO.EngName,
                                    Logo = pJO.Logo,
                                    Cover = pJO.Cover,
                                    Type = pJO.Type,
                                    Description = pJO.Description,
                                    Mission = pJO.Mission,
                                    Vision = pJO.Vision,
                                    Achievement = pJO.Achievement,
                                    IsActive = pJO.IsActive,
                                    FoundingDate = pJO.FoundingDate,
                                    Config = pJO.Config,
                                    CreationCode = pJO.CreationCode,
                                    CreationMessage = pJO.CreationMessage,
                                    TaxCode = pJO.TaxCode,
                                    Location = _locationService.GetLocationByOrgId(pJO.Id)
                                }
                            };
                        };
                    }
                }
                catch (Exception e)
                {
                    _logger.Error(e.Message);

                    throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<ProjectResponse>
                    {
                        ResultCode = ResultCode.SQL_ERROR_CODE,
                        ResultMessage = ResultCode.GetMessage(ResultCode.SQL_ERROR_CODE),
                        Data = default
                    });
                }
            }

            if (response == null)
            {
                return new BaseResponse<ProjectResponse>
                {
                    ResultCode = ResultCode.PROJECT_NOT_FOUND_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.PROJECT_NOT_FOUND_CODE),
                    Data = response
                };
            }

                response.Location = _locationService.GetLocationByProjectId(response.Id);

            return new BaseResponse<ProjectResponse>
            {
                ResultCode = ResultCode.SUCCESS_CODE,
                ResultMessage = ResultCode.GetMessage(ResultCode.SUCCESS_CODE),
                Data = response
            };
        }

        public BaseResponse<IEnumerable<ProjectResponse>> GetProjectByOrg(string orgId)
        {
            List<ProjectResponse> projectResponses = new List<ProjectResponse>();

            IEnumerable<string> keys = _redisService.GetKeysByPattern(PATTERN);

            //get from redis
            //foreach (string key in keys)
            //{
            //    ProjectResponse project = JsonConvert.DeserializeObject<ProjectResponse>(_distributedCache.GetString(key));

            //    if (project.OrgId.Equals(orgId))
            //    {
            //        projectResponses.Add(project);
            //    }
            //}
            //get from db
            if (_utilService.IsNullOrEmpty(projectResponses))
            {
                try
                {
                    using (var context = new WeSpreadCoreContext())
                    {
                        var projectJoinOrg =
                            from p in context.Projects
                            join o in context.Organizations
                            on p.OrgId equals o.Id
                            where p.OrgId == orgId
                            select new
                            {
                                ProId = p.Id,
                                ProName = p.Name,
                                ProEngName = p.EngName,
                                ProDescription = p.Description,
                                p.OrgId,
                                ProLogo = p.Logo,
                                ProCover = p.Cover,
                                ProCreatedBy = p.CreatedBy,
                                ProCreatedAt = p.CreatedAt,
                                ProUpdatedAt = p.UpdatedAt,
                                ProConfig = p.Config,
                                ProIsActive = p.IsActive,
                                ProCreationCode = p.CreationCode,
                                ProCreationMessage = p.CreationMessage,
                                p.StartDate,
                                p.EndDate,
                                o.Id,
                                o.Name,
                                o.EngName,
                                o.Logo,
                                o.Cover,
                                o.Type,
                                o.Description,
                                o.Mission,
                                o.Vision,
                                o.Achievement,
                                o.FoundingDate,
                                o.IsActive,
                                o.CreatedBy,
                                o.CreatedAt,
                                o.UpdatedAt,
                                o.Config,
                                o.CreationCode,
                                o.CreationMessage,
                                o.TaxCode
                            };

                        foreach (var pJO in projectJoinOrg)
                        {
                            ProjectResponse response = new ProjectResponse
                            {
                                Id = pJO.ProId,
                                Name = pJO.ProName,
                                EngName = pJO.ProEngName,
                                Description = pJO.ProDescription,
                                OrgId = pJO.OrgId,
                                Logo = pJO.ProLogo,
                                Cover = pJO.ProCover,
                                CreatedAt = pJO.ProCreatedAt,
                                CreatedBy = pJO.ProCreatedBy,
                                UpdatedAt = pJO.ProUpdatedAt,
                                Config = pJO.ProConfig,
                                IsActive = pJO.IsActive,
                                CreationCode = pJO.ProCreationCode,
                                CreationMessage = pJO.ProCreationMessage,
                                StartDate = pJO.StartDate,
                                EndDate = pJO.EndDate,
                                Org = new OrganizationResponse
                                {
                                    Id = pJO.Id,
                                    Name = pJO.Name,
                                    EngName = pJO.EngName,
                                    Logo = pJO.Logo,
                                    Cover = pJO.Cover,
                                    Type = pJO.Type,
                                    Description = pJO.Description,
                                    Mission = pJO.Mission,
                                    Vision = pJO.Vision,
                                    Achievement = pJO.Achievement,
                                    IsActive = pJO.IsActive,
                                    FoundingDate = pJO.FoundingDate,
                                    Config = pJO.Config,
                                    CreationCode = pJO.CreationCode,
                                    CreationMessage = pJO.CreationMessage,
                                    TaxCode = pJO.TaxCode,
                                    Location = _locationService.GetLocationByOrgId(pJO.Id)
                                }
                            };

                            projectResponses.Add(response);
                        };
                    }
                }
                catch (Exception e)
                {
                    _logger.Error(e.Message);

                    throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<ProjectResponse>
                    {
                        ResultCode = ResultCode.SQL_ERROR_CODE,
                        ResultMessage = ResultCode.GetMessage(ResultCode.SQL_ERROR_CODE),
                        Data = default
                    });
                }
            }

            if (_utilService.IsNullOrEmpty(projectResponses))
            {
                return new BaseResponse<IEnumerable<ProjectResponse>>
                {
                    ResultCode = ResultCode.PROJECT_NOT_FOUND_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.PROJECT_NOT_FOUND_CODE),
                    Data = default
                };
            }

            foreach (ProjectResponse response in projectResponses)
            {
                response.Location = _locationService.GetLocationByProjectId(response.Id);
            }

            return new BaseResponse<IEnumerable<ProjectResponse>>
            {
                ResultCode = ResultCode.SUCCESS_CODE,
                ResultMessage = ResultCode.GetMessage(ResultCode.SUCCESS_CODE),
                Data = projectResponses
            };
        }

        public BaseResponse<ProjectResponse> UpdateProject(string id, ProjectRequest projectRequest,
            string token, IFormFile logo, List<IFormFile> cover)
        {
            //validate token
            string userId = _persistentLoginService.GetUserIDByToken(token);
            if (String.IsNullOrEmpty(userId))
            {
                _logger.Error($"User with token {token} is unauthorized.");

                throw new HttpStatusException(HttpStatusCode.Unauthorized, new BaseResponse<ProjectResponse>
                {
                    ResultCode = ResultCode.USER_UNAUTHORIZED_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.USER_UNAUTHORIZED_CODE),
                    Data = default
                });
            }

            //validate role
            OrganizationResponse org = _organizationService.GetByIdAndUser(userId, projectRequest.OrgId, ORG_ADMIN);

            if (org == null)
            {
                _logger.Error($"User {userId} does not have permission to create project for Org {projectRequest.OrgId}");

                throw new HttpStatusException(HttpStatusCode.Unauthorized, new BaseResponse<ProjectResponse>
                {
                    ResultCode = ResultCode.USER_UNAUTHORIZED_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.USER_UNAUTHORIZED_CODE),
                    Data = default
                });
            }

            //validate Project id
            Project project;
            try
            {
                project = _unitOfWork.GetRepository<Project>().Get(id);
            }
            catch (Exception e)
            {
                _logger.Error(e.Message);

                throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<ProjectResponse>
                {
                    ResultCode = ResultCode.PROJECT_NOT_FOUND_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.PROJECT_NOT_FOUND_CODE),
                    Data = default
                });
            }

            //validate name
            if (!_validateDataService.IsValidName(projectRequest.ProjectName, null))
            {
                _logger.Error($"{projectRequest.ProjectName} is invalid name.");

                throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<ProjectResponse>
                {
                    ResultCode = ResultCode.INVALID_NAME_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.INVALID_NAME_CODE),
                    Data = default
                });
            }

            if (projectRequest.EngName != null)
            {
                if (!_validateDataService.IsValidName(projectRequest.EngName, null))
                {
                    _logger.Error($"{projectRequest.EngName} is invalid name.");

                    throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<ProjectResponse>
                    {
                        ResultCode = ResultCode.INVALID_NAME_CODE,
                        ResultMessage = ResultCode.GetMessage(ResultCode.INVALID_NAME_CODE),
                        Data = default
                    });
                }
            }

            //validate date
            if (_validateDataService.Is7DaysBeforeStartDate(DateTime.Now, projectRequest.StartDate))
            {
                if (!_validateDataService.Is15DaysBefore(projectRequest.StartDate, projectRequest.EndDate))
                {
                    _logger.Error($"Startdate {projectRequest.StartDate} is not 15 days before enddate {projectRequest.EndDate}");

                    throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<ProjectResponse>
                    {
                        ResultCode = ResultCode.INVALID_STARTTIME_ENDTIME_CODE,
                        ResultMessage = ResultCode.GetMessage(ResultCode.INVALID_STARTTIME_ENDTIME_CODE)
                    });
                }
            }
            else
            {
                _logger.Error($"Now is not 7 days before the project's start date {projectRequest.StartDate}");

                throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<ProjectResponse>
                {
                    ResultCode = ResultCode.INVALID_TIME_FOR_UPDATE_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.INVALID_TIME_FOR_UPDATE_CODE)
                });
            }

            //upload file to Firebase Storage
            string coverUrl = _uploadFirebaseService
                .UploadFilesToFirebase(cover, TYPE, projectRequest.ProjectName, "cover").Result;

            string logoUrl = _uploadFirebaseService
                .UploadFileToFirebase(logo, TYPE, projectRequest.ProjectName, "logo").Result;

            if (coverUrl is null || logoUrl is null)
            {
                throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<ProjectResponse>
                {
                    ResultCode = ResultCode.UPLOAD_FILE_FIREBASE_ERROR_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.UPLOAD_FILE_FIREBASE_ERROR_CODE)
                });
            }

            //update data project
            project = _mapper.Map(projectRequest, project);
            project.Name = projectRequest.ProjectName;
            project.UpdatedAt = DateTime.Now;
            project.Logo = logoUrl;
            project.Cover = coverUrl;
            project.CreationCode = ResultCode.UNVERIFIED_UPDATE_PROJECT_CODE;
            project.CreationMessage = ResultCode.GetMessage(ResultCode.UNVERIFIED_UPDATE_PROJECT_CODE);
            project.Checker = null;
            project.CheckerNavigation = null;

            ICollection<LocationResponse> location = _locationService.CreateLocation(projectRequest.Location);

            UpdateItemCategory(projectRequest.Category, id);
            UpdateItemLocation(location, id);

            //store to db
            try
            {
                _unitOfWork.GetRepository<Project>().Update(project);

                _unitOfWork.Commit();
            }
            catch (Exception e)
            {
                _logger.Error(e.Message);

                throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<ProjectResponse>
                {
                    ResultCode = ResultCode.SQL_ERROR_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.SQL_ERROR_CODE)
                });
            }

            ProjectResponse projectResponse = _mapper.Map<ProjectResponse>(project);
            projectResponse.Org = org;

            //store project Info to Redis
            string cacheKey = id;
            if (!string.IsNullOrEmpty(cacheKey))
            {
                _distributedCache.Remove(cacheKey);
            }

            _distributedCache.SetString(cacheKey, JsonConvert.SerializeObject(projectResponse).ToString());

            return new BaseResponse<ProjectResponse>
            {
                ResultCode = ResultCode.SUCCESS_CODE,
                ResultMessage = ResultCode.GetMessage(ResultCode.SUCCESS_CODE),
                Data = projectResponse
            };
        }

        public void CreateItemLocation(ICollection<LocationResponse> locations, string projectId)
        {
            foreach (LocationResponse location in locations)
            {
                try
                {
                    ItemLocation item = new ItemLocation
                    {
                        Id = _utilService.Create16DigitString(),
                        ProjectId = projectId,
                        LocationId = location.Id,
                    };

                    _unitOfWork.GetRepository<ItemLocation>().Add(item);

                }
                catch (Exception e)
                {
                    _logger.Error(e.Message);

                    throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<ItemLocation>
                    {
                        ResultCode = ResultCode.SQL_ERROR_CODE,
                        ResultMessage = ResultCode.GetMessage(ResultCode.SQL_ERROR_CODE)
                    });
                }
            }
        }

        public void CreateItemCategory(ICollection<string> categoryIds, string projectId)
        {
            if (_utilService.IsNullOrEmpty<string>(categoryIds))
            {
                _logger.Warning($"Project with id: {projectId} cannot create without any category");

                throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<ItemCategory>
                {
                    ResultCode = ResultCode.EMPTY_CATEGORY_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.EMPTY_CATEGORY_CODE)
                });
            }

            foreach (string categoryId in categoryIds)
            {
                try
                {
                    ItemCategory item = new ItemCategory
                    {
                        Id = _utilService.Create16DigitString(),
                        ProjectId = projectId,
                        CategoryId = categoryId,
                    };

                    _unitOfWork.GetRepository<ItemCategory>().Add(item);
                }
                catch (Exception e)
                {
                    _logger.Error(e.Message);

                    throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<ItemCategory>
                    {
                        ResultCode = ResultCode.SQL_ERROR_CODE,
                        ResultMessage = ResultCode.GetMessage(ResultCode.SQL_ERROR_CODE)
                    });
                }
            }
        }

        public BaseResponse<IEnumerable<ProjectResponse>> GetProjectByCategory(string cateId)
        {
            List<ProjectResponse> projectResponses = new List<ProjectResponse>();
            try
            {
                using (var context = new WeSpreadCoreContext())
                {
                    var projectJoinOrg =
                            from p in context.Projects
                            join c in context.ItemCategories
                            on p.Id equals c.ProjectId
                            join o in context.Organizations
                                  on p.OrgId equals o.Id
                            select new
                            {
                                ProId = p.Id,
                                ProName = p.Name,
                                ProEngName = p.EngName,
                                ProDescription = p.Description,
                                p.OrgId,
                                ProLogo = p.Logo,
                                ProCover = p.Cover,
                                ProCreatedBy = p.CreatedBy,
                                ProCreatedAt = p.CreatedAt,
                                ProUpdatedAt = p.UpdatedAt,
                                ProConfig = p.Config,
                                ProCreationCode = p.CreationCode,
                                ProIsActive = p.IsActive,
                                ProCreationMessage = p.CreationMessage,
                                p.StartDate,
                                p.EndDate,
                                o.Id,
                                o.Name,
                                o.EngName,
                                o.Logo,
                                o.Cover,
                                o.Type,
                                o.Description,
                                o.Mission,
                                o.Vision,
                                o.Achievement,
                                o.FoundingDate,
                                o.IsActive,
                                o.CreatedBy,
                                o.CreatedAt,
                                o.UpdatedAt,
                                o.Config,
                                o.CreationCode,
                                o.CreationMessage,
                                o.TaxCode
                            };

                    foreach (var pJO in projectJoinOrg)
                    {
                        ProjectResponse response = new ProjectResponse
                        {
                            Id = pJO.ProId,
                            Name = pJO.ProName,
                            EngName = pJO.ProEngName,
                            Description = pJO.ProDescription,
                            OrgId = pJO.OrgId,
                            Logo = pJO.ProLogo,
                            Cover = pJO.ProCover,
                            CreatedAt = pJO.ProCreatedAt,
                            CreatedBy = pJO.ProCreatedBy,
                            UpdatedAt = pJO.ProUpdatedAt,
                            IsActive = pJO.ProIsActive,
                            Config = pJO.ProConfig,
                            CreationCode = pJO.ProCreationCode,
                            CreationMessage = pJO.ProCreationMessage,
                            StartDate = pJO.StartDate,
                            EndDate = pJO.EndDate,
                            Org = new OrganizationResponse
                            {
                                Id = pJO.Id,
                                Name = pJO.Name,
                                EngName = pJO.EngName,
                                Logo = pJO.Logo,
                                Cover = pJO.Cover,
                                Type = pJO.Type,
                                Description = pJO.Description,
                                Mission = pJO.Mission,
                                Vision = pJO.Vision,
                                Achievement = pJO.Achievement,
                                IsActive = pJO.IsActive,
                                FoundingDate = pJO.FoundingDate,
                                Config = pJO.Config,
                                CreationCode = pJO.CreationCode,
                                CreationMessage = pJO.CreationMessage,
                                TaxCode = pJO.TaxCode,
                                Location = _locationService.GetLocationByOrgId(pJO.Id)
                            }
                        };

                        projectResponses.Add(response);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Error(e.Message);

                throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<ItemCategory>
                {
                    ResultCode = ResultCode.SQL_ERROR_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.SQL_ERROR_CODE)
                });
            }

            if (_utilService.IsNullOrEmpty(projectResponses))
            {
                return new BaseResponse<IEnumerable<ProjectResponse>>
                {
                    ResultCode = ResultCode.PROJECT_NOT_FOUND_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.PROJECT_NOT_FOUND_CODE),
                    Data = default
                };
            }

            foreach (ProjectResponse response in projectResponses)
            {
                response.Location = _locationService.GetLocationByProjectId(response.Id);
            }

            projectResponses.Sort((x, y) => x.CreatedAt.CompareTo(y.CreatedAt));

            return new BaseResponse<IEnumerable<ProjectResponse>>
            {
                ResultCode = ResultCode.SUCCESS_CODE,
                ResultMessage = ResultCode.GetMessage(ResultCode.SUCCESS_CODE),
                Data = projectResponses
            };
        }

        public BaseResponse<IEnumerable<ProjectResponse>> GetProjectByLocation(string locationId)
        {
            List<ProjectResponse> projectResponses = new List<ProjectResponse>();
            try
            {
                using (var context = new WeSpreadCoreContext())
                {
                    var projectJoinOrg =
                            from p in context.Projects
                            join c in context.ItemLocations
                            on p.Id equals c.ProjectId
                            join o in context.Organizations
                                  on p.OrgId equals o.Id
                            select new
                            {
                                ProId = p.Id,
                                ProName = p.Name,
                                ProEngName = p.EngName,
                                ProDescription = p.Description,
                                p.OrgId,
                                ProLogo = p.Logo,
                                ProCover = p.Cover,
                                ProCreatedBy = p.CreatedBy,
                                ProCreatedAt = p.CreatedAt,
                                ProUpdatedAt = p.UpdatedAt,
                                ProIsActive = p.IsActive,
                                ProConfig = p.Config,
                                ProCreationCode = p.CreationCode,
                                ProCreationMessage = p.CreationMessage,
                                p.StartDate,
                                p.EndDate,
                                o.Id,
                                o.Name,
                                o.EngName,
                                o.Logo,
                                o.Cover,
                                o.Type,
                                o.Description,
                                o.Mission,
                                o.Vision,
                                o.Achievement,
                                o.FoundingDate,
                                o.IsActive,
                                o.CreatedBy,
                                o.CreatedAt,
                                o.UpdatedAt,
                                o.Config,
                                o.CreationCode,
                                o.CreationMessage,
                                o.TaxCode
                            };

                    foreach (var pJO in projectJoinOrg)
                    {
                        ProjectResponse response = new ProjectResponse
                        {
                            Id = pJO.ProId,
                            Name = pJO.ProName,
                            EngName = pJO.ProEngName,
                            Description = pJO.ProDescription,
                            OrgId = pJO.OrgId,
                            Logo = pJO.ProLogo,
                            Cover = pJO.ProCover,
                            CreatedAt = pJO.ProCreatedAt,
                            CreatedBy = pJO.ProCreatedBy,
                            UpdatedAt = pJO.ProUpdatedAt,
                            IsActive = pJO.ProIsActive,
                            Config = pJO.ProConfig,
                            CreationCode = pJO.ProCreationCode,
                            CreationMessage = pJO.ProCreationMessage,
                            StartDate = pJO.StartDate,
                            EndDate = pJO.EndDate,
                            Org = new OrganizationResponse
                            {
                                Id = pJO.Id,
                                Name = pJO.Name,
                                EngName = pJO.EngName,
                                Logo = pJO.Logo,
                                Cover = pJO.Cover,
                                Type = pJO.Type,
                                Description = pJO.Description,
                                Mission = pJO.Mission,
                                Vision = pJO.Vision,
                                Achievement = pJO.Achievement,
                                IsActive = pJO.IsActive,
                                FoundingDate = pJO.FoundingDate,
                                Config = pJO.Config,
                                CreationCode = pJO.CreationCode,
                                CreationMessage = pJO.CreationMessage,
                                TaxCode = pJO.TaxCode,
                                Location = _locationService.GetLocationByOrgId(pJO.Id)
                            }
                        };

                        projectResponses.Add(response);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Error(e.Message);

                throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<ProjectResponse>
                {
                    ResultCode = ResultCode.SQL_ERROR_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.SQL_ERROR_CODE)
                });
            }

            if (_utilService.IsNullOrEmpty(projectResponses))
            {
                return new BaseResponse<IEnumerable<ProjectResponse>>
                {
                    ResultCode = ResultCode.PROJECT_NOT_FOUND_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.PROJECT_NOT_FOUND_CODE),
                    Data = default
                };
            }

            foreach (ProjectResponse response in projectResponses)
            {
                response.Location = _locationService.GetLocationByProjectId(response.Id);
            }

            projectResponses.Sort((x, y) => x.CreatedAt.CompareTo(y.CreatedAt));

            return new BaseResponse<IEnumerable<ProjectResponse>>
            {
                ResultCode = ResultCode.SUCCESS_CODE,
                ResultMessage = ResultCode.GetMessage(ResultCode.SUCCESS_CODE),
                Data = projectResponses
            };
        }

        public void UpdateItemCategory(ICollection<string> categoryIds, string projectId)
        {
            if (_utilService.IsNullOrEmpty<string>(categoryIds))
            {
                _logger.Warning($"Project with id: {projectId} cannot update without any category");

                throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<ItemCategory>
                {
                    ResultCode = ResultCode.EMPTY_CATEGORY_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.EMPTY_CATEGORY_CODE)
                });
            }

            try
            {
                IEnumerable<ItemCategory> itemsModel = _unitOfWork.GetRepository<ItemCategory>()
                    .GetAll()
                    .Where(item => item.ProjectId.Equals(projectId));

                foreach (ItemCategory item in itemsModel)
                {
                    if (!categoryIds.Contains(item.CategoryId))
                    {
                        _unitOfWork.GetRepository<ItemCategory>().Delete(item);
                        itemsModel = itemsModel.Where(x => x.CategoryId != item.CategoryId);
                    }
                }

                foreach (string categoryId in categoryIds)
                {
                    //if itemsModel is not contains ItemCategory with @param categoryId
                    if (_utilService.IsNullOrEmpty<ItemCategory>
                        (itemsModel.Where(x => x.CategoryId.Equals(categoryId))))
                    {
                        ItemCategory item = new ItemCategory
                        {
                            Id = _utilService.Create16DigitString(),
                            ProjectId = projectId,
                            CategoryId = categoryId
                        };

                        _unitOfWork.GetRepository<ItemCategory>().Add(item);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Error(e.Message);

                throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<ItemCategory>
                {
                    ResultCode = ResultCode.SQL_ERROR_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.SQL_ERROR_CODE)
                });
            }
        }

        public void UpdateItemLocation(ICollection<LocationResponse> locations, string projectId)
        {
            try
            {
                IEnumerable<ItemLocation> itemsModel = _unitOfWork.GetRepository<ItemLocation>()
                    .GetAll()
                    .Where(item => item.ProjectId.Equals(projectId));

                foreach (ItemLocation item in itemsModel)
                {
                    if (!locations.Where(location => location.Id == item.LocationId).Any())
                    {
                        _unitOfWork.GetRepository<ItemLocation>().Delete(item);
                        itemsModel = itemsModel.Where(x => x.LocationId != item.LocationId);
                    }
                }

                foreach (LocationResponse location in locations)
                {
                    //if itemsModel is not contains ItemCity with @param cityCode
                    if (_utilService.IsNullOrEmpty<ItemLocation>
                        (itemsModel.Where(x => x.LocationId.Equals(location.Id))))
                    {
                        ItemLocation item = new ItemLocation
                        {
                            Id = _utilService.Create16DigitString(),
                            ProjectId = projectId,
                            LocationId = location.Id,
                        };

                        _unitOfWork.GetRepository<ItemLocation>().Add(item);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Error(e.Message);

                throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<ItemLocation>
                {
                    ResultCode = ResultCode.SQL_ERROR_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.SQL_ERROR_CODE)
                });
            }
        }

        public Period GetProjectPeriod(string projectId)
        {
            BaseResponse<ProjectResponse> response = GetProjectById(projectId);

            return new Period
            {
                StartDate = response.Data.StartDate,
                EndDate = response.Data.EndDate
            };
        }

        public BaseResponse<ProjectResponse> DeleteProject(string id, string token)
        {
            //validate token
            string userId = _persistentLoginService.GetUserIDByToken(token);
            if (String.IsNullOrEmpty(userId))
            {
                _logger.Error($"User with token {token} is unauthorized.");

                throw new HttpStatusException(HttpStatusCode.Unauthorized, new BaseResponse<ProjectResponse>
                {
                    ResultCode = ResultCode.USER_UNAUTHORIZED_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.USER_UNAUTHORIZED_CODE)
                });
            }

            //validate Project id
            Project projectModel;
            try
            {
                projectModel = _unitOfWork.GetRepository<Project>().Get(id);
            }
            catch (Exception e)
            {
                _logger.Error(e.Message);

                throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<ProjectResponse>
                {
                    ResultCode = ResultCode.PROJECT_NOT_FOUND_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.PROJECT_NOT_FOUND_CODE)
                });
            }

            //validate role
            OrganizationResponse org = _organizationService.GetByIdAndUser(userId, projectModel.OrgId, ORG_ADMIN);

            if (org == null)
            {
                _logger.Error($"User {userId} does not have permission to create project for Org {projectModel.OrgId}");

                throw new HttpStatusException(HttpStatusCode.Unauthorized, new BaseResponse<ProjectResponse>
                {
                    ResultCode = ResultCode.USER_UNAUTHORIZED_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.USER_UNAUTHORIZED_CODE),
                    Data = default
                });
            }

            //validate date
            if (!_validateDataService.Is7DaysBeforeStartDate(DateTime.Now, projectModel.StartDate))
            {
                _logger.Error($"Now is not 7 days before the project's start date {projectModel.StartDate}");

                throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<ProjectResponse>
                {
                    ResultCode = ResultCode.INVALID_TIME_FOR_UPDATE_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.INVALID_TIME_FOR_UPDATE_CODE)
                });
            }

            //delete project
            if (projectModel.CreationCode == ResultCode.UNVERIFIED_CREATE_PROJECT_CODE)
            {
                projectModel.CreationCode = ResultCode.DELETED_PROJECT_CODE;
                projectModel.CreationMessage = ResultCode.GetMessage(ResultCode.DELETED_PROJECT_CODE);
            }
            else
            {
                projectModel.CreationCode = ResultCode.UNVERIFIED_DELETE_PROJECT_CODE;
                projectModel.CreationMessage = ResultCode.GetMessage(ResultCode.UNVERIFIED_DELETE_PROJECT_CODE);
                projectModel.Checker = null;
                projectModel.CheckerNavigation = null;
            }
            projectModel.UpdatedAt = DateTime.Now;
            try
            {
                _unitOfWork.GetRepository<Project>().Update(projectModel);

                _unitOfWork.Commit();
            }
            catch (Exception e)
            {
                _logger.Error(e.Message);

                throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<ProjectResponse>
                {
                    ResultCode = ResultCode.SQL_ERROR_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.SQL_ERROR_CODE)
                });
            }

            //create response
            ProjectResponse projectResponse = _mapper.Map<ProjectResponse>(projectModel);

            //store project Info to Redis
            string cacheKey = id;
            if (!string.IsNullOrEmpty(cacheKey))
            {
                _distributedCache.Remove(cacheKey);
            }

            _distributedCache.SetString(cacheKey, JsonConvert.SerializeObject(projectResponse).ToString());

            return new BaseResponse<ProjectResponse>
            {
                ResultCode = ResultCode.SUCCESS_CODE,
                ResultMessage = ResultCode.GetMessage(ResultCode.SUCCESS_CODE),
                Data = projectResponse
            };
        }

        public ProjectResponse CheckRoleInOrgByProIdAndUserId(string userId, string proId, string roleId)
        {
            ProjectResponse response = null;

            string projectRedis = _distributedCache.GetString(proId);
            string userOrganizationRedis = _distributedCache.GetString(userId + "_ROLE");

            if (!string.IsNullOrEmpty(userOrganizationRedis) && !string.IsNullOrEmpty(projectRedis))
            {
                ProjectResponse project = JsonConvert.DeserializeObject<ProjectResponse>(projectRedis);

                List<UserOrganization> userOrganizations = JsonConvert.DeserializeObject<List<UserOrganization>>(userOrganizationRedis);

                if (userOrganizations.Any(uo => uo.OrganizationId.Equals(project.OrgId) && uo.RoleId.Equals(roleId)))
                    return project;
            }
            try
            {
                using (var context = new WeSpreadCoreContext())
                {
                    var projectJoinOrg =
                            from p in context.Projects
                            join uo in context.UserOrganizations
                            on p.OrgId equals uo.OrganizationId
                            where uo.RoleId == roleId
                            join o in context.Organizations
                            on p.OrgId equals o.Id
                            select new
                            {
                                ProId = p.Id,
                                ProName = p.Name,
                                ProEngName = p.EngName,
                                ProDescription = p.Description,
                                p.OrgId,
                                ProLogo = p.Logo,
                                ProCover = p.Cover,
                                ProCreatedBy = p.CreatedBy,
                                ProCreatedAt = p.CreatedAt,
                                ProUpdatedAt = p.UpdatedAt,
                                ProConfig = p.Config,
                                ProCreationCode = p.CreationCode,
                                ProCreationMessage = p.CreationMessage,
                                p.StartDate,
                                p.EndDate,
                                o.Id,
                                o.Name,
                                o.EngName,
                                o.Logo,
                                o.Cover,
                                o.Type,
                                o.Description,
                                o.Mission,
                                o.Vision,
                                o.Achievement,
                                o.FoundingDate,
                                o.IsActive,
                                o.CreatedBy,
                                o.CreatedAt,
                                o.UpdatedAt,
                                o.Config,
                                o.CreationCode,
                                o.CreationMessage,
                                o.TaxCode
                            };

                    foreach (var pJO in projectJoinOrg)
                    {
                        response = new ProjectResponse
                        {
                            Id = pJO.ProId,
                            Name = pJO.ProName,
                            EngName = pJO.ProEngName,
                            Description = pJO.ProDescription,
                            OrgId = pJO.OrgId,
                            Logo = pJO.ProLogo,
                            Cover = pJO.ProCover,
                            CreatedAt = pJO.ProCreatedAt,
                            CreatedBy = pJO.ProCreatedBy,
                            UpdatedAt = pJO.ProUpdatedAt,
                            Config = pJO.ProConfig,
                            CreationCode = pJO.ProCreationCode,
                            CreationMessage = pJO.ProCreationMessage,
                            StartDate = pJO.StartDate,
                            EndDate = pJO.EndDate,
                            Org = new OrganizationResponse
                            {
                                Id = pJO.Id,
                                Name = pJO.Name,
                                EngName = pJO.EngName,
                                Logo = pJO.Logo,
                                Cover = pJO.Cover,
                                Type = pJO.Type,
                                Description = pJO.Description,
                                Mission = pJO.Mission,
                                Vision = pJO.Vision,
                                Achievement = pJO.Achievement,
                                IsActive = pJO.IsActive,
                                FoundingDate = pJO.FoundingDate,
                                Config = pJO.Config,
                                CreationCode = pJO.CreationCode,
                                CreationMessage = pJO.CreationMessage,
                                TaxCode = pJO.TaxCode
                            }
                        };
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Error(e.Message);

                throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<ProjectResponse>
                {
                    ResultCode = ResultCode.SQL_ERROR_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.SQL_ERROR_CODE)
                });
            }

            return response;
        }
    }
}
