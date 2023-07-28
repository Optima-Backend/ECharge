using System.Security.Claims;
using ECharge.Domain.JWT.DTOs;

namespace ECharge.Domain.JWT.Interface
{
    public interface IJwtService
    {
        string GenerateJwtToken(GenerateTokenModel tokenModel);
        bool ValidateJwtToken(string token);
        ClaimsPrincipal GetClaimsPrincipal(string token);
    }
}

