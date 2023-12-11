using Microsoft.AspNetCore.Authentication.Certificate;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using System.Net;
using System.Net.Security;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using TestSR.API;

var builder = WebApplication.CreateBuilder(args);

ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback(ValidateCertificate);


static bool ValidateCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
{
    return true;
}


builder.Services.AddSignalR(opt =>
{

});

builder.Services.AddHostedService<ServerTimeNotifierService>();


builder.Services.AddAuthorization(options =>
{
    //options.FallbackPolicy = new AuthorizationPolicyBuilder()
    //    .Build();
});
builder.Services.AddAuthentication(CertificateAuthenticationDefaults.AuthenticationScheme)
    .AddCertificate(options =>
    {

        options.ValidateCertificateUse = false;
        options.AllowedCertificateTypes = CertificateTypes.All;
        options.Events = new CertificateAuthenticationEvents
        {

            OnCertificateValidated = context =>
                {
                    var validationService = context.HttpContext.RequestServices
                        .GetRequiredService<ICertificateValidationService>();

                    if (validationService.ValidateCertificate(context.ClientCertificate))
                    {
                        var claims = new[]
                    {
                    new Claim(
                        ClaimTypes.NameIdentifier,
                        context.ClientCertificate.Subject,
                        ClaimValueTypes.String, context.Options.ClaimsIssuer),
                    new Claim(
                        ClaimTypes.Name,
                        context.ClientCertificate.Subject,
                        ClaimValueTypes.String, context.Options.ClaimsIssuer)
                    };

                        context.Principal = new ClaimsPrincipal(
                            new ClaimsIdentity(claims, context.Scheme.Name));
                        context.Success();
                    }
                    return Task.CompletedTask;
                },
            OnAuthenticationFailed = context =>
            {
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                var cer = context.HttpContext.Connection.ClientCertificate;
                //Console.WriteLine(context.Request.ToString());
                return Task.CompletedTask;

            }


        };
    });

//builder.Services.AddAuthentication("Basic")
//    .AddScheme<ClientCertificateAuthenticationOptions, ClientCertificateAuthenticationHandler>("Basic", null);

builder.Logging.AddConsole();


builder.Services.Configure<KestrelServerOptions>(options =>
{
    //var localMachineStore = new X509Store(StoreLocation.LocalMachine);
    //localMachineStore.Open(OpenFlags.ReadOnly);
    //var certificates = localMachineStore.Certificates.Find(findType: X509FindType.FindBySubjectName,
    //                                                       findValue: "c2.com",
    //                                                       validOnly: false) ?? [];
    //localMachineStore.Close();

    options.Listen(IPAddress.Loopback, 5555, listenOptions =>
    {
        listenOptions.UseHttps("E:\\certs\\server.pfx", "12345");

        listenOptions.UseConnectionLogging();

    });
    options.ConfigureHttpsDefaults(options =>
    {
        options.OnAuthenticate = (context, auth) =>
        {
            auth.ClientCertificateRequired = true;
            auth.CertificateRevocationCheckMode = X509RevocationMode.NoCheck;


        };
        options.ClientCertificateMode = ClientCertificateMode.DelayCertificate;
        options.ClientCertificateValidation = (cert, chain, policyErrors) =>
            {
                // Certificate validation logic here
                // Return true if the certificate is valid or false if it is invalid
                return true;
            };



    });


});

//builder.WebHost.UseKestrel(options =>
//{
//    options.AddServerHeader = false;
//    options.Listen(IPAddress.Loopback, 5555, listenOptions =>
//    {
//        listenOptions.UseHttps("E:\\certs\\server.pfx", "12345");
//    });
//})
//           .ConfigureKestrel(o =>
//           {
//               o.ConfigureHttpsDefaults(o =>
//               o.ClientCertificateMode =
//               ClientCertificateMode.RequireCertificate);
//           })
//           .UseUrls("https://localhost:5555");



var app = builder.Build();
app.UseCertificateForwarding();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};


app.MapGet("/weather", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
}).RequireAuthorization();


app.UseAuthentication();
app.UseAuthorization();
app.MapHub<ChatHub>("notifications", options =>
{

});

app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}



