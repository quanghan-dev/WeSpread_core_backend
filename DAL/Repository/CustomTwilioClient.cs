using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using Twilio.Clients;
using Twilio.Http;

namespace DAL.Repository
{
    public class CustomTwilioClient : ITwilioRestClient
    {
        private readonly ITwilioRestClient _innerClient;

        public CustomTwilioClient(IConfiguration config, System.Net.Http.HttpClient httpClient)
        {
            // customize the underlying HttpClient
            httpClient.DefaultRequestHeaders.Add("X-Custom-Header", "CustomTwilioRestClient-Demo");

            _innerClient = new TwilioRestClient(
                config["Twilio:AccountSid"],
                config["Twilio:AuthToken"],
                httpClient: new SystemNetHttpClient(httpClient));
        }

        public string AccountSid => _innerClient.AccountSid;

        public string Region => _innerClient.Region;

        public HttpClient HttpClient => _innerClient.HttpClient;

        public Response Request(Request request) => _innerClient.Request(request);

        public Task<Response> RequestAsync(Request request) => _innerClient.RequestAsync(request);
    }
}
