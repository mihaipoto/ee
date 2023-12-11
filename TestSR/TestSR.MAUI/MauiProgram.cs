using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography.X509Certificates;
using TestSR.MAUI.Services;

namespace TestSR.MAUI;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        var clientCertificate =
        new X509Certificate2(
      "E:\\certs\\u1.pfx", "12345");

        builder.Services.AddHttpClient("namedClient", c =>
        {

        }).ConfigurePrimaryHttpMessageHandler(() =>
        {
            var handler = new HttpClientHandler();
            handler.ClientCertificates.Add(clientCertificate);
            return handler;
        });
        //builder.Services.AddHttpClient(c =>
        //{
        //    c.ConfigurePrimaryHttpMessageHandler(c =>
        //    {
        //        var handler = new HttpClientHandler();
        //        handler.ClientCertificates.Add(SrService.GetClientCertificates("dd")[0]);
        //        return handler;
        //    });

        //});
        builder.Services.AddSingletonWithShellRoute<MainPage, MainVM>(nameof(MainPage));
        builder.Services.AddSingleton<SrService>();
        builder.Services.AddKeyedSingleton<IMyRetryPolicy, MyRetryPolicy>(KeyedPolicies.MyPolicy);
        builder.Services.AddSingleton<IClientCertificateProvider, ClientCertificateProvider>();

        return builder.Build();
    }
}
