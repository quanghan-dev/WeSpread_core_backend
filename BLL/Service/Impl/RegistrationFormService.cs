using AutoMapper;
using BLL.Constant;
using BLL.Dto;
using BLL.Dto.Exception;
using BLL.Dto.Location;
using BLL.Dto.Member;
using BLL.Dto.RecruitmentSession;
using BLL.Dto.RegistrationForm;
using BLL.Dto.User;
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
    public class RegistrationFormService : IRegistrationFormService
    {
        private readonly ILogger _logger;
        private readonly IPersistentLoginService _persistentLoginService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRecruitmentSessionService _recruitmentSessionService;
        private readonly IMapper _mapper;
        private readonly IDistributedCache _distributedCache;
        private const string PREFIX = "RGF_";
        private const string ORG_ADMIN = "ORG_ADMIN";

        public RegistrationFormService(ILogger logger, IPersistentLoginService persistentLoginService,
            IUnitOfWork unitOfWork, IRecruitmentSessionService recruitmentSessionService, IMapper mapper,
            IDistributedCache distributedCache)
        {
            _logger = logger;
            _persistentLoginService = persistentLoginService;
            _unitOfWork = unitOfWork;
            _recruitmentSessionService = recruitmentSessionService;
            _mapper = mapper;
            _distributedCache = distributedCache;
        }

        public BaseResponse<RegistrationFormResponse> CancelRegistrationForm(string id, string token)
        {
            //validate token
            string userId = _persistentLoginService.GetUserIDByToken(token);
            if (String.IsNullOrEmpty(userId))
            {
                _logger.Error($"User with token {token} is unauthorized.");

                throw new HttpStatusException(HttpStatusCode.Unauthorized, new BaseResponse<RegistrationFormResponse>
                {
                    ResultCode = ResultCode.USER_UNAUTHORIZED_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.USER_UNAUTHORIZED_CODE),
                    Data = default
                });
            }

            RegistrationForm registrationForm;

            try
            {
                registrationForm = _unitOfWork.GetRepository<RegistrationForm>().Get(id);

                registrationForm.CreationCode = ResultCode.CANCEL_REGISTRATION_FORM_CODE;
                registrationForm.CreationMessage = ResultCode.GetMessage(ResultCode.CANCEL_REGISTRATION_FORM_CODE);
                registrationForm.UpdatedAt = DateTime.Now;

                _unitOfWork.GetRepository<RegistrationForm>().Update(registrationForm);

                _unitOfWork.Commit();
            }
            catch (Exception e)
            {
                _logger.Error("[RegistrationFormService] " + e.Message);

                throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<RegistrationFormResponse>
                {
                    ResultCode = ResultCode.SQL_ERROR_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.SQL_ERROR_CODE),
                    Data = default
                });
            }

            RegistrationFormResponse response = _mapper.Map<RegistrationFormResponse>(registrationForm);

            return new BaseResponse<RegistrationFormResponse>
            {
                ResultCode = ResultCode.SUCCESS_CODE,
                ResultMessage = ResultCode.GetMessage(ResultCode.SUCCESS_CODE),
                Data = response
            };
        }

        public BaseResponse<RegistrationFormResponse> CreateRegistrationForm(RegistrationFormRequest registrationFormRequest, string token)
        {
            //validate token
            string userId = _persistentLoginService.GetUserIDByToken(token);
            if (String.IsNullOrEmpty(userId))
            {
                _logger.Error($"User with token {token} is unauthorized.");

                throw new HttpStatusException(HttpStatusCode.Unauthorized, new BaseResponse<RegistrationFormResponse>
                {
                    ResultCode = ResultCode.USER_UNAUTHORIZED_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.USER_UNAUTHORIZED_CODE),
                    Data = default
                });
            }

            //validate session
            RecruitmentSessionResponse sessionResponse = _recruitmentSessionService.GetRecruitmentSessionById(registrationFormRequest.SessionId).Data;

            //store db
            RegistrationForm registrationForm = _mapper.Map<RegistrationForm>(registrationFormRequest);

            try
            {
                registrationForm.Id = PREFIX + ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds().ToString();
                registrationForm.CreationCode = ResultCode.UNVERIFIED_REGISTRATION_FORM_CODE;
                registrationForm.CreationMessage = ResultCode.GetMessage(ResultCode.UNVERIFIED_REGISTRATION_FORM_CODE);
                registrationForm.UserId = userId;
                registrationForm.CreatedAt = DateTime.Now;
                registrationForm.UpdatedAt = DateTime.Now;

                _unitOfWork.GetRepository<RegistrationForm>().Add(registrationForm);

                _unitOfWork.Commit();
            }
            catch (Exception e)
            {
                _logger.Error("[RegistrationFormService] " + e.Message);

                throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<RegistrationFormResponse>
                {
                    ResultCode = ResultCode.SQL_ERROR_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.SQL_ERROR_CODE),
                    Data = default
                });
            }

            //response
            RegistrationFormResponse response = _mapper.Map<RegistrationFormResponse>(registrationForm);

            response.Session = sessionResponse;

            string cacheKey = response.Id;
            if (_distributedCache.GetString(cacheKey) != null)
            {
                _distributedCache.Remove(cacheKey);
            }
            _distributedCache.SetString(cacheKey, JsonConvert.SerializeObject(response));

            return new BaseResponse<RegistrationFormResponse>
            {
                ResultCode = ResultCode.SUCCESS_CODE,
                ResultMessage = ResultCode.GetMessage(ResultCode.SUCCESS_CODE),
                Data = response
            };
        }

        public BaseResponse<IEnumerable<RegistrationFormResponse>> GetRegistrationFormByUser(string token)
        {
            List<RegistrationFormResponse> responses = new List<RegistrationFormResponse>();

            //validate token
            string userId = _persistentLoginService.GetUserIDByToken(token);
            if (String.IsNullOrEmpty(userId))
            {
                _logger.Error($"User with token {token} is unauthorized.");

                throw new HttpStatusException(HttpStatusCode.Unauthorized, new BaseResponse<RegistrationFormResponse>
                {
                    ResultCode = ResultCode.USER_UNAUTHORIZED_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.USER_UNAUTHORIZED_CODE),
                    Data = default
                });
            }

            //get data
            List<RegistrationFormResponse> history = new List<RegistrationFormResponse>();

            try
            {
                using (var context = new WeSpreadCoreContext())
                {
                    var registrationJoinSessions = context.RegistrationForms.Join(
                        context.RecruitmentSessions,
                        rg => rg.SessionId,
                        ss => ss.Id,
                        (rg, ss) => new
                        {
                            Id = rg.Id,
                            UserId = rg.UserId,
                            Address = rg.Address,
                            ApplyLocationId = rg.ApplyLocationId,
                            SessionId = rg.SessionId,
                            CreationCode = rg.CreationCode,
                            CreationMessage = rg.CreationMessage,
                            CreatedAt = rg.CreatedAt,
                            UpdatedAt = rg.UpdatedAt,
                            StartDate = ss.StartDate,
                            EndDate = ss.EndDate,
                            Benefit = ss.Benefit,
                            Name = ss.Name,
                            Description = ss.Description,
                            ProjectId = ss.ProjectId,
                            SessionCreationCode = ss.CreationCode,
                            SessionCreationMessage = ss.CreationMessage,
                            Config = ss.Config,
                            SessionCreatedAt = ss.CreatedAt,
                            SessionUpdatedAt = ss.UpdatedAt,
                            CreatedBy = ss.CreatedBy,
                            Quota = ss.Quota,
                            Requirement = ss.Requirement,
                            JobDescription = ss.JobDescription
                        });

                    if(registrationJoinSessions != null)
                    {
                        foreach (var registrationJoinSession in registrationJoinSessions)
                        {
                            RegistrationFormResponse registrationFormResponse = new RegistrationFormResponse
                            {
                                Id = registrationJoinSession.Id,
                                UserId = registrationJoinSession.UserId,
                                Address = registrationJoinSession.Address,
                                ApplyLocationId = registrationJoinSession.ApplyLocationId,
                                SessionId = registrationJoinSession.SessionId,
                                CreationCode = registrationJoinSession.CreationCode,
                                CreationMessage = registrationJoinSession.CreationMessage,
                                CreatedAt = registrationJoinSession.CreatedAt,
                                UpdatedAt = registrationJoinSession.UpdatedAt,
                                Session = new RecruitmentSessionResponse
                                {
                                    Id = registrationJoinSession.SessionId,
                                    StartDate = registrationJoinSession.StartDate,
                                    EndDate = registrationJoinSession.EndDate,
                                    Benefit = registrationJoinSession.Benefit,
                                    Name = registrationJoinSession.Name,
                                    Description = registrationJoinSession.Description,
                                    ProjectId = registrationJoinSession.ProjectId,
                                    CreationCode = registrationJoinSession.SessionCreationCode,
                                    CreationMessage = registrationJoinSession.SessionCreationMessage,
                                    Config = registrationJoinSession.Config,
                                    CreatedAt = registrationJoinSession.SessionCreatedAt,
                                    UpdatedAt = registrationJoinSession.SessionUpdatedAt,
                                    CreatedBy = registrationJoinSession.CreatedBy,
                                    Quota = registrationJoinSession.Quota,
                                    Requirement = registrationJoinSession.Requirement,
                                    JobDescription = registrationJoinSession.JobDescription
                                }
                            };

                            history.Add(registrationFormResponse);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Error("[RegistrationFormService] " + e.Message);

                throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<RegistrationFormResponse>
                {
                    ResultCode = ResultCode.SQL_ERROR_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.SQL_ERROR_CODE),
                    Data = default
                });
            }

            history.Sort((x, y) => x.CreatedAt.CompareTo(y.CreatedAt));

            return new BaseResponse<IEnumerable<RegistrationFormResponse>>
            {
                ResultCode = ResultCode.SUCCESS_CODE,
                ResultMessage = ResultCode.GetMessage(ResultCode.SUCCESS_CODE),
                Data = history
            };
        }

        public BaseResponse<IEnumerable<RegistrationFormResponse>> GetRegistrationFormBySessionIdAndOrgAdmin(string sessonId, string token)
        {
            //validate token
            string userId = _persistentLoginService.GetUserIDByToken(token);
            if (String.IsNullOrEmpty(userId))
            {
                _logger.Error($"User with token {token} is unauthorized.");

                throw new HttpStatusException(HttpStatusCode.Unauthorized, new BaseResponse<RegistrationFormResponse>
                {
                    ResultCode = ResultCode.USER_UNAUTHORIZED_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.USER_UNAUTHORIZED_CODE),
                    Data = default
                });
            }

            //check role
            if (!CheckOrgRoleByRecruitmentSession(sessonId, userId))
            {
                _logger.Error("[RegistrationForm] User does not have permission to make this request");

                throw new HttpStatusException(HttpStatusCode.Unauthorized, new BaseResponse<RegistrationFormResponse>
                {
                    ResultCode = ResultCode.USER_UNAUTHORIZED_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.USER_UNAUTHORIZED_CODE),
                    Data = default
                });
            }

            //get data
            List<RegistrationForm> registrationForms;

            try
            {
                registrationForms = _unitOfWork.GetRepository<RegistrationForm>()
                    .GetAll()
                    .Where(r => r.SessionId.Equals(sessonId)).ToList();
            }
            catch (Exception e)
            {
                _logger.Error("[RegistrationForm] " + e.Message);

                throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<RegistrationFormResponse>
                {
                    ResultCode = ResultCode.SQL_ERROR_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.SQL_ERROR_CODE),
                    Data = default
                });
            }

            List<RegistrationFormResponse> responses = _mapper.Map<List<RegistrationFormResponse>>(registrationForms);

            responses.Sort((x, y) => x.CreatedAt.CompareTo(y.CreatedAt));

            return new BaseResponse<IEnumerable<RegistrationFormResponse>>
            {
                ResultCode = ResultCode.SUCCESS_CODE,
                ResultMessage = ResultCode.GetMessage(ResultCode.SUCCESS_CODE),
                Data = responses
            };
        }

        public bool CheckOrgRoleByRecruitmentSession(string recruitmentSessionId, string userId)
        {
            try
            {
                using (var context = new WeSpreadCoreContext())
                {
                    var roleIdObj = from r in context.RecruitmentSessions
                                    where r.Id == recruitmentSessionId
                                    join p in context.Projects
                                    on r.ProjectId equals p.Id
                                    join uo in context.UserOrganizations
                                    on p.OrgId equals uo.OrganizationId
                                    where uo.UserId == userId
                                    select new
                                    {
                                        RoleId = uo.RoleId
                                    };

                    foreach(var r in roleIdObj)
                    {
                        if (r != null && r.RoleId.Equals(ORG_ADMIN))
                            return true;
                        else
                        {
                            _logger.Error("[CheckOrgRoleByRecruitmentSession] err");

                            throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<RegistrationFormResponse>
                            {
                                ResultCode = ResultCode.SQL_ERROR_CODE,
                                ResultMessage = ResultCode.GetMessage(ResultCode.SQL_ERROR_CODE),
                                Data = default
                            });
                        }
                    };
                }
            }
            catch (Exception e)
            {
                _logger.Error("[CheckOrgRoleByRecruitmentSession]" + e.Message);

                throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<RegistrationFormResponse>
                {
                    ResultCode = ResultCode.SQL_ERROR_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.SQL_ERROR_CODE),
                    Data = default
                });
            }
            return false;
        }

        public BaseResponse<RegistrationFormResponse> ApproveForm(string formId, string token)
        {
            //validate token
            string userId = _persistentLoginService.GetUserIDByToken(token);
            if (String.IsNullOrEmpty(userId))
            {
                _logger.Error($"User with token {token} is unauthorized.");

                throw new HttpStatusException(HttpStatusCode.Unauthorized, new BaseResponse<RegistrationFormResponse>
                {
                    ResultCode = ResultCode.USER_UNAUTHORIZED_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.USER_UNAUTHORIZED_CODE),
                    Data = default
                });
            }

            RegistrationForm registrationForm;

            //get data
            try
            {
                registrationForm = _unitOfWork.GetRepository<RegistrationForm>().Get(formId);
            }
            catch (Exception e)
            {
                _logger.Error("[RegistrationForm] " + e.Message);

                throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<RegistrationFormResponse>
                {
                    ResultCode = ResultCode.SQL_ERROR_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.SQL_ERROR_CODE),
                    Data = default
                });
            }

            //check role
            if (!CheckOrgRoleByRecruitmentSession(registrationForm.SessionId, userId))
            {
                _logger.Error("[RegistrationForm] User does not have permission to make this request");

                throw new HttpStatusException(HttpStatusCode.Unauthorized, new BaseResponse<RegistrationFormResponse>
                {
                    ResultCode = ResultCode.USER_UNAUTHORIZED_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.USER_UNAUTHORIZED_CODE),
                    Data = default
                });
            }

            //approve form
            try
            {
                registrationForm.CreationCode = ResultCode.SUCCESS_CODE;
                registrationForm.CreationMessage = ResultCode.GetMessage(ResultCode.SUCCESS_CODE);
                registrationForm.CheckerId = userId;
                registrationForm.UpdatedAt = DateTime.Now;

                _unitOfWork.GetRepository<RegistrationForm>().Update(registrationForm);

                _unitOfWork.Commit();
            }
            catch (Exception e)
            {
                _logger.Error("[RegistrationForm] " + e.Message);

                throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<RegistrationFormResponse>
                {
                    ResultCode = ResultCode.SQL_ERROR_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.SQL_ERROR_CODE),
                    Data = default
                });
            }

            return new BaseResponse<RegistrationFormResponse>
            {
                ResultCode = ResultCode.SUCCESS_CODE,
                ResultMessage = ResultCode.GetMessage(ResultCode.SUCCESS_CODE),
                Data = _mapper.Map<RegistrationFormResponse>(registrationForm)
            };
        }

        public BaseResponse<RegistrationFormResponse> RejectForm(string formId, string token)
        {
            //validate token
            string userId = _persistentLoginService.GetUserIDByToken(token);
            if (String.IsNullOrEmpty(userId))
            {
                _logger.Error($"User with token {token} is unauthorized.");

                throw new HttpStatusException(HttpStatusCode.Unauthorized, new BaseResponse<RegistrationFormResponse>
                {
                    ResultCode = ResultCode.USER_UNAUTHORIZED_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.USER_UNAUTHORIZED_CODE),
                    Data = default
                });
            }

            RegistrationForm registrationForm;

            //get data
            try
            {
                registrationForm = _unitOfWork.GetRepository<RegistrationForm>().Get(formId);
            }
            catch (Exception e)
            {
                _logger.Error("[RegistrationForm] " + e.Message);

                throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<RegistrationFormResponse>
                {
                    ResultCode = ResultCode.SQL_ERROR_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.SQL_ERROR_CODE),
                    Data = default
                });
            }

            //check role
            if (!CheckOrgRoleByRecruitmentSession(registrationForm.SessionId, userId))
            {
                _logger.Error("[RegistrationForm] User does not have permission to make this request");

                throw new HttpStatusException(HttpStatusCode.Unauthorized, new BaseResponse<RegistrationFormResponse>
                {
                    ResultCode = ResultCode.USER_UNAUTHORIZED_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.USER_UNAUTHORIZED_CODE),
                    Data = default
                });
            }

            //reject form
            try
            {
                registrationForm.CreationCode = ResultCode.REJECT_REGISTRATION_FORM_CODE;
                registrationForm.CreationMessage = ResultCode.GetMessage(ResultCode.REJECT_REGISTRATION_FORM_CODE);
                registrationForm.CheckerId = userId;
                registrationForm.UpdatedAt = DateTime.Now;

                _unitOfWork.GetRepository<RegistrationForm>().Update(registrationForm);

                _unitOfWork.Commit();
            }
            catch (Exception e)
            {
                _logger.Error("[RegistrationForm] " + e.Message);

                throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<RegistrationFormResponse>
                {
                    ResultCode = ResultCode.SQL_ERROR_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.SQL_ERROR_CODE),
                    Data = default
                });
            }

            return new BaseResponse<RegistrationFormResponse>
            {
                ResultCode = ResultCode.SUCCESS_CODE,
                ResultMessage = ResultCode.GetMessage(ResultCode.SUCCESS_CODE),
                Data = _mapper.Map<RegistrationFormResponse>(registrationForm)
            };
        }

        public BaseResponse<IEnumerable<MemberApply>> GetMemberApplyBySession(string id)
        {
            List<MemberApply> memberApplies = new List<MemberApply>();

            try
            {
                using(var context = new WeSpreadCoreContext())
                {
                    var members = from u in context.AppUsers
                                  join rf in context.RegistrationForms
                                  on u.Id equals rf.UserId
                                  where rf.SessionId == id
                                  join l in context.Locations
                                  on rf.ApplyLocationId equals l.Id
                                  select new
                                  {
                                      UserId = u.Id,
                                      FirstName = u.FirstName,
                                      LastName = u.LastName,
                                      Gender = u.Gender,
                                      Email = u.Email,
                                      NumberPhone = u.NumberPhone,
                                      Birthday = u.Birthday,
                                      IsActive = u.IsActive,
                                      CreatedAt = u.CreatedAt,
                                      UpdatedAt = u.UpdatedAt,
                                      RoleId = u.RoleId,
                                      IsBlock = u.IsBlock,
                                      LocationId = l.Id,
                                      Name = l.Name,
                                      Address = l.Address,
                                      Latitude = l.Latitude,
                                      Longitude = l.Longitude
                                  };

                    foreach(var member in members)
                    {
                        memberApplies.Add(new MemberApply
                        {
                            User = new AppUserResponse
                            {
                                Id = member.UserId,
                                LastName = member.LastName,
                                FirstName = member.FirstName,
                                Gender = member.Gender,
                                Email = member.Email,
                                Birthday = member.Birthday,
                                IsActive = member.IsActive,
                                CreatedAt = member.CreatedAt,
                                UpdatedAt = member.UpdatedAt,
                                RoleId = member.RoleId,
                                IsBlock = member.IsBlock,
                                NumberPhone = member.NumberPhone
                            },
                            ApplyLocation = new LocationResponse
                            {
                                Id = member.LocationId,
                                Name = member.Name,
                                Address = member.Address,
                                Latitude = member.Latitude,
                                Longitude = member.Longitude
                            }
                        });
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Error("[GetMemberApplyBySessionId] " + e.Message);

                throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<DonateDetail>
                {
                    ResultCode = ResultCode.SQL_ERROR_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.SQL_ERROR_CODE)
                });
            }

            return new BaseResponse<IEnumerable<MemberApply>>
            {
                ResultCode = ResultCode.SUCCESS_CODE,
                ResultMessage = ResultCode.GetMessage(ResultCode.SUCCESS_CODE),
                Data = memberApplies
            };
        }
    }
}
