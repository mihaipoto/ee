using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Text.Encodings.Web;

namespace TestSR.API;

public class CertificateValidationService : ICertificateValidationService
{
    private readonly string[] validThumbprints = new[]
    {
        "141594A0AE38CBBECED7AF680F7945CD51D8F28A",
        "0C89639E4E2998A93E423F919B36D4009A0F9991",
        "BA9BF91ED35538A01375EFC212A2F46104B33A44"
    };

    public bool ValidateCertificate(X509Certificate2 clientCertificate)
        => validThumbprints.Contains(clientCertificate.Thumbprint);
}

public interface ICertificateValidationService
{
    bool ValidateCertificate(X509Certificate2 clientCertificate);
}

public class ClientCertificateAuthenticationOptions : AuthenticationSchemeOptions
{
}


public class ClientCertificateAuthenticationHandler : AuthenticationHandler<ClientCertificateAuthenticationOptions>
{


    public ClientCertificateAuthenticationHandler(IOptionsMonitor<ClientCertificateAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock) : base(options, logger, encoder, clock)
    {
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        //var cert = await Request.HttpContext.Connection.GetClientCertificateAsync();
        //object cert = null;
        //if (!Request.Environment.TryGetValue("ssl.ClientCertificate", out cert) ||
        //   !(cert is X509Certificate2))
        //{
        //    s_logger.WarnFormat("Hub {0} called without certificate or cookie", hub.Context.Request.ToString());
        //    throw new Exception("not authenticated");
        //}


        //if (cert == null)
        //{
        //    return Task.FromResult<AuthenticationTicket>(null);
        //}

        //try
        //{
        //    Options.Validator.Validate(cert);
        //}
        //catch
        //{
        //    return Task.FromResult<AuthenticationTicket>(null);
        //}
        //return null;
        var claims = new[]
                    {
                    new Claim(
                        ClaimTypes.NameIdentifier,
                        "aa",
                        ClaimValueTypes.String, "bb"),
                    new Claim(
                        ClaimTypes.Name,
                        "bb",
                        ClaimValueTypes.String, "cc")
                    };

        var principal = new ClaimsPrincipal(
            new ClaimsIdentity(claims, Scheme.Name));
        var ticket = new AuthenticationTicket(principal, Scheme.Name);
        var r = AuthenticateResult.Success(ticket);
        return r;
    }
    //protected override Task<AuthenticationTicket> AuthenticateCoreAsync()
    //{
    //    var cert = Context.Get<X509Certificate>("ssl.ClientCertificate");

    //    if (cert == null)
    //    {
    //        return Task.FromResult<AuthenticationTicket>(null);
    //    }

    //    try
    //    {
    //        Options.Validator.Validate(cert);
    //    }
    //    catch
    //    {
    //        return Task.FromResult<AuthenticationTicket>(null);
    //    }
    //    return null;
    //}
}