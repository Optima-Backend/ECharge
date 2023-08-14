using System.Reflection;

namespace ECharge.Infrastructure.Services.CibPay.Certificate.Api;

public class CertificatePath
{
    public string CurrentPath { get; }

    public CertificatePath()
    {
        var certificatePath = "ECharge.Infrastructure/Services/CibPay/Certificate/Api/taxiapp.p12";

        var root = Directory.GetCurrentDirectory();
        
        var currentDirectory = Path.Combine(root[..^8] ,certificatePath);
        
        CurrentPath = currentDirectory;
    }
}