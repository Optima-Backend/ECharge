using System.Reflection;

namespace ECharge.Infrastructure.Services.CibPay.Certificate.Api;

public class CertificatePath
{
    public string CurrentPath { get; }

    public CertificatePath()
    {
        var assemblyFolder = Assembly.GetExecutingAssembly().Location;
        var certificatePath = Path.Combine(Path.GetDirectoryName(assemblyFolder), "Services/CibPay/Certificate/Api/taxiapp.p12");

        CurrentPath = certificatePath;
    }
}