using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Connections.Features;
using Microsoft.AspNetCore.SignalR;

namespace TestSR.API;

[Authorize]
public class ChatHub : Hub<IChatClient>
{
    public override async Task OnConnectedAsync()
    {
        var httpCtx = Context.GetHttpContext();
        IHttpContextFeature feature = (IHttpContextFeature)this.Context.Features[typeof(IHttpContextFeature)];
        HttpContext cntx = feature.HttpContext;

        if (httpCtx.Connection.ClientCertificate is null)
        {
            var result = await httpCtx.Connection.GetClientCertificateAsync();
        }
        //await Clients.Client(Context.ConnectionId).ReceiveNotification(message: $"Thank you for connecting {Context.User?.Identity?.Name}");
        await Clients.All.ReceiveNotification(message: $"Thank you for connecting {Context.User?.Identity?.Name}");
        await base.OnConnectedAsync();
    }



}

public interface IChatClient
{
    Task ReceiveNotification(string message);

    Task UpdateTime(DateTime dateTime);
}
