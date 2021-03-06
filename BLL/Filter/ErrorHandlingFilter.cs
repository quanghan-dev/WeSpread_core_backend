using BLL.Dto.Exception;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;

namespace BLL.Filter
{
    public class ErrorHandlingFilter : ExceptionFilterAttribute
    {
        public override void OnException(ExceptionContext context)
        {
            if(context.Exception is HttpStatusException statusException)
            {
                var response = new HttpResponseMessage(statusException.Status)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(statusException.Response))
                };
                context.Result = new HttpResponseMessageResult(response);
                base.OnException(context);
            }
        }

    }

    public class HttpResponseMessageResult : IActionResult
    {
        private readonly HttpResponseMessage _responseMessage;

        public HttpResponseMessageResult(HttpResponseMessage responseMessage)
        {
            _responseMessage = responseMessage;
        }

        public async Task ExecuteResultAsync(ActionContext context)
        {
            context.HttpContext.Response.StatusCode = (int)_responseMessage.StatusCode;
            using (var stream = await _responseMessage.Content.ReadAsStreamAsync())
            {
                await stream.CopyToAsync(context.HttpContext.Response.Body);
                await context.HttpContext.Response.Body.FlushAsync();
            }
        }
    }

}
