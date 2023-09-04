using System.Security.Claims;

namespace ECharge.Domain.JWT.Interface
{
    public interface IJwtService
    {
        string GenerateJwtToken(string role);
        bool ValidateJwtToken(string token);
        ClaimsPrincipal GetClaimsPrincipal(string token);
    }
}

