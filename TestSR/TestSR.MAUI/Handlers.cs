using Microsoft.AspNetCore.SignalR.Client;

namespace TestSR.MAUI;

public static class Handlers
{
    public delegate void ExceptionHandler(Exception ex);

    public delegate void ReceiveNotificationHandler(string message);

    public delegate void ReceiveTimeHandler(DateTime time);

    public delegate void ConnectionClosedHandler(Exception? exception);

    public delegate void ReconnectedHandler();

    public delegate void RetryHandler(RetryContext context);


}
