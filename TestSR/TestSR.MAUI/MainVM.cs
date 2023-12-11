using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using TestSR.MAUI.Services;

namespace TestSR.MAUI;

public partial class MainVM(
    SrService srService, ILogger<MainVM> logger,
    [FromKeyedServices(KeyedPolicies.MyPolicy)] IMyRetryPolicy policy,
    IClientCertificateProvider clientCertificateProvider,
    IHttpClientFactory httpClientFactory) : ObservableObject
{
    public ObservableCollection<string> Messages { get; set; } = [];

    [ObservableProperty]
    private DateTime _currentTime = DateTime.MinValue;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Reason))]
    [NotifyPropertyChangedFor(nameof(TimeSinceDisconnection))]
    [NotifyPropertyChangedFor(nameof(NoOfRetryes))]
    RetryContext? _retryContext;

    public string Reason => RetryContext?.RetryReason?.Message ?? "No retry reason";
    public string TimeSinceDisconnection => RetryContext?.ElapsedTime.ToString() ?? string.Empty;
    public string NoOfRetryes => RetryContext?.PreviousRetryCount.ToString() ?? string.Empty;

    public async Task PageLoaded()
    {
        //var httpClient = httpClientFactory.CreateClient("namedClient");
        var handler = new HttpClientHandler();
        handler.ClientCertificateOptions = ClientCertificateOption.Manual;
        handler.SslProtocols = SslProtocols.Tls12;
        handler.ClientCertificates.Add(clientCertificateProvider.GetClientCertificates(subject: "c1.com")[0]);
        HttpClient httpClient = new(handler);
        var httpResponseMessage = await httpClient.GetAsync("https://localhost:5555/weather");

        if (httpResponseMessage.IsSuccessStatusCode)
        {
            var resp = JsonDocument.Parse(
                await httpResponseMessage.Content.ReadAsStringAsync());
        }

        throw new ApplicationException($"Status code: {httpResponseMessage.StatusCode}");
        //X509Certificate2Collection certificates = clientCertificateProvider.GetClientCertificates(subject: "c1.com");

        //if (certificates.Count > 0)
        //{
        //    policy.HandleReconnectionAttempt((context) => RetryContext = context);
        //    await srService.ConfigureService((options) =>
        //    {
        //        options.Url = "https://localhost:5555/notifications";
        //        options.RetryPolicy = policy;
        //        options.ConnectionOptions = new()
        //        {
        //            //AccessTokenProvider = async () => await Task.FromResult<string>("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6Im1paGFpIiwic3ViIjoibWloYWkiLCJqdGkiOiJkY2EzODUxOSIsImF1ZCI6WyJodHRwOi8vbG9jYWxob3N0OjI0MDEiLCJodHRwczovL2xvY2FsaG9zdDo0NDMzMCIsImh0dHA6Ly9sb2NhbGhvc3Q6NTAxNyIsImh0dHBzOi8vbG9jYWxob3N0OjcyOTMiXSwibmJmIjoxNzAwOTg5MTY5LCJleHAiOjE3MDg5Mzc5NjksImlhdCI6MTcwMDk4OTE3MCwiaXNzIjoiZG90bmV0LXVzZXItand0cyJ9.zUSCGE6P9t_XvqRfoUsf8oniTpM_b62nGJaMV3l8e-M")
        //            ClientCertificates = certificates,
        //            HttpMessageHandlerFactory = (msg) =>
        //            {
        //                if (msg is HttpClientHandler clientHandler)
        //                {
        //                    // bypass SSL certificate
        //                    clientHandler.ServerCertificateCustomValidationCallback +=
        //                        (sender, certificate, chain, sslPolicyErrors) => { return true; };
        //                }

        //                return msg;
        //            }
        //        };
        //        options.Events = new SrServiceEvents()
        //        {
        //            ConnectionClosedHandler = (ex) => Messages.Add(ex?.Message ?? "Connection closed"),
        //            ExceptionHandler = (ex) => Messages.Add(ex?.ToString() ?? "Eroare"),
        //            ReceiveNotificationHandler = (message) => Messages.Add(message),
        //            ReceiveTimenHandler = (newTime) => CurrentTime = newTime,
        //            ReconnectedHandler = () => { Messages.Add("Reconnected"); RetryContext = null; }
        //        };
        //    })
        //    .ConnectToHub();
        //}
        //else
        //{
        //    //nu au fost gasite certificate client
        //}



    }

    static bool MySslCertificateValidationCallback(object sender, X509Certificate? certificate, X509Chain? chain, SslPolicyErrors sslPolicyErrors)
    {
        // If there are no errors, then everything went smoothly.
        if (sslPolicyErrors == SslPolicyErrors.None)
            return true;

        // Note: MailKit will always pass the host name string as the `sender` argument.
        var host = (string)sender;

        if ((sslPolicyErrors & SslPolicyErrors.RemoteCertificateNotAvailable) != 0)
        {
            // This means that the remote certificate is unavailable. Notify the user and return false.
            Console.WriteLine("The SSL certificate was not available for {0}", host);
            return false;
        }

        if ((sslPolicyErrors & SslPolicyErrors.RemoteCertificateNameMismatch) != 0)
        {
            // This means that the server's SSL certificate did not match the host name that we are trying to connect to.
            var certificate2 = certificate as X509Certificate2;
            var cn = certificate2 != null ? certificate2.GetNameInfo(X509NameType.SimpleName, false) : certificate.Subject;

            Console.WriteLine("The Common Name for the SSL certificate did not match {0}. Instead, it was {1}.", host, cn);
            return false;
        }

        // The only other errors left are chain errors.
        Console.WriteLine("The SSL certificate for the server could not be validated for the following reasons:");

        // The first element's certificate will be the server's SSL certificate (and will match the `certificate` argument)
        // while the last element in the chain will typically either be the Root Certificate Authority's certificate -or- it
        // will be a non-authoritative self-signed certificate that the server admin created. 
        foreach (var element in chain.ChainElements)
        {
            // Each element in the chain will have its own status list. If the status list is empty, it means that the
            // certificate itself did not contain any errors.
            if (element.ChainElementStatus.Length == 0)
                continue;

            Console.WriteLine("\u2022 {0}", element.Certificate.Subject);
            foreach (var error in element.ChainElementStatus)
            {
                // `error.StatusInformation` contains a human-readable error string while `error.Status` is the corresponding enum value.
                Console.WriteLine("\t\u2022 {0}", error.StatusInformation);
            }
        }

        return false;
    }






}
