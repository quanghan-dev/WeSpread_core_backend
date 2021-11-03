using BLL.Dto;

namespace BLL.Service
{
    public interface IMessageService
    {
        BaseResponse<MessageResponse> SendSMS(string phone);
        void SaveOTPToRedis(string otp, string phone);
    }
}
