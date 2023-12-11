using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hosting;

namespace TestSR.API;

public class MyConnection : PersistentConnection
{
    protected override Task OnReceived(IRequest request, string connectionId, string data)
    {
        return Connection.Broadcast("Server Received: " + data);
    }

    protected override bool AuthorizeRequest(IRequest request)
    {
        INameValueCollection headers = request.Headers;

        foreach (KeyValuePair<string, string> entry in headers)
        {
            Console.WriteLine("Key: {0}, Value: {1}", entry.Key, entry.Value);
            Console.WriteLine("");
        }

        return true;
    }
}
