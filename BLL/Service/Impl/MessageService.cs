using BLL.Dto;
using Microsoft.Extensions.Caching.Distributed;
using System;
using Twilio.Clients;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using BLL.Constant;
using BLL.Dto.Exception;
using System.Net;

namespace BLL.Service.Impl
{
    public class MessageService : IMessageService
    {
        private readonly ITwilioRestClient _client;
        private readonly IDistributedCache _distributedCache;
        private readonly ILogger _logger;
        private readonly IUtilService _utilService;
        private readonly IValidateDataService _validateDataService;

        public MessageService(ITwilioRestClient client, IDistributedCache distributedCache, ILogger logger,
            IUtilService utilService, IValidateDataService validateDataService)
        {
            _client = client;
            _distributedCache = distributedCache;
            _logger = logger;
            _utilService = utilService;
            _validateDataService = validateDataService;
        }

        public BaseResponse<MessageResponse> SendSMS(string phone)
        {
            //validate phone
            if (!_validateDataService.IsValidPhone(phone))
            {
                _logger.Error($"User enter wrong phone format: {phone}");

                throw new HttpStatusException(HttpStatusCode.OK, new BaseResponse<MessageResponse>
                {
                    ResultCode = ResultCode.INVALID_PHONE_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.INVALID_PHONE_CODE),
                    Data = default
                });
            }

            string vietNamPhone = _utilService.changeToVietnamPhoneNumber(phone);

            string from = "+12019034591";
            string otp = _utilService.GenerateOTP();
            string message = "Vui long nhap ma OTP " + otp + " de xac thuc. " +
                "Vi ly do bao mat, ma OTP se het han sau 3 phut. Ban khong nen chia se " +
                "ma OTP voi bat ky ai.";

            string to = _utilService.changeToInternationalPhoneNumber(vietNamPhone);

            MessageResponse messageResponse = new MessageResponse(_utilService.changeToVietnamPhoneNumber(to), from, message);
            try
            {
                SaveOTPToRedis(otp, messageResponse.To);
                var messageSend = MessageResource.Create(
                to: new PhoneNumber(to),
                from: new PhoneNumber(from),
                body: message,
                client: _client);
                messageResponse.Sid = messageSend.Sid;
            }
            catch (Exception e)
            {
                _logger.Error(e.Message);

                throw new HttpStatusException(HttpStatusCode.Unauthorized, new BaseResponse<MessageResponse>
                {
                    ResultCode = ResultCode.MESSAGE_NOT_SENT_CODE,
                    ResultMessage = ResultCode.GetMessage(ResultCode.MESSAGE_NOT_SENT_CODE),
                    Data = default
                });
            }

            return new BaseResponse<MessageResponse>
            {
                ResultCode = ResultCode.SUCCESS_CODE,
                ResultMessage = ResultCode.GetMessage(ResultCode.SUCCESS_CODE),
                Data = messageResponse
            };
        }

        public void SaveOTPToRedis(string otp, string phone)
        {
            string cacheKey = "OTP_" + phone;
            string cacheOTPValue = _distributedCache.GetString(cacheKey);

            if (string.IsNullOrEmpty(cacheOTPValue))
            {
                var options = new DistributedCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(TimeUnit.THREE_MINUTES));

                _distributedCache.SetString(cacheKey, otp, options);
            }
        }
    }
}
