using AutoMapper;
using BLL.Constant;
using BLL.Dto;
using BLL.Dto.Donate;
using BLL.Dto.DonationSession;
using BLL.Dto.Exception;
using BLL.Dto.Member;
using BLL.Dto.Payment.MoMo.CaptureWallet;
using BLL.Dto.User;
using DAL.Model;
using DAL.UnifOfWork;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace BLL.Service.Impl
{
    public class DonateService : IDonateService
    {
        private readonly ILogger _logger;
        private readonly IMoMoService _moMoService;
        private readonly IPersistentLoginService _persistentLoginService;
        private readonly IOrganizationService _organizationService;
        private readonly IUtilService _utilService;
        private readonly ISecurityService _securityService;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDistributedCache _distributedCache;
        private readonly IValidateDataService _validateDataService;
        private const string MOMO = "momo";
        private const string ORG_ADMIN = "ORG_ADMIN";
        private const string PREFIX = "Dnt_";

        public DonateService(ILogger logger, IMoMoService moMoService, IPersistentLoginService persistentLoginService,
            IOrganizationService organizationService, IUtilService utilService, ISecurityService securityService,
            IConfiguration configuration, IMapper mapper, IUnitOfWork unitOfWork,
            IDistributedCache distributedCache, IValidateDataService validateDataService)
        {
            _logger = logger;
            _moMoService = moMoService;
            _persistentLoginService = persistentLoginService;
            _organizationService = organizationService;
            _utilService = utilService;
            _securityService = securityService;
            _configuration = configuration;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _distributedCache = distributedCache;
            _validateDataService = validateDataService;
        }
        public BaseResponse<DonateLinkResponse> CreateLinkDonate(DonateRequest request, string token)
        {
            //validate token
            string userId = _persistentLoginService.GetUserIDByToken(token);
            if (String.IsNullOrEmpty(userId))
            {
                _logger.Error($"User with token {token} is unauthorized.");

                throw new HttpStatusException(HttpStatusCode.Unauthorized, new BaseResponse<DonateLinkResponse>
                {
                    ResultCode = ResultCode.USER_UNAUTHORIZED_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.USER_UNAUTHORIZED_CODE),
                    Data = default
                });
            }

            //check role donator
            if (request.DonatorId.StartsWith("Org"))
            {
                if (!_organizationService.CheckUserRoleByOrgId(userId, request.DonatorId, ORG_ADMIN))
                {
                    throw new HttpStatusException(HttpStatusCode.Unauthorized, new BaseResponse<DonateLinkResponse>
                    {
                        ResultCode = ResultCode.USER_UNAUTHORIZED_CODE,
                        ResultMessage = ResultCode.GetMessage(ResultCode.USER_UNAUTHORIZED_CODE),
                        Data = default
                    });
                }
            }

            //check amount limit
            if (!_validateDataService.IsValidDonateAmount(request.Amount))
            {
                _logger.Error($"[DonateDetail] Unvalid donate amount: {request.Amount}");

                throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<DonateDetail>
                {
                    ResultCode = ResultCode.INVALID_DONATE_DETAIL_AMOUNT_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.INVALID_DONATE_DETAIL_AMOUNT_CODE)
                });
            }

            //check remaining target
            //if (!CheckRemainingTarget(request.SessionId, request.Amount))
            //{
            //    _logger.Error($"Donate {request.Amount} is over session's target.");

            //    throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<DonateLinkResponse>
            //    {
            //        ResultCode = ResultCode.OVER_TARGET_DONATE_DETAIL_CODE,
            //        ResultMessage = ResultCode.GetMessage(ResultCode.OVER_TARGET_DONATE_DETAIL_CODE),
            //        Data = default
            //    });
            //}

            DonateLinkResponse donateLinkResponse = null;

            string orderId = PREFIX + _utilService.Create16Alphanumeric();

            //if paytype is MoMo
            if (request.PayType.Equals(MOMO))
            {
                MoMoCaptureWalletRequest momoRequest = new MoMoCaptureWalletRequest
                {
                    partnerCode = _configuration.GetValue<string>("MoMo:PartnerCode"),
                    requestId = ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds().ToString(),
                    amount = request.Amount,
                    orderId = orderId,
                    orderInfo = request.Message,
                    redirectUrl = request.RedirectUrl,
                    ipnUrl = "http://171.244.143.202:5000/core/api/ipn/momo",
                    requestType = "captureWallet",
                    extraData = ""
                };

                // Validate signature
                List<string> ignoreFields = new List<string>() { "signature", "partnerName", "storeId", "lang" };

                string rawData = _securityService.GetRawDataSignature(momoRequest, ignoreFields);

                rawData = "accessKey=" + _configuration.GetValue<string>("MoMo:AccessKey") + "&" + rawData;

                string merchantSignature = _securityService.SignHmacSHA256(rawData, _configuration.GetValue<string>("MoMo:SecretKey"));

                momoRequest.signature = merchantSignature;

                MoMoCaptureWalletResponse momoResponse = _moMoService.CreateCaptureWallet(momoRequest);

                donateLinkResponse = new DonateLinkResponse
                {
                    Deeplink = momoResponse.deeplink,
                    PayUrl = momoResponse.payUrl
                };
            }

            //store donate detail to DB
            DonateDetail donateDetail = _mapper.Map<DonateDetail>(request);
            try
            {
                donateDetail.Id = orderId;
                donateDetail.UserId = userId;
                donateDetail.CreatedAt = DateTime.Now;
                donateDetail.UpdatedAt = DateTime.Now;
                donateDetail.TransId = "";
                donateDetail.DonateTime = DateTime.Now;
                donateDetail.ResultCode = ResultCode.UNVERIFIED_DONATE_DETAIL_CODE;
                donateDetail.ResultMessage = ResultCode.GetMessage(ResultCode.UNVERIFIED_DONATE_DETAIL_CODE);

                _unitOfWork.GetRepository<DonateDetail>().Add(donateDetail);

                _unitOfWork.Commit();
            }
            catch (Exception e)
            {
                _logger.Error("[DonateService] " + e.Message);

                throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<DonateDetail>
                {
                    ResultCode = ResultCode.SQL_ERROR_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.SQL_ERROR_CODE)
                });
            }

            //store connectionId to Redis
            string cacheKey = donateDetail.Id;
            if (_distributedCache.GetString(cacheKey) != null)
            {
                _distributedCache.Remove(cacheKey);
            }

            //SignalRHubService signalRHubService = new SignalRHubService();
            //_distributedCache.SetString(cacheKey, signalRHubService.GetConnectionId());

            return new BaseResponse<DonateLinkResponse>
            {
                ResultCode = ResultCode.SUCCESS_CODE,
                ResultMessage = ResultCode.GetMessage(ResultCode.SUCCESS_CODE),
                Data = donateLinkResponse
            };
        }

        public BaseResponse<IEnumerable<DonateResultResponse>> HistoryDonate(string token)
        {
            //validate token
            string userId = _persistentLoginService.GetUserIDByToken(token);
            if (String.IsNullOrEmpty(userId))
            {
                _logger.Error($"User with token {token} is unauthorized.");

                throw new HttpStatusException(HttpStatusCode.Unauthorized, new BaseResponse<DonateLinkResponse>
                {
                    ResultCode = ResultCode.USER_UNAUTHORIZED_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.USER_UNAUTHORIZED_CODE),
                    Data = default
                });
            }

            List<DonateResultResponse> history = new List<DonateResultResponse>();
            //get data
            try
            {
                using (var context = new WeSpreadCoreContext())
                {
                    var donateJoinSessions = context.DonateDetails.Join(
                        context.DonationSessions,
                        dd => dd.SessionId,
                        dss => dss.Id,
                        (dd, dss) => new
                        {
                            Id = dd.Id,
                            UserId = dd.UserId,
                            DonatorId = dd.DonatorId,
                            TransId = dd.TransId,
                            DonatorType = dd.DonatorType,
                            Amount = dd.Amount,
                            Message = dd.Message,
                            PayType = dd.PayType,
                            DonateTime = dd.DonateTime,
                            ResultCode = dd.ResultCode,
                            ResultMessage = dd.ResultMessage,
                            CreatedAt = dd.CreatedAt,
                            UpdatedAt = dd.UpdatedAt,
                            IsIncognito = dd.IsIncognito,
                            SessionId = dss.Id,
                            StartDate = dss.StartDate,
                            EndDate = dss.EndDate,
                            Target = dss.Target,
                            Name = dss.Name,
                            Description = dss.Description,
                            ProjectId = dss.ProjectId,
                            CreationCode = dss.CreationCode,
                            CreationMessage = dss.CreationMessage,
                            Config = dss.Config,
                            SessionCreatedAt = dss.CreatedAt,
                            SessionUpdatedAt = dss.UpdatedAt,
                            CreatedBy = dss.CreatedBy
                        }).ToList();

                    foreach (var donateJoinSession in donateJoinSessions)
                    {
                        DonateResultResponse donateResult = new DonateResultResponse
                        {
                            Id = donateJoinSession.Id,
                            UserId = donateJoinSession.UserId,
                            DonatorId = donateJoinSession.DonatorId,
                            TransId = donateJoinSession.TransId,
                            DonatorType = donateJoinSession.DonatorType,
                            Amount = donateJoinSession.Amount,
                            Message = donateJoinSession.Message,
                            PayType = donateJoinSession.PayType,
                            DonateTime = donateJoinSession.DonateTime,
                            ResultCode = donateJoinSession.ResultCode,
                            ResultMessage = donateJoinSession.ResultMessage,
                            CreatedAt = donateJoinSession.CreatedAt,
                            UpdatedAt = donateJoinSession.UpdatedAt,
                            IsIncognito = donateJoinSession.IsIncognito,
                            Session = new DonationSessionResponse
                            {
                                Id = donateJoinSession.Id,
                                StartDate = donateJoinSession.StartDate,
                                EndDate = donateJoinSession.EndDate,
                                Target = donateJoinSession.Target,
                                Name = donateJoinSession.Name,
                                Description = donateJoinSession.Description,
                                ProjectId = donateJoinSession.ProjectId,
                                CreationCode = donateJoinSession.CreationCode,
                                CreationMessage = donateJoinSession.CreationMessage,
                                Config = donateJoinSession.Config,
                                CreatedAt = donateJoinSession.SessionCreatedAt,
                                UpdatedAt = donateJoinSession.SessionUpdatedAt,
                                CreatedBy = donateJoinSession.CreatedBy
                            }
                        };

                        history.Add(donateResult);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Error("[DonateService] " + e.Message);

                throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<DonateDetail>
                {
                    ResultCode = ResultCode.SQL_ERROR_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.SQL_ERROR_CODE)
                });
            }

            history.Sort((x, y) => x.CreatedAt.CompareTo(y.CreatedAt));

            return new BaseResponse<IEnumerable<DonateResultResponse>>
            {
                ResultCode = ResultCode.SUCCESS_CODE,
                ResultMessage = ResultCode.GetMessage(ResultCode.SUCCESS_CODE),
                Data = history
            };
        }

        public bool CheckRemainingTarget(string sessionId, long amount)
        {
            try
            {
                using (var context = new WeSpreadCoreContext())
                {
                    double sessionTarget = context.DonationSessions.Where(ds => ds.Id == sessionId).First().Target - context.DonateDetails.Where(dd => dd.SessionId == sessionId).Sum(dd => dd.Amount);

                    if (sessionTarget >= amount)
                    {
                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Error("[DonateService] " + e.Message);

                throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<DonateDetail>
                {
                    ResultCode = ResultCode.SQL_ERROR_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.SQL_ERROR_CODE)
                });
            }
            return false;
        }

        public double GetTotalAmountDonatedBySessionId(string sessionId)
        {
            try
            {
                double totalAmount = _unitOfWork.GetRepository<DonateDetail>()
                    .GetAll().Where(dd => dd.SessionId == sessionId)
                    .Sum(dd => dd.Amount);

                return totalAmount;
            }
            catch (Exception e)
            {
                _logger.Error("[GetTotalAmountDonatedBySessionId] " + e.Message);

                throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<DonateDetail>
                {
                    ResultCode = ResultCode.SQL_ERROR_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.SQL_ERROR_CODE)
                });
            }
        }

        public double GetTotalDonationsBySessionId(string sessionId)
        {
            try
            {
                double totalDonations = _unitOfWork.GetRepository<DonateDetail>()
                    .GetAll().Where(dd => dd.SessionId == sessionId)
                    .Count();

                return totalDonations;
            }
            catch (Exception e)
            {
                _logger.Error("[GetTotalDonationsBySessionId] " + e.Message);

                throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<DonateDetail>
                {
                    ResultCode = ResultCode.SQL_ERROR_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.SQL_ERROR_CODE)
                });
            }
        }

        public BaseResponse<IEnumerable<MemberDonate>> GetMemberDonateBySession(string sessionId)
        {
            List<MemberDonate> memberDonates = new List<MemberDonate>();

            try
            {
                using (var context = new WeSpreadCoreContext())
                {
                    var members = from u in context.AppUsers
                                  join dd in context.DonateDetails
                                  on u.Id equals dd.UserId
                                  where dd.SessionId == sessionId
                                  orderby dd.DonateTime
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
                                      Iscognito = dd.IsIncognito,
                                      Amount = dd.Amount
                                  };

                    foreach (var member in members)
                    {
                        memberDonates.Add(new MemberDonate
                        {
                            IsIncognito = member.Iscognito,
                            AmountDonated = member.Amount,
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
                            }
                        });
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Error("[GetMemberDonateBySession] " + e.Message);

                throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<DonateDetail>
                {
                    ResultCode = ResultCode.SQL_ERROR_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.SQL_ERROR_CODE)
                });
            }

            return new BaseResponse<IEnumerable<MemberDonate>>
            {
                ResultCode = ResultCode.SUCCESS_CODE,
                ResultMessage = ResultCode.GetMessage(ResultCode.SUCCESS_CODE),
                Data = memberDonates
            };
        }
    }
}
