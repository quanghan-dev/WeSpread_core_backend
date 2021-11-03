using AutoMapper;
using BLL.Constant;
using BLL.Dto;
using BLL.Dto.RecruitmentSession;
using BLL.Dto.Exception;
using DAL.Model;
using DAL.UnifOfWork;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using BLL.Dto.Project;

namespace BLL.Service.Impl
{
    public class RecruitmentSessionService : IRecruitmentSessionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly IDistributedCache _distributedCache;
        private readonly IPersistentLoginService _persistentLoginService;
        private readonly IValidateDataService _validateDataService;
        private readonly IProjectService _projectService;
        private readonly IUtilService _utilService;
        private readonly IRedisService _redisService;
        private readonly ILocationService _locationService;
        private const string ORG_ADMIN = "ORG_ADMIN";
        private const string PATTERN = "Rcm_*";
        private const string PREFIX = "Rcm_";

        public RecruitmentSessionService(IUnitOfWork unitOfWork, ILogger logger, IMapper mapper,
            IDistributedCache distributedCache, IPersistentLoginService persistentLoginService,
            IValidateDataService validateDataService, IProjectService projectService, IUtilService utilService,
            IRedisService redisService, ILocationService locationService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _mapper = mapper;
            _distributedCache = distributedCache;
            _persistentLoginService = persistentLoginService;
            _validateDataService = validateDataService;
            _projectService = projectService;
            _utilService = utilService;
            _redisService = redisService;
            _locationService = locationService;
        }

        public BaseResponse<RecruitmentSessionResponse> CreateRecruitmentSession(string token, RecruitmentSessionRequest recruitmentSessionRequest)
        {
            //validate token
            string userId = _persistentLoginService.GetUserIDByToken(token);
            if (String.IsNullOrEmpty(userId))
            {
                _logger.Error($"User with token {token} is unauthorized.");

                throw new HttpStatusException(HttpStatusCode.Unauthorized,
                    new BaseResponse<RecruitmentSessionResponse>
                    {
                        ResultCode = ResultCode.USER_UNAUTHORIZED_CODE,
                        ResultMessage = ResultCode.GetMessage(ResultCode.USER_UNAUTHORIZED_CODE)
                    });
            }

            //validate role
            ProjectResponse project = _projectService.CheckRoleInOrgByProIdAndUserId(userId, recruitmentSessionRequest.ProjectId, ORG_ADMIN);

            if (project == null)
            {
                _logger.Error($"User {userId} does not have permission to create donation session for project {recruitmentSessionRequest.ProjectId}");

                throw new HttpStatusException(HttpStatusCode.Unauthorized, new BaseResponse<RecruitmentSessionResponse>
                {
                    ResultCode = ResultCode.USER_UNAUTHORIZED_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.USER_UNAUTHORIZED_CODE)
                });
            }

            //validate name
            if (!_validateDataService.IsValidName(recruitmentSessionRequest.Name, null))
            {
                _logger.Error($"{recruitmentSessionRequest.Name} is invalid name.");

                throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<RecruitmentSessionResponse>
                {
                    ResultCode = ResultCode.INVALID_NAME_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.INVALID_NAME_CODE)
                });
            }

            //validate date
            if (_validateDataService.IsValidDateInProjectPeriod(
                _projectService.GetProjectPeriod(recruitmentSessionRequest.ProjectId),
                recruitmentSessionRequest.StartDate, recruitmentSessionRequest.EndDate))
            {
                if (!_validateDataService.Is15DaysBefore(recruitmentSessionRequest.StartDate, recruitmentSessionRequest.EndDate))
                {
                    _logger.Error($"Startdate {recruitmentSessionRequest.StartDate} is not before enddate {recruitmentSessionRequest.EndDate}");

                    throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<RecruitmentSessionResponse>
                    {
                        ResultCode = ResultCode.INVALID_STARTTIME_ENDTIME_CODE,
                        ResultMessage = ResultCode.GetMessage(ResultCode.INVALID_STARTTIME_ENDTIME_CODE)
                    });
                }
            }
            else
            {
                _logger.Error($"Startdate {recruitmentSessionRequest.StartDate} and enddate {recruitmentSessionRequest.EndDate} is not in project period.");

                throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<RecruitmentSessionResponse>
                {
                    ResultCode = ResultCode.INVALID_DATE_IN_PROJECT_PERIOD_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.INVALID_DATE_IN_PROJECT_PERIOD_CODE)
                });
            }

            //create recruitment session
            RecruitmentSession recruitmentSessionModel = _mapper.Map<RecruitmentSession>(recruitmentSessionRequest);
            try
            {
                recruitmentSessionModel.Id = PREFIX + ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds().ToString();
                recruitmentSessionModel.CreatedBy = userId;
                recruitmentSessionModel.CreatedAt = DateTime.Now;
                recruitmentSessionModel.UpdatedAt = DateTime.Now;
                recruitmentSessionModel.CreationCode = ResultCode.UNVERIFIED_CREATE_RECRUITMENT_SESSION_CODE;
                recruitmentSessionModel.CreationMessage = ResultCode.GetMessage(ResultCode.UNVERIFIED_CREATE_RECRUITMENT_SESSION_CODE);

                _unitOfWork.GetRepository<RecruitmentSession>().Add(recruitmentSessionModel);

                _unitOfWork.Commit();
            }
            catch (Exception e)
            {
                _logger.Error(e.Message);

                throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<RecruitmentSessionResponse>
                {
                    ResultCode = ResultCode.SQL_ERROR_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.SQL_ERROR_CODE)
                });
            }

            //create response
            RecruitmentSessionResponse rcmResponse = _mapper.Map<RecruitmentSessionResponse>(recruitmentSessionModel);
            rcmResponse.Project = project;

            //store donation session Info to Redis
            string cacheKey = rcmResponse.Id;
            string cacheValue = _distributedCache.GetString(cacheKey);

            if (!string.IsNullOrEmpty(cacheValue))
            {
                _distributedCache.Remove(cacheKey);
            }

            _distributedCache.SetString(cacheKey, JsonConvert.SerializeObject(rcmResponse).ToString());
            return new BaseResponse<RecruitmentSessionResponse>
            {
                ResultCode = ResultCode.SUCCESS_CODE,
                ResultMessage = ResultCode.GetMessage(ResultCode.SUCCESS_CODE),
                Data = rcmResponse
            };
        }

        public BaseResponse<RecruitmentSessionResponse> GetRecruitmentSessionById(string id)
        {
            RecruitmentSessionResponse recruitmentSession = null;
            //if (_distributedCache.GetString(id) != null)
            //{
            //    return new BaseResponse<RecruitmentSessionResponse>
            //    {
            //        ResultCode = ResultCode.SUCCESS_CODE,
            //        ResultMessage = ResultCode.GetMessage(ResultCode.SUCCESS_CODE),
            //        Data = JsonConvert.DeserializeObject<RecruitmentSessionResponse>(_distributedCache.GetString(id))
            //    };
            //}
            //else
            //{
            try
            {
                using (var context = new WeSpreadCoreContext())
                {

                    var recruitmentSessionJoinPro =
                        from r in context.RecruitmentSessions
                        join p in context.Projects
                        on r.ProjectId equals p.Id
                        where r.Id == id
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
                            ProStartDate = p.StartDate,
                            ProEndDate = p.EndDate,
                            r.Id,
                            r.Name,
                            r.Quota,
                            r.Benefit,
                            r.Description,
                            r.ProjectId,
                            r.CreatedBy,
                            r.CreatedAt,
                            r.UpdatedAt,
                            r.CreationCode,
                            r.CreationMessage,
                            r.Config,
                            r.StartDate,
                            r.EndDate,
                            r.JobDescription,
                            r.Requirement
                        };

                    foreach (var rcmJP in recruitmentSessionJoinPro)
                    {
                        recruitmentSession = new RecruitmentSessionResponse
                        {
                            Id = rcmJP.Id,
                            Name = rcmJP.Name,
                            Description = rcmJP.Description,
                            ProjectId = rcmJP.ProjectId,
                            Benefit = rcmJP.Benefit,
                            JobDescription = rcmJP.JobDescription,
                            Requirement = rcmJP.Requirement,
                            CreatedAt = rcmJP.CreatedAt,
                            CreatedBy = rcmJP.CreatedBy,
                            UpdatedAt = rcmJP.UpdatedAt,
                            Config = rcmJP.Config,
                            CreationCode = rcmJP.CreationCode,
                            CreationMessage = rcmJP.CreationMessage,
                            StartDate = rcmJP.StartDate,
                            EndDate = rcmJP.EndDate,
                            Quota = rcmJP.Quota,
                            Project = new ProjectResponse
                            {
                                Id = rcmJP.ProId,
                                Name = rcmJP.ProName,
                                EngName = rcmJP.ProEngName,
                                Logo = rcmJP.ProLogo,
                                Cover = rcmJP.ProCover,
                                Description = rcmJP.ProDescription,
                                Config = rcmJP.ProConfig,
                                CreationCode = rcmJP.ProCreationCode,
                                CreationMessage = rcmJP.ProCreationMessage,
                                CreatedAt = rcmJP.ProCreatedAt,
                                CreatedBy = rcmJP.ProCreatedBy,
                                UpdatedAt = rcmJP.ProUpdatedAt,
                                StartDate = rcmJP.ProStartDate,
                                EndDate = rcmJP.ProEndDate,
                                Location = _locationService.GetLocationByProjectId(rcmJP.ProId)
                            },
                            TotalApplies = GetTotalAppliesBySessionId(rcmJP.Id)
                        };
                    };
                }
            }
            catch (Exception e)
            {
                _logger.Error(e.Message);

                throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<RecruitmentSessionResponse>
                {
                    ResultCode = ResultCode.SQL_ERROR_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.SQL_ERROR_CODE)
                });
            }
            //}
            if (recruitmentSession == null)
            {
                return new BaseResponse<RecruitmentSessionResponse>
                {
                    ResultCode = ResultCode.RECRUITMENT_SESSION_NOT_FOUND_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.RECRUITMENT_SESSION_NOT_FOUND_CODE),
                    Data = default
                };
            }

            return new BaseResponse<RecruitmentSessionResponse>
            {
                ResultCode = ResultCode.SUCCESS_CODE,
                ResultMessage = ResultCode.GetMessage(ResultCode.SUCCESS_CODE),
                Data = recruitmentSession
            };
        }

        public BaseResponse<IEnumerable<RecruitmentSessionResponse>> GetRecruitmentSessionByProjectId(string projectId)
        {
            List<RecruitmentSessionResponse> recruitmentSessions = new List<RecruitmentSessionResponse>();

            IEnumerable<string> keys = _redisService.GetKeysByPattern(PATTERN);

            //get from redis
            //foreach (string key in keys)
            //{
            //    RecruitmentSessionResponse recruitmentSession = JsonConvert.DeserializeObject<RecruitmentSessionResponse>(_distributedCache.GetString(key));

            //    if (recruitmentSession.ProjectId.Equals(projectId))
            //    {
            //        recruitmentSessions.Add(recruitmentSession);
            //    }
            //}

            //get from db
            if (_utilService.IsNullOrEmpty(recruitmentSessions))
            {
                try
                {
                    using (var context = new WeSpreadCoreContext())
                    {

                        var recruitmentSessionJoinPro =
                            from r in context.RecruitmentSessions
                            join p in context.Projects
                            on r.ProjectId equals p.Id
                            where r.ProjectId == projectId
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
                                IsActive = p.IsActive,
                                ProCreationCode = p.CreationCode,
                                ProCreationMessage = p.CreationMessage,
                                ProStartDate = p.StartDate,
                                ProEndDate = p.EndDate,
                                r.Id,
                                r.Name,
                                r.Benefit,
                                r.Description,
                                r.ProjectId,
                                r.CreatedBy,
                                r.CreatedAt,
                                r.UpdatedAt,
                                r.CreationCode,
                                r.CreationMessage,
                                r.Config,
                                r.StartDate,
                                r.EndDate,
                                r.JobDescription,
                                r.Requirement,
                                r.Quota
                            };

                        foreach (var rcmJP in recruitmentSessionJoinPro)
                        {
                            RecruitmentSessionResponse recruitmentSession = new RecruitmentSessionResponse
                            {
                                Id = rcmJP.Id,
                                Name = rcmJP.Name,
                                Description = rcmJP.Description,
                                ProjectId = rcmJP.ProjectId,
                                Benefit = rcmJP.Benefit,
                                JobDescription = rcmJP.JobDescription,
                                Requirement = rcmJP.Requirement,
                                CreatedAt = rcmJP.CreatedAt,
                                CreatedBy = rcmJP.CreatedBy,
                                UpdatedAt = rcmJP.UpdatedAt,
                                Config = rcmJP.Config,
                                CreationCode = rcmJP.CreationCode,
                                CreationMessage = rcmJP.CreationMessage,
                                StartDate = rcmJP.StartDate,
                                EndDate = rcmJP.EndDate,
                                Quota = rcmJP.Quota,
                                Project = new ProjectResponse
                                {
                                    Id = rcmJP.ProId,
                                    Name = rcmJP.ProName,
                                    EngName = rcmJP.ProEngName,
                                    Logo = rcmJP.ProLogo,
                                    Cover = rcmJP.ProCover,
                                    Description = rcmJP.ProDescription,
                                    Config = rcmJP.ProConfig,
                                    CreationCode = rcmJP.ProCreationCode,
                                    CreationMessage = rcmJP.ProCreationMessage,
                                    CreatedAt = rcmJP.ProCreatedAt,
                                    CreatedBy = rcmJP.ProCreatedBy,
                                    UpdatedAt = rcmJP.ProUpdatedAt,
                                    StartDate = rcmJP.ProStartDate,
                                    EndDate = rcmJP.ProEndDate,
                                    IsActive = rcmJP.IsActive,
                                    Location = _locationService.GetLocationByProjectId(rcmJP.ProId)
                                },
                                TotalApplies = GetTotalAppliesBySessionId(rcmJP.Id)
                            };

                            recruitmentSessions.Add(recruitmentSession);
                        };
                    }
                }
                catch (Exception e)
                {
                    _logger.Error(e.Message);

                    throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<RecruitmentSessionResponse>
                    {
                        ResultCode = ResultCode.RECRUITMENT_SESSION_NOT_FOUND_CODE,
                        ResultMessage = ResultCode.GetMessage(ResultCode.RECRUITMENT_SESSION_NOT_FOUND_CODE)
                    });
                }
            }

            if (_utilService.IsNullOrEmpty(recruitmentSessions))
            {
                return new BaseResponse<IEnumerable<RecruitmentSessionResponse>>
                {
                    ResultCode = ResultCode.RECRUITMENT_SESSION_NOT_FOUND_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.RECRUITMENT_SESSION_NOT_FOUND_CODE),
                    Data = default
                };
            }

            return new BaseResponse<IEnumerable<RecruitmentSessionResponse>>
            {
                ResultCode = ResultCode.SUCCESS_CODE,
                ResultMessage = ResultCode.GetMessage(ResultCode.SUCCESS_CODE),
                Data = recruitmentSessions
            };
        }

        public BaseResponse<RecruitmentSessionResponse> UpdateRecruitmentSession(string token, string id,
            RecruitmentSessionRequest recruitmentSessionRequest)
        {
            //validate token
            string userId = _persistentLoginService.GetUserIDByToken(token);
            if (String.IsNullOrEmpty(userId))
            {
                _logger.Error($"User with token {token} is unauthorized.");

                throw new HttpStatusException(HttpStatusCode.Unauthorized, new BaseResponse<RecruitmentSessionResponse>
                {
                    ResultCode = ResultCode.USER_UNAUTHORIZED_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.USER_UNAUTHORIZED_CODE)
                });
            }

            //validate role
            ProjectResponse project = _projectService.CheckRoleInOrgByProIdAndUserId(userId, recruitmentSessionRequest.ProjectId, ORG_ADMIN);

            if (project == null)
            {
                _logger.Error($"User {userId} does not have permission to create donation session for project {recruitmentSessionRequest.ProjectId}");

                throw new HttpStatusException(HttpStatusCode.Unauthorized, new BaseResponse<RecruitmentSessionResponse>
                {
                    ResultCode = ResultCode.USER_UNAUTHORIZED_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.USER_UNAUTHORIZED_CODE)
                });
            }

            //validate recruitment session id
            RecruitmentSession recruitmentSessionModel;
            try
            {
                recruitmentSessionModel = _unitOfWork.GetRepository<RecruitmentSession>().Get(id);
            }
            catch (Exception e)
            {
                _logger.Error(e.Message);

                throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<RecruitmentSessionResponse>
                {
                    ResultCode = ResultCode.RECRUITMENT_SESSION_NOT_FOUND_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.RECRUITMENT_SESSION_NOT_FOUND_CODE)
                });
            }

            //validate name
            if (!_validateDataService.IsValidName(recruitmentSessionRequest.Name, null))
            {
                _logger.Error($"{recruitmentSessionRequest.Name} is invalid name.");

                throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<RecruitmentSessionResponse>
                {
                    ResultCode = ResultCode.INVALID_NAME_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.INVALID_NAME_CODE)
                });
            }

            //validate date
            Period proPeriod = _projectService.GetProjectPeriod(recruitmentSessionRequest.ProjectId);

            if (_validateDataService.Is7DaysBeforeStartDate(DateTime.Now, proPeriod.StartDate))
            {
                if (_validateDataService.IsValidDateInProjectPeriod(proPeriod, recruitmentSessionRequest.StartDate,
                    recruitmentSessionRequest.EndDate))
                {
                    if (!_validateDataService.Is15DaysBefore(recruitmentSessionRequest.StartDate, recruitmentSessionRequest.EndDate))
                    {
                        _logger.Error($"Startdate {recruitmentSessionRequest.StartDate} is not before enddate {recruitmentSessionRequest.EndDate}");

                        throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<RecruitmentSessionResponse>
                        {
                            ResultCode = ResultCode.INVALID_STARTTIME_ENDTIME_CODE,
                            ResultMessage = ResultCode.GetMessage(ResultCode.INVALID_STARTTIME_ENDTIME_CODE)
                        });
                    }
                }
                else
                {
                    _logger.Error($"Startdate {recruitmentSessionRequest.StartDate} and enddate {recruitmentSessionRequest.EndDate} is not in project period.");

                    throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<RecruitmentSessionResponse>
                    {
                        ResultCode = ResultCode.INVALID_DATE_IN_PROJECT_PERIOD_CODE,
                        ResultMessage = ResultCode.GetMessage(ResultCode.INVALID_DATE_IN_PROJECT_PERIOD_CODE)
                    });
                }
            }
            else
            {
                _logger.Error($"Now is not before the project's start date {proPeriod.StartDate}");

                throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<RecruitmentSessionResponse>
                {
                    ResultCode = ResultCode.INVALID_TIME_FOR_UPDATE_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.INVALID_TIME_FOR_UPDATE_CODE)
                });
            }


            //update data
            recruitmentSessionModel = _mapper.Map(recruitmentSessionRequest, recruitmentSessionModel);
            recruitmentSessionModel.UpdatedAt = DateTime.Now;
            recruitmentSessionModel.CreationCode = ResultCode.UNVERIFIED_UPDATE_RECRUITMENT_SESSION_CODE;
            recruitmentSessionModel.CreationMessage = ResultCode.GetMessage(ResultCode.UNVERIFIED_UPDATE_RECRUITMENT_SESSION_CODE);
            recruitmentSessionModel.Checker = null;
            recruitmentSessionModel.CheckerNavigation = null;

            try
            {
                _unitOfWork.GetRepository<RecruitmentSession>().Update(recruitmentSessionModel);

                _unitOfWork.Commit();
            }
            catch (Exception e)
            {
                _logger.Error(e.Message);

                throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<RecruitmentSessionResponse>
                {
                    ResultCode = ResultCode.SQL_ERROR_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.SQL_ERROR_CODE)
                });
            }

            //create response
            RecruitmentSessionResponse rcmResponse = _mapper.Map<RecruitmentSessionResponse>(recruitmentSessionModel);
            rcmResponse.Project = project;

            //store recruitment session Info to Redis
            string cacheKey = rcmResponse.Id;
            string cacheValue = _distributedCache.GetString(cacheKey);

            if (!string.IsNullOrEmpty(cacheValue))
            {
                _distributedCache.Remove(cacheKey);
            }

            _distributedCache.SetString(cacheKey, JsonConvert.SerializeObject(rcmResponse).ToString());
            return new BaseResponse<RecruitmentSessionResponse>
            {
                ResultCode = ResultCode.SUCCESS_CODE,
                ResultMessage = ResultCode.GetMessage(ResultCode.SUCCESS_CODE),
                Data = rcmResponse
            };
        }

        public BaseResponse<RecruitmentSessionResponse> DeleteRecruitmentSession(string token, string id)
        {
            //validate token
            string userId = _persistentLoginService.GetUserIDByToken(token);
            if (String.IsNullOrEmpty(userId))
            {
                _logger.Error($"User with token {token} is unauthorized.");

                throw new HttpStatusException(HttpStatusCode.Unauthorized, new BaseResponse<RecruitmentSessionResponse>
                {
                    ResultCode = ResultCode.USER_UNAUTHORIZED_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.USER_UNAUTHORIZED_CODE)
                });
            }

            //validate recruitment session id
            RecruitmentSession recruitmentSessionModel;
            try
            {
                recruitmentSessionModel = _unitOfWork.GetRepository<RecruitmentSession>().Get(id);
            }
            catch (Exception e)
            {
                _logger.Error(e.Message);

                throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<RecruitmentSessionResponse>
                {
                    ResultCode = ResultCode.RECRUITMENT_SESSION_NOT_FOUND_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.RECRUITMENT_SESSION_NOT_FOUND_CODE)
                });
            }

            //validate role
            ProjectResponse project = _projectService.CheckRoleInOrgByProIdAndUserId(userId, recruitmentSessionModel.ProjectId, ORG_ADMIN);

            if (project == null)
            {
                _logger.Error($"User {userId} does not have permission to create donation session for project {recruitmentSessionModel.ProjectId}");

                throw new HttpStatusException(HttpStatusCode.Unauthorized, new BaseResponse<RecruitmentSessionResponse>
                {
                    ResultCode = ResultCode.USER_UNAUTHORIZED_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.USER_UNAUTHORIZED_CODE)
                });
            }

            //delete recruitment session
            if (recruitmentSessionModel.CreationCode == ResultCode.UNVERIFIED_CREATE_RECRUITMENT_SESSION_CODE)
            {
                recruitmentSessionModel.CreationCode = ResultCode.DELETED_RECRUITMENT_SESSION_CODE;
                recruitmentSessionModel.CreationMessage = ResultCode.GetMessage(ResultCode.DELETED_RECRUITMENT_SESSION_CODE);
            }
            else
            {
                recruitmentSessionModel.CreationCode = ResultCode.UNVERIFIED_DELETE_RECRUITMENT_SESSION_CODE;
                recruitmentSessionModel.CreationMessage = ResultCode.GetMessage(ResultCode.UNVERIFIED_DELETE_RECRUITMENT_SESSION_CODE);
                recruitmentSessionModel.Checker = null;
                recruitmentSessionModel.CheckerNavigation = null;
            }

            recruitmentSessionModel.UpdatedAt = DateTime.Now;
            try
            {
                _unitOfWork.GetRepository<RecruitmentSession>().Update(recruitmentSessionModel);

                _unitOfWork.Commit();
            }
            catch (Exception e)
            {
                _logger.Error(e.Message);

                throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<RecruitmentSessionResponse>
                {
                    ResultCode = ResultCode.SQL_ERROR_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.SQL_ERROR_CODE)
                });
            }
            //create response
            RecruitmentSessionResponse rcmResponse = _mapper.Map<RecruitmentSessionResponse>(recruitmentSessionModel);

            //store recruitment session Info to Redis
            string cacheKey = rcmResponse.Id;
            string cacheValue = _distributedCache.GetString(cacheKey);

            if (!string.IsNullOrEmpty(cacheValue))
            {
                _distributedCache.Remove(cacheKey);
            }

            _distributedCache.SetString(cacheKey, JsonConvert.SerializeObject(rcmResponse).ToString());
            return new BaseResponse<RecruitmentSessionResponse>
            {
                ResultCode = ResultCode.SUCCESS_CODE,
                ResultMessage = ResultCode.GetMessage(ResultCode.SUCCESS_CODE),
                Data = rcmResponse
            };
        }

        public double GetTotalAppliesBySessionId(string sessionId)
        {
            try
            {
                double totalApplies = _unitOfWork.GetRepository<RegistrationForm>()
                    .GetAll().Where(rf => rf.SessionId == sessionId)
                    .Count();

                return totalApplies;
            }
            catch (Exception e)
            {
                _logger.Error("[GetTotalAppliesBySessionId] " + e.Message);

                throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<DonateDetail>
                {
                    ResultCode = ResultCode.SQL_ERROR_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.SQL_ERROR_CODE)
                });
            }
        }

        public BaseResponse<IEnumerable<RecruitmentSessionResponse>> GetAllRecruitmentSession()
        {
            List<RecruitmentSessionResponse> recruitmentSessions = new List<RecruitmentSessionResponse>();

            IEnumerable<string> keys = _redisService.GetKeysByPattern(PATTERN);

            //get from redis
            //foreach (string key in keys)
            //{
            //    RecruitmentSessionResponse recruitmentSession = JsonConvert.DeserializeObject<RecruitmentSessionResponse>(_distributedCache.GetString(key));

            //    if (recruitmentSession.ProjectId.Equals(projectId))
            //    {
            //        recruitmentSessions.Add(recruitmentSession);
            //    }
            //}

            //get from db
            if (_utilService.IsNullOrEmpty(recruitmentSessions))
            {
                try
                {
                    using (var context = new WeSpreadCoreContext())
                    {

                        var recruitmentSessionJoinPro =
                            from r in context.RecruitmentSessions
                            join p in context.Projects
                            on r.ProjectId equals p.Id
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
                                ProStartDate = p.StartDate,
                                ProEndDate = p.EndDate,
                                IsActive = p.IsActive,
                                r.Id,
                                r.Name,
                                r.Benefit,
                                r.Description,
                                r.ProjectId,
                                r.CreatedBy,
                                r.CreatedAt,
                                r.UpdatedAt,
                                r.CreationCode,
                                r.CreationMessage,
                                r.Config,
                                r.StartDate,
                                r.EndDate,
                                r.JobDescription,
                                r.Requirement,
                                r.Quota
                            };

                        foreach (var rcmJP in recruitmentSessionJoinPro)
                        {
                            RecruitmentSessionResponse recruitmentSession = new RecruitmentSessionResponse
                            {
                                Id = rcmJP.Id,
                                Name = rcmJP.Name,
                                Description = rcmJP.Description,
                                ProjectId = rcmJP.ProjectId,
                                Benefit = rcmJP.Benefit,
                                JobDescription = rcmJP.JobDescription,
                                Requirement = rcmJP.Requirement,
                                CreatedAt = rcmJP.CreatedAt,
                                CreatedBy = rcmJP.CreatedBy,
                                UpdatedAt = rcmJP.UpdatedAt,
                                Config = rcmJP.Config,
                                CreationCode = rcmJP.CreationCode,
                                CreationMessage = rcmJP.CreationMessage,
                                StartDate = rcmJP.StartDate,
                                EndDate = rcmJP.EndDate,
                                Quota = rcmJP.Quota,
                                Project = new ProjectResponse
                                {
                                    Id = rcmJP.ProId,
                                    Name = rcmJP.ProName,
                                    EngName = rcmJP.ProEngName,
                                    Logo = rcmJP.ProLogo,
                                    Cover = rcmJP.ProCover,
                                    Description = rcmJP.ProDescription,
                                    Config = rcmJP.ProConfig,
                                    CreationCode = rcmJP.ProCreationCode,
                                    CreationMessage = rcmJP.ProCreationMessage,
                                    CreatedAt = rcmJP.ProCreatedAt,
                                    CreatedBy = rcmJP.ProCreatedBy,
                                    UpdatedAt = rcmJP.ProUpdatedAt,
                                    StartDate = rcmJP.ProStartDate,
                                    EndDate = rcmJP.ProEndDate,
                                    IsActive = rcmJP.IsActive,
                                    Location = _locationService.GetLocationByProjectId(rcmJP.ProId)
                                },
                                TotalApplies = GetTotalAppliesBySessionId(rcmJP.Id)
                            };

                            recruitmentSessions.Add(recruitmentSession);
                        };
                    }
                }
                catch (Exception e)
                {
                    _logger.Error(e.Message);

                    throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<RecruitmentSessionResponse>
                    {
                        ResultCode = ResultCode.RECRUITMENT_SESSION_NOT_FOUND_CODE,
                        ResultMessage = ResultCode.GetMessage(ResultCode.RECRUITMENT_SESSION_NOT_FOUND_CODE)
                    });
                }
            }

            if (_utilService.IsNullOrEmpty(recruitmentSessions))
            {
                return new BaseResponse<IEnumerable<RecruitmentSessionResponse>>
                {
                    ResultCode = ResultCode.RECRUITMENT_SESSION_NOT_FOUND_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.RECRUITMENT_SESSION_NOT_FOUND_CODE),
                    Data = default
                };
            }

            return new BaseResponse<IEnumerable<RecruitmentSessionResponse>>
            {
                ResultCode = ResultCode.SUCCESS_CODE,
                ResultMessage = ResultCode.GetMessage(ResultCode.SUCCESS_CODE),
                Data = recruitmentSessions
            };
        }
    }
}
