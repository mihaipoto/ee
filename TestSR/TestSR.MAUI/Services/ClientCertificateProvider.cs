using System.Security.Cryptography.X509Certificates;

namespace TestSR.MAUI.Services;

public interface IClientCertificateProvider
{
    X509Certificate2Collection GetClientCertificates(string subject);
}




public class ClientCertificateProvider : IClientCertificateProvider
{


    //public X509Certificate2Collection GetClientCertificates(string subject)
    //{
    //    var localMachineStore = new X509Store(StoreLocation.LocalMachine);
    //    localMachineStore.Open(OpenFlags.ReadOnly);
    //    var certificates = localMachineStore.Certificates.Find(findType: X509FindType.FindBySubjectName,
    //                                                           findValue: subject,
    //                                                           validOnly: false) ?? [];
    //    localMachineStore.Close();
    //    return certificates;
    //}

    public X509Certificate2Collection GetClientCertificates(string subject)
    {
        X509Certificate2 cert = new(
           fileName: "E:\\certs\\u1.pfx", "12345");

        X509Certificate2Collection col = [cert];
        return col;
    }



}
