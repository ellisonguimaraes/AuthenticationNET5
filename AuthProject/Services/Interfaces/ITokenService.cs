using System.Security.Claims;
using System.Collections.Generic;
using AuthProject.Models;

namespace AuthProject.Services.Interfaces
{
    public interface ITokenService
    {
        string GenerateAccessToken(User user);
        string GenerateRefreshToken();
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
    }
}