using BLL.Dto;
using BLL.Dto.Donate;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace BLL.SignalRHub
{
    public class SignalRHubService : Hub
    {
        public SignalRHubService()
        {
        }
        public async Task SendPaymentResponseToClient(BaseResponse<DonateResultResponse> response, string connectionId)
        {
            await Clients.Client(connectionId).SendAsync("MomoPaymentResult", response);
        }

        public string GetConnectionId() => Context.ConnectionId;

        //public override Task OnConnected()
        //{
        //    string name = Context.User.Identity.Name;
        //    _connections.Add(name, Context.ConnectionId);
        //    return base.OnConnected();
        //}

        //public override Task OnDisconnected(bool stopCalled)
        //{
        //    string name = Context.User.Identity.Name;
        //    _connections.Remove(name, Context.ConnectionId);
        //    return base.OnDisconnected(stopCalled);
        //}
    }
}
