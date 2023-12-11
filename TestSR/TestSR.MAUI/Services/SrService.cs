using Microsoft.AspNetCore.Http.Connections.Client;
using Microsoft.AspNetCore.SignalR.Client;
using System.Security.Cryptography.X509Certificates;
using static TestSR.MAUI.Handlers;


namespace TestSR.MAUI.Services;


public enum KeyedPolicies
{
    MyPolicy
}

public class SrService()
{
    enum HubMethods
    {
        ReceiveNotification,
        UpdateTime
    }

    private HubConnection? _hubConnection;

    public static X509Certificate2Collection GetClientCertificates(string subject)
    {
        X509Certificate2 cert = new(
           fileName: "E:\\certs\\u1.crt");

        X509Certificate2Collection col = [cert];
        return col;
    }

    public async Task ConnectToHub()
    {
        try
        {
            if (_hubConnection == null)
            {
                _hubConnection = new HubConnectionBuilder()
                .WithUrl(Opt.Url, (options) =>
                {
                    options = Opt.ConnectionOptions;

                    options.ClientCertificates = GetClientCertificates("dd");
                    var handler = new HttpClientHandler
                    {
                        ClientCertificateOptions = ClientCertificateOption.Manual,
                        ServerCertificateCustomValidationCallback = (httpRequestMessage, cert, cetChain, policyErrors) => true
                    };
                    options.HttpMessageHandlerFactory = _ => handler;
                    options.WebSocketConfiguration = sockets =>
                    {
                        sockets.RemoteCertificateValidationCallback = (sender, certificate, chain, policyErrors) => true;
                    };

                })

                .WithAutomaticReconnect(Opt.RetryPolicy)
                .Build();



                _hubConnection.On<string>(HubMethods.ReceiveNotification.ToString(), message =>
                {
                    Opt.Events.ReceiveNotificationHandler(message);
                });

                _hubConnection.On<DateTime>(HubMethods.UpdateTime.ToString(), time =>
                {
                    Opt.Events.ReceiveTimenHandler(time);
                });
                _hubConnection.Closed += HubConnection_Closed;
                _hubConnection.Reconnected += HubConnection_Reconnected;
                _hubConnection.HandshakeTimeout = TimeSpan.FromSeconds(60);

                bool _isConnected = false;
                while (!_isConnected)
                {
                    try
                    {
                        await _hubConnection.StartAsync();
                        _isConnected = true;
                        break;
                    }
                    catch (Exception ex)
                    {
                        Opt.Events.ExceptionHandler(ex);
                    }
                    await Task.Delay(TimeSpan.FromSeconds(5));
                }
            }
        }
        catch (Exception ex)
        {
            Opt.Events.ExceptionHandler(ex);
        }
    }

    //public async Task ConnectToHub()
    //{
    //    try
    //    {
    //        if (_hubConnection == null)
    //        {
    //            _hubConnection = new HubConnectionBuilder()
    //            .WithUrl(Opt.Url, (options) =>
    //            {
    //                options = Opt.ConnectionOptions;
    //            })
    //            .WithAutomaticReconnect(Opt.RetryPolicy)
    //            .Build();

    //            _hubConnection.On<string>(HubMethods.ReceiveNotification.ToString(), message =>
    //            {
    //                Opt.Events.ReceiveNotificationHandler(message);
    //            });

    //            _hubConnection.On<DateTime>(HubMethods.UpdateTime.ToString(), time =>
    //            {
    //                Opt.Events.ReceiveTimenHandler(time);
    //            });
    //            _hubConnection.Closed += HubConnection_Closed;
    //            _hubConnection.Reconnected += HubConnection_Reconnected;

    //            bool _isConnected = false;
    //            while (!_isConnected)
    //            {
    //                try
    //                {
    //                    await _hubConnection.StartAsync();
    //                    _isConnected = true;
    //                    break;
    //                }
    //                catch (Exception ex)
    //                {
    //                    Opt.Events.ExceptionHandler(ex);
    //                }
    //                await Task.Delay(TimeSpan.FromSeconds(5));
    //            }
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        Opt.Events.ExceptionHandler(ex);
    //    }
    //}

    private Task HubConnection_Reconnected(string? arg)
    {
        Opt.Events.ReconnectedHandler();
        return Task.CompletedTask;
    }

    private Task HubConnection_Closed(Exception? arg)
    {
        Opt.Events.ConnectionClosedHandler(arg);
        return Task.CompletedTask;
    }

    public SrServiceOptions Opt { get; set; } = new();

}


public class SrServiceOptions
{
    public string Url { get; set; } = string.Empty;

    public IRetryPolicy RetryPolicy { get; set; } = new MyRetryPolicy();

    public HttpConnectionOptions ConnectionOptions { get; set; } = new();
    public SrServiceEvents Events { get; set; } = new();
}

public class SrServiceEvents
{
    public ExceptionHandler ExceptionHandler { get; set; } = (_) => { };
    public ReceiveNotificationHandler ReceiveNotificationHandler { get; set; } = (_) => { };
    public ReceiveTimeHandler ReceiveTimenHandler { get; set; } = (_) => { };
    public ConnectionClosedHandler ConnectionClosedHandler { get; set; } = (_) => { };
    public ReconnectedHandler ReconnectedHandler { get; set; } = () => { };
}


public class MyRetryPolicy() : IMyRetryPolicy
{
    private TimeSpan _retryInterval = TimeSpan.FromSeconds(2);

    public RetryHandler? RetryHandler { get; set; }

    public TimeSpan? NextRetryDelay(RetryContext retryContext)
    {
        if (RetryHandler is not null) { RetryHandler(retryContext); }
        return _retryInterval;
    }
}

public static class MyRetryPolicyExtensions
{
    public static IMyRetryPolicy HandleReconnectionAttempt(this IMyRetryPolicy policy, RetryHandler? retryHandler)
    {
        policy.RetryHandler = retryHandler;
        return policy;
    }
}

public interface IMyRetryPolicy : IRetryPolicy
{
    RetryHandler? RetryHandler { get; set; }
}

public static class SrServiceExtensions
{
    public static SrService ConfigureService(this SrService srService,
      Action<SrServiceOptions> configureOptions)
    {
        configureOptions(srService.Opt);
        return srService;
    }

    //public static SrService WithUrl(this SrService srService, string url = "https://localhost:7293/notifications")
    //{
    //    srService.Url = url;
    //    return srService;
    //}

    //public static SrService WithConnectionOptions(this SrService srService,
    //    SrServiceOptions options, HttpConnectionOptions httpConnectionOptions)
    //{
    //    srService.ConnectionOptions = httpConnectionOptions;
    //    return srService;
    //}

    //public static SrService OnError(this SrService srService, ExceptionHandler handler)
    //{
    //    srService.ExceptionHandler = handler;
    //    return srService;
    //}

    //public static SrService OnReceiveTime(this SrService srService, ReceiveTimeHandler handler)
    //{
    //    srService.ReceiveTimenHandler = handler;
    //    return srService;
    //}

    //public static SrService OnReceiveMessage(this SrService srService, ReceiveNotificationHandler handler)
    //{
    //    srService.ReceiveNotificationHandler = handler;
    //    return srService;
    //}

    //public static SrService OnConnectionClosed(this SrService srService, ConnectionClosedHandler handler)
    //{
    //    srService.ConnectionClosedHandler = handler;
    //    return srService;
    //}

    //public static SrService OnReconnected(this SrService srService, ReconnectedHandler handler)
    //{
    //    srService.ReconnectedHandler = handler;
    //    return srService;
    //}
}
