using AutoMapper;
using BLL.Constant;
using BLL.Dto;
using BLL.Dto.DonationSession;
using BLL.Dto.Exception;
using BLL.Dto.Project;
using DAL.Model;
using DAL.UnifOfWork;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace BLL.Service.Impl
{
    public class DonationSessionService : IDonationSessionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly IDistributedCache _distributedCache;
        private readonly IPersistentLoginService _persistentLoginService;
        private readonly IValidateDataService _validateDataService;
        private readonly IProjectService _projectService;
        private readonly IRedisService _redisService;
        private readonly IUtilService _utilService;
        private readonly ILocationService _locationService;
        private readonly IDonateService _donateService;
        private const string ORG_ADMIN = "ORG_ADMIN";
        private const string PATTERN = "Dnss_*";
        private const string PREFIX = "Dnss_";

        public DonationSessionService(IUnitOfWork unitOfWork, ILogger logger, IMapper mapper,
            IDistributedCache distributedCache, IPersistentLoginService persistentLoginService,
            IValidateDataService validateDataService, IProjectService projectService, IRedisService redisService,
            IUtilService utilService, ILocationService locationService, IDonateService donateService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _mapper = mapper;
            _distributedCache = distributedCache;
            _persistentLoginService = persistentLoginService;
            _validateDataService = validateDataService;
            _projectService = projectService;
            _redisService = redisService;
            _utilService = utilService;
            _locationService = locationService;
            _donateService = donateService;
        }

        //Create DonationSession
        public BaseResponse<DonationSessionResponse> CreateDonationSession(string token, DonationSessionRequest donationSessionRequest)
        {
            //validate token
            string userId = _persistentLoginService.GetUserIDByToken(token);
            if (String.IsNullOrEmpty(userId))
            {
                _logger.Error($"User with token {token} is unauthorized.");

                throw new HttpStatusException(HttpStatusCode.Unauthorized, new BaseResponse<DonationSessionResponse>
                {
                    ResultCode = ResultCode.USER_UNAUTHORIZED_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.USER_UNAUTHORIZED_CODE)
                });
            }

            //validate role
            ProjectResponse project = _projectService.CheckRoleInOrgByProIdAndUserId(userId, donationSessionRequest.ProjectId, ORG_ADMIN);

            if (project == null)
            {
                _logger.Error($"User {userId} does not have permission to create donation session for project {donationSessionRequest.ProjectId}");

                throw new HttpStatusException(HttpStatusCode.Unauthorized, new BaseResponse<DonationSessionResponse>
                {
                    ResultCode = ResultCode.USER_UNAUTHORIZED_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.USER_UNAUTHORIZED_CODE)
                });
            }

            //validate name
            if (!_validateDataService.IsValidName(donationSessionRequest.Name, null))
            {
                _logger.Error($"{donationSessionRequest.Name} is invalid name.");

                throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<DonationSessionResponse>
                {
                    ResultCode = ResultCode.INVALID_NAME_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.INVALID_NAME_CODE)
                });
            }

            //validate date
            if (_validateDataService.IsValidDateInProjectPeriod(
                _projectService.GetProjectPeriod(donationSessionRequest.ProjectId),
                donationSessionRequest.StartDate, donationSessionRequest.EndDate))
            {
                if (!_validateDataService.Is15DaysBefore(donationSessionRequest.StartDate, donationSessionRequest.EndDate))
                {
                    _logger.Error($"Startdate {donationSessionRequest.StartDate} is not before enddate {donationSessionRequest.EndDate}");

                    throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<DonationSessionResponse>
                    {
                        ResultCode = ResultCode.INVALID_STARTTIME_ENDTIME_CODE,
                        ResultMessage = ResultCode.GetMessage(ResultCode.INVALID_STARTTIME_ENDTIME_CODE)
                    });
                }
            }
            else
            {
                _logger.Error($"Startdate {donationSessionRequest.StartDate} and enddate {donationSessionRequest.EndDate} is not in project period.");

                throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<DonationSessionResponse>
                {
                    ResultCode = ResultCode.INVALID_DATE_IN_PROJECT_PERIOD_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.INVALID_DATE_IN_PROJECT_PERIOD_CODE)
                });
            }

            //validate target
            if (!_validateDataService.IsValidDonationTarget(donationSessionRequest.Target))
            {
                _logger.Error($"Target '{donationSessionRequest.Target}' is not valid.");

                throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<DonationSessionResponse>
                {
                    ResultCode = ResultCode.INVALID_DONATION_TARGET_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.INVALID_DONATION_TARGET_CODE)
                });
            }

            //create donation session
            DonationSession donationSessionModel = _mapper.Map<DonationSession>(donationSessionRequest);
            try
            {
                donationSessionModel.Id = PREFIX + ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds().ToString();
                donationSessionModel.CreatedBy = userId;
                donationSessionModel.CreatedAt = DateTime.Now;
                donationSessionModel.UpdatedAt = DateTime.Now;
                donationSessionModel.CreationCode = ResultCode.UNVERIFIED_CREATE_DONATION_SESSION_CODE;
                donationSessionModel.CreationMessage = ResultCode.GetMessage(ResultCode.UNVERIFIED_CREATE_DONATION_SESSION_CODE);

                _unitOfWork.GetRepository<DonationSession>().Add(donationSessionModel);

                _unitOfWork.Commit();
            }
            catch (Exception e)
            {
                _logger.Error(e.Message);

                throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<DonationSessionResponse>
                {
                    ResultCode = ResultCode.SQL_ERROR_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.SQL_ERROR_CODE)
                });
            }

            //create response
            DonationSessionResponse dnssResponse = _mapper.Map<DonationSessionResponse>(donationSessionModel);
            dnssResponse.Project = project;

            //store donation session Info to Redis
            string cacheKey = dnssResponse.Id;
            string cacheValue = _distributedCache.GetString(cacheKey);

            if (!string.IsNullOrEmpty(cacheValue))
            {
                _distributedCache.Remove(cacheKey);
            }

            _distributedCache.SetString(cacheKey, JsonConvert.SerializeObject(dnssResponse).ToString());
            return new BaseResponse<DonationSessionResponse>
            {
                ResultCode = ResultCode.SUCCESS_CODE,
                ResultMessage = ResultCode.GetMessage(ResultCode.SUCCESS_CODE),
                Data = dnssResponse
            };
        }

        public BaseResponse<DonationSessionResponse> GetDonationSessionById(string id)
        {
            DonationSessionResponse response = null;
            //if (_distributedCache.GetString(id) != null)
            //{
            //    return new BaseResponse<DonationSessionResponse>
            //    {
            //        ResultCode = ResultCode.SUCCESS_CODE,
            //        ResultMessage = ResultCode.GetMessage(ResultCode.SUCCESS_CODE),
            //        Data = JsonConvert.DeserializeObject<DonationSessionResponse>(_distributedCache.GetString(id))
            //    };
            //}
            //else
            //{
            try
            {
                using (var context = new WeSpreadCoreContext())
                {

                    var donationSessionJoinPro =
                        from d in context.DonationSessions
                        join p in context.Projects
                        on d.ProjectId equals p.Id
                        where d.Id == id
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
                            d.Id,
                            d.Name,
                            d.Target,
                            d.Description,
                            d.ProjectId,
                            d.CreatedBy,
                            d.CreatedAt,
                            d.UpdatedAt,
                            d.CreationCode,
                            d.CreationMessage,
                            d.Config,
                            d.StartDate,
                            d.EndDate
                        };

                    foreach (var dsJP in donationSessionJoinPro)
                    {
                        response = new DonationSessionResponse
                        {
                            Id = dsJP.Id,
                            Name = dsJP.Name,
                            Description = dsJP.Description,
                            ProjectId = dsJP.ProjectId,
                            Target = dsJP.Target,
                            CreatedAt = dsJP.CreatedAt,
                            CreatedBy = dsJP.CreatedBy,
                            UpdatedAt = dsJP.UpdatedAt,
                            Config = dsJP.Config,
                            CreationCode = dsJP.CreationCode,
                            CreationMessage = dsJP.CreationMessage,
                            StartDate = dsJP.StartDate,
                            EndDate = dsJP.EndDate,
                            Project = new ProjectResponse
                            {
                                Id = dsJP.ProId,
                                Name = dsJP.ProName,
                                EngName = dsJP.ProEngName,
                                Logo = dsJP.ProLogo,
                                Cover = dsJP.ProCover,
                                Description = dsJP.ProDescription,
                                Config = dsJP.ProConfig,
                                CreationCode = dsJP.ProCreationCode,
                                CreationMessage = dsJP.ProCreationMessage,
                                CreatedAt = dsJP.ProCreatedAt,
                                CreatedBy = dsJP.ProCreatedBy,
                                UpdatedAt = dsJP.ProUpdatedAt,
                                StartDate = dsJP.ProStartDate,
                                EndDate = dsJP.ProEndDate,
                                Location = _locationService.GetLocationByProjectId(dsJP.ProId)
                            },
                            TotalAmountDonated = _donateService.GetTotalAmountDonatedBySessionId(dsJP.Id),
                            TotalDonations = _donateService.GetTotalDonationsBySessionId(dsJP.Id)
                        };
                    };
                }
            }
            catch (Exception e)
            {
                _logger.Error(e.Message);

                throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<DonationSessionResponse>
                {
                    ResultCode = ResultCode.SQL_ERROR_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.SQL_ERROR_CODE)
                });
            }
            //}

            if (response == null)
            {
                return new BaseResponse<DonationSessionResponse>
                {
                    ResultCode = ResultCode.DONATION_SESSION_NOT_FOUND_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.DONATION_SESSION_NOT_FOUND_CODE)
                };
            }

            return new BaseResponse<DonationSessionResponse>
            {
                ResultCode = ResultCode.SUCCESS_CODE,
                ResultMessage = ResultCode.GetMessage(ResultCode.SUCCESS_CODE),
                Data = response
            };
        }

        public BaseResponse<IEnumerable<DonationSessionResponse>> GetDonationSessionByProjectId(string projectId)
        {
            List<DonationSessionResponse> donationSessionResponses = new List<DonationSessionResponse>();

            //IEnumerable<string> keys = _redisService.GetKeysByPattern(PATTERN);

            //get from redis
            //foreach (string key in keys)
            //{
            //    DonationSessionResponse donationSession = JsonConvert.DeserializeObject<DonationSessionResponse>(_distributedCache.GetString(key));

            //    if (donationSession.ProjectId.Equals(projectId))
            //    {
            //        donationSessionResponses.Add(donationSession);
            //    }
            //}

            //get from db
            if (_utilService.IsNullOrEmpty(donationSessionResponses))
            {
                try
                {
                    using (var context = new WeSpreadCoreContext())
                    {

                        var donationSessionJoinPro =
                            from d in context.DonationSessions
                            join p in context.Projects
                            on d.ProjectId equals p.Id
                            where d.ProjectId == projectId
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
                                IsActive = p.IsActive,
                                ProEndDate = p.EndDate,
                                d.Id,
                                d.Name,
                                d.Target,
                                d.Description,
                                d.ProjectId,
                                d.CreatedBy,
                                d.CreatedAt,
                                d.UpdatedAt,
                                d.CreationCode,
                                d.CreationMessage,
                                d.Config,
                                d.StartDate,
                                d.EndDate
                            };

                        foreach (var dsJP in donationSessionJoinPro)
                        {
                            DonationSessionResponse response = new DonationSessionResponse
                            {
                                Id = dsJP.Id,
                                Name = dsJP.Name,
                                Description = dsJP.Description,
                                ProjectId = dsJP.ProjectId,
                                Target = dsJP.Target,
                                CreatedAt = dsJP.CreatedAt,
                                CreatedBy = dsJP.CreatedBy,
                                UpdatedAt = dsJP.UpdatedAt,
                                Config = dsJP.Config,
                                CreationCode = dsJP.CreationCode,
                                CreationMessage = dsJP.CreationMessage,
                                StartDate = dsJP.StartDate,
                                EndDate = dsJP.EndDate,
                                Project = new ProjectResponse
                                {
                                    Id = dsJP.ProId,
                                    Name = dsJP.ProName,
                                    EngName = dsJP.ProEngName,
                                    Logo = dsJP.ProLogo,
                                    Cover = dsJP.ProCover,
                                    Description = dsJP.ProDescription,
                                    Config = dsJP.ProConfig,
                                    CreationCode = dsJP.ProCreationCode,
                                    CreationMessage = dsJP.ProCreationMessage,
                                    CreatedAt = dsJP.ProCreatedAt,
                                    CreatedBy = dsJP.ProCreatedBy,
                                    UpdatedAt = dsJP.ProUpdatedAt,
                                    StartDate = dsJP.ProStartDate,
                                    EndDate = dsJP.ProEndDate,
                                    IsActive = dsJP.IsActive,
                                    Location = _locationService.GetLocationByProjectId(dsJP.ProId)
                                },
                                TotalAmountDonated = _donateService.GetTotalAmountDonatedBySessionId(dsJP.Id),
                                TotalDonations = _donateService.GetTotalDonationsBySessionId(dsJP.Id)
                            };

                            donationSessionResponses.Add(response);
                        };
                    }
                }
                catch (Exception e)
                {
                    _logger.Error(e.Message);

                    throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<DonationSessionResponse>
                    {
                        ResultCode = ResultCode.DONATION_SESSION_NOT_FOUND_CODE,
                        ResultMessage = ResultCode.GetMessage(ResultCode.DONATION_SESSION_NOT_FOUND_CODE)
                    });
                }
            }

            if (_utilService.IsNullOrEmpty(donationSessionResponses))
            {
                return new BaseResponse<IEnumerable<DonationSessionResponse>>
                {
                    ResultCode = ResultCode.DONATION_SESSION_NOT_FOUND_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.DONATION_SESSION_NOT_FOUND_CODE),
                    Data = default
                };
            }

            return new BaseResponse<IEnumerable<DonationSessionResponse>>
            {
                ResultCode = ResultCode.SUCCESS_CODE,
                ResultMessage = ResultCode.GetMessage(ResultCode.SUCCESS_CODE),
                Data = donationSessionResponses
            };
        }

        public BaseResponse<DonationSessionResponse> UpdateDonationSession(string id, string token, DonationSessionRequest donationSessionRequest)
        {
            //validate token
            string userId = _persistentLoginService.GetUserIDByToken(token);
            if (String.IsNullOrEmpty(userId))
            {
                _logger.Error($"User with token {token} is unauthorized.");

                throw new HttpStatusException(HttpStatusCode.Unauthorized, new BaseResponse<DonationSessionResponse>
                {
                    ResultCode = ResultCode.USER_UNAUTHORIZED_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.USER_UNAUTHORIZED_CODE)
                });
            }

            //validate role
            ProjectResponse project = _projectService.CheckRoleInOrgByProIdAndUserId(userId, donationSessionRequest.ProjectId, ORG_ADMIN);

            if (project == null)
            {
                _logger.Error($"User {userId} does not have permission to create donation session for project {donationSessionRequest.ProjectId}");

                throw new HttpStatusException(HttpStatusCode.Unauthorized, new BaseResponse<DonationSessionResponse>
                {
                    ResultCode = ResultCode.USER_UNAUTHORIZED_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.USER_UNAUTHORIZED_CODE)
                });
            }

            //validate donation session id
            DonationSession donationSessionModel;
            try
            {
                donationSessionModel = _unitOfWork.GetRepository<DonationSession>().Get(id);
            }
            catch (Exception e)
            {
                _logger.Error(e.Message);

                throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<DonationSessionResponse>
                {
                    ResultCode = ResultCode.DONATION_SESSION_NOT_FOUND_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.DONATION_SESSION_NOT_FOUND_CODE)
                });
            }

            //validate name
            if (!_validateDataService.IsValidName(donationSessionRequest.Name, null))
            {
                _logger.Error($"{donationSessionRequest.Name} is invalid name.");

                throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<DonationSessionResponse>
                {
                    ResultCode = ResultCode.INVALID_NAME_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.INVALID_NAME_CODE)
                });
            }

            //validate date
            Period proPeriod = _projectService.GetProjectPeriod(donationSessionRequest.ProjectId);

            if (_validateDataService.Is7DaysBeforeStartDate(DateTime.Now, proPeriod.StartDate))
            {
                if (_validateDataService.IsValidDateInProjectPeriod(proPeriod, donationSessionRequest.StartDate,
                    donationSessionRequest.EndDate))
                {
                    if (!_validateDataService.Is15DaysBefore(donationSessionRequest.StartDate, donationSessionRequest.EndDate))
                    {
                        _logger.Error($"Startdate {donationSessionRequest.StartDate} is not before enddate {donationSessionRequest.EndDate}");

                        throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<DonationSessionResponse>
                        {
                            ResultCode = ResultCode.INVALID_STARTTIME_ENDTIME_CODE,
                            ResultMessage = ResultCode.GetMessage(ResultCode.INVALID_STARTTIME_ENDTIME_CODE)
                        });
                    }
                }
                else
                {
                    _logger.Error($"Startdate {donationSessionRequest.StartDate} and enddate {donationSessionRequest.EndDate} is not in project period.");

                    throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<DonationSessionResponse>
                    {
                        ResultCode = ResultCode.INVALID_DATE_IN_PROJECT_PERIOD_CODE,
                        ResultMessage = ResultCode.GetMessage(ResultCode.INVALID_DATE_IN_PROJECT_PERIOD_CODE)
                    });
                }
            }
            else
            {
                _logger.Error($"Now is not before the project's start date {proPeriod.StartDate}");

                throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<DonationSessionResponse>
                {
                    ResultCode = ResultCode.INVALID_TIME_FOR_UPDATE_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.INVALID_TIME_FOR_UPDATE_CODE)
                });
            }


            //validate target
            if (!_validateDataService.IsValidDonationTarget(donationSessionRequest.Target))
            {
                _logger.Error($"Target '{donationSessionRequest.Target}' is not valid.");

                throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<DonationSessionResponse>
                {
                    ResultCode = ResultCode.INVALID_DONATION_TARGET_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.INVALID_DONATION_TARGET_CODE)
                });
            }

            //update data
            donationSessionModel = _mapper.Map(donationSessionRequest, donationSessionModel);
            donationSessionModel.UpdatedAt = DateTime.Now;
            donationSessionModel.CreationCode = ResultCode.UNVERIFIED_UPDATE_DONATION_SESSION_CODE;
            donationSessionModel.CreationMessage = ResultCode.GetMessage(ResultCode.UNVERIFIED_UPDATE_DONATION_SESSION_CODE);
            donationSessionModel.Checker = null;
            donationSessionModel.CheckerNavigation = null;

            try
            {
                _unitOfWork.GetRepository<DonationSession>().Update(donationSessionModel);

                _unitOfWork.Commit();
            }
            catch (Exception e)
            {
                _logger.Error(e.Message);

                throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<DonationSessionResponse>
                {
                    ResultCode = ResultCode.SQL_ERROR_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.SQL_ERROR_CODE)
                });
            }

            //create response
            DonationSessionResponse dnssResponse = _mapper.Map<DonationSessionResponse>(donationSessionModel);
            dnssResponse.Project = project;

            //store donation session Info to Redis
            string cacheKey = dnssResponse.Id;
            string cacheValue = _distributedCache.GetString(cacheKey);

            if (!string.IsNullOrEmpty(cacheValue))
            {
                _distributedCache.Remove(cacheKey);
            }

            _distributedCache.SetString(cacheKey, JsonConvert.SerializeObject(dnssResponse).ToString());

            return new BaseResponse<DonationSessionResponse>
            {
                ResultCode = ResultCode.SUCCESS_CODE,
                ResultMessage = ResultCode.GetMessage(ResultCode.SUCCESS_CODE),
                Data = dnssResponse
            };
        }

        public BaseResponse<DonationSessionResponse> DeleteDonationSession(string id, string token)
        {
            //validate token
            string userId = _persistentLoginService.GetUserIDByToken(token);
            if (String.IsNullOrEmpty(userId))
            {
                _logger.Error($"User with token {token} is unauthorized.");

                throw new HttpStatusException(HttpStatusCode.Unauthorized, new BaseResponse<DonationSessionResponse>
                {
                    ResultCode = ResultCode.USER_UNAUTHORIZED_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.USER_UNAUTHORIZED_CODE)
                });
            }

            //validate donation session id
            DonationSession donationSessionModel;
            try
            {
                donationSessionModel = _unitOfWork.GetRepository<DonationSession>().Get(id);
            }
            catch (Exception e)
            {
                _logger.Error(e.Message);

                throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<DonationSessionResponse>
                {
                    ResultCode = ResultCode.DONATION_SESSION_NOT_FOUND_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.DONATION_SESSION_NOT_FOUND_CODE)
                });
            }

            //validate role
            ProjectResponse project = _projectService.CheckRoleInOrgByProIdAndUserId(userId, donationSessionModel.ProjectId, ORG_ADMIN);

            if (project == null)
            {
                _logger.Error($"User {userId} does not have permission to create donation session for project {donationSessionModel.ProjectId}");

                throw new HttpStatusException(HttpStatusCode.Unauthorized, new BaseResponse<DonationSessionResponse>
                {
                    ResultCode = ResultCode.USER_UNAUTHORIZED_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.USER_UNAUTHORIZED_CODE)
                });
            }

            //delete donation session
            if (donationSessionModel.CreationCode == ResultCode.UNVERIFIED_CREATE_DONATION_SESSION_CODE)
            {
                donationSessionModel.CreationCode = ResultCode.DELETED_DONATION_SESSION_CODE;
                donationSessionModel.CreationMessage = ResultCode.GetMessage(ResultCode.DELETED_DONATION_SESSION_CODE);
            }
            else
            {
                donationSessionModel.CreationCode = ResultCode.UNVERIFIED_DELETE_DONATION_SESSION_CODE;
                donationSessionModel.CreationMessage = ResultCode.GetMessage(ResultCode.UNVERIFIED_DELETE_DONATION_SESSION_CODE);
                donationSessionModel.Checker = null;
                donationSessionModel.CheckerNavigation = null;
            }
            donationSessionModel.UpdatedAt = DateTime.Now;
            try
            {
                _unitOfWork.GetRepository<DonationSession>().Update(donationSessionModel);

                _unitOfWork.Commit();
            }
            catch (Exception e)
            {
                _logger.Error(e.Message);

                throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<DonationSessionResponse>
                {
                    ResultCode = ResultCode.SQL_ERROR_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.SQL_ERROR_CODE)
                });
            }

            //create response
            DonationSessionResponse dnssResponse = _mapper.Map<DonationSessionResponse>(donationSessionModel);

            //store donation session Info to Redis
            string cacheKey = dnssResponse.Id;
            string cacheValue = _distributedCache.GetString(cacheKey);

            if (!string.IsNullOrEmpty(cacheValue))
            {
                _distributedCache.Remove(cacheKey);
            }

            _distributedCache.SetString(cacheKey, JsonConvert.SerializeObject(dnssResponse).ToString());

            return new BaseResponse<DonationSessionResponse>
            {
                ResultCode = ResultCode.SUCCESS_CODE,
                ResultMessage = ResultCode.GetMessage(ResultCode.SUCCESS_CODE),
                Data = dnssResponse
            };
        }

        public BaseResponse<IEnumerable<DonationSessionResponse>> GetDonationSession()
        {
            List<DonationSessionResponse> donationSessionResponses = new List<DonationSessionResponse>();

            //IEnumerable<string> keys = _redisService.GetKeysByPattern(PATTERN);

            //get from redis
            //foreach (string key in keys)
            //{
            //    DonationSessionResponse donationSession = JsonConvert.DeserializeObject<DonationSessionResponse>(_distributedCache.GetString(key));

            //    donationSessionResponses.Add(donationSession);
            //}

            //get from db
            if (_utilService.IsNullOrEmpty(donationSessionResponses))
            {
                try
                {
                    using (var context = new WeSpreadCoreContext())
                    {

                        var donationSessionJoinPro =
                            from d in context.DonationSessions
                            join p in context.Projects
                            on d.ProjectId equals p.Id
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
                                d.Id,
                                d.Name,
                                d.Target,
                                d.Description,
                                d.ProjectId,
                                d.CreatedBy,
                                d.CreatedAt,
                                d.UpdatedAt,
                                d.CreationCode,
                                d.CreationMessage,
                                d.Config,
                                d.StartDate,
                                d.EndDate
                            };

                        foreach (var dsJP in donationSessionJoinPro)
                        {
                            DonationSessionResponse response = new DonationSessionResponse
                            {
                                Id = dsJP.Id,
                                Name = dsJP.Name,
                                Description = dsJP.Description,
                                ProjectId = dsJP.ProjectId,
                                Target = dsJP.Target,
                                CreatedAt = dsJP.CreatedAt,
                                CreatedBy = dsJP.CreatedBy,
                                UpdatedAt = dsJP.UpdatedAt,
                                Config = dsJP.Config,
                                CreationCode = dsJP.CreationCode,
                                CreationMessage = dsJP.CreationMessage,
                                StartDate = dsJP.StartDate,
                                EndDate = dsJP.EndDate,
                                Project = new ProjectResponse
                                {
                                    Id = dsJP.ProId,
                                    Name = dsJP.ProName,
                                    EngName = dsJP.ProEngName,
                                    Logo = dsJP.ProLogo,
                                    Cover = dsJP.ProCover,
                                    Description = dsJP.ProDescription,
                                    Config = dsJP.ProConfig,
                                    CreationCode = dsJP.ProCreationCode,
                                    CreationMessage = dsJP.ProCreationMessage,
                                    CreatedAt = dsJP.ProCreatedAt,
                                    CreatedBy = dsJP.ProCreatedBy,
                                    UpdatedAt = dsJP.ProUpdatedAt,
                                    StartDate = dsJP.ProStartDate,
                                    EndDate = dsJP.ProEndDate,
                                    IsActive = dsJP.IsActive,
                                    Location = _locationService.GetLocationByProjectId(dsJP.ProId)
                                },
                                TotalAmountDonated = _donateService.GetTotalAmountDonatedBySessionId(dsJP.Id),
                                TotalDonations = _donateService.GetTotalDonationsBySessionId(dsJP.Id)
                            };

                            donationSessionResponses.Add(response);
                        };
                    }
                }
                catch (Exception e)
                {
                    _logger.Error(e.Message);

                    throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<DonationSessionResponse>
                    {
                        ResultCode = ResultCode.DONATION_SESSION_NOT_FOUND_CODE,
                        ResultMessage = ResultCode.GetMessage(ResultCode.DONATION_SESSION_NOT_FOUND_CODE)
                    });
                }
            }

            if (_utilService.IsNullOrEmpty(donationSessionResponses))
            {
                return new BaseResponse<IEnumerable<DonationSessionResponse>>
                {
                    ResultCode = ResultCode.DONATION_SESSION_NOT_FOUND_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.DONATION_SESSION_NOT_FOUND_CODE),
                    Data = default
                };
            }

            return new BaseResponse<IEnumerable<DonationSessionResponse>>
            {
                ResultCode = ResultCode.SUCCESS_CODE,
                ResultMessage = ResultCode.GetMessage(ResultCode.SUCCESS_CODE),
                Data = donationSessionResponses
            };
        }
    }
}
