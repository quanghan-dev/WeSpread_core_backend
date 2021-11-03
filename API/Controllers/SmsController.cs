using BLL.Constant;
using BLL.Dto;
using BLL.Service;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;

namespace API.Controllers
{
    [EnableCors("MyPolicy")]
    [ApiController]
    [Route("core/api/sms")]
    public class SmsController : ControllerBase
    {
        private readonly IMessageService _messageService;
        private readonly ILogger _logger;

        public SmsController(IMessageService messageService, ILogger logger)
        {
            _messageService = messageService;
            _logger = logger;
        }

        [HttpPost("sendotp")]
        public IActionResult SendOTP([FromForm] string numberPhone)
        {


            _logger.Information($"POST core/api/sms/sendOTP START Request: {numberPhone}");

            Stopwatch watch = new Stopwatch();
            watch.Start();

            BaseResponse<MessageResponse> messageResponse = _messageService.SendSMS(numberPhone);

            string json = JsonConvert.SerializeObject(messageResponse);

            watch.Stop();

            _logger.Information("POST core/api/sms/sendOTP END duration: " +
                $"{watch.ElapsedMilliseconds} ms-----------Response: " + json);

            if (messageResponse.ResultCode == ResultCode.MESSAGE_NOT_SENT_CODE)
            {
                return NotFound(messageResponse);
            }

            return Ok(json);
        }
    }
}
