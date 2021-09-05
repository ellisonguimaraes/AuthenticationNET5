using System.Security.Cryptography;
using System;
using System.Text;
using System.Collections.Generic;
using System.Security.Claims;
using AuthProject.Models.Configuration;
using AuthProject.Services.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;

namespace AuthProject.Services
{
    public class TokenService : ITokenService
    {
        private readonly TokenConfiguration _configuration;

        public TokenService(TokenConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateAccessToken(IEnumerable<Claim> claims)
        {
            
            // Pegando a Secret do appsettings.json
            SymmetricSecurityKey secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.Secret));
            
            // Definindo signincredentials
            SigningCredentials signingCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

            // Configurando a descrição do token
            var tokenDescriptor = new SecurityTokenDescriptor{
                Subject = new ClaimsIdentity(claims),
                Audience = _configuration.Audience,
                Issuer = _configuration.Issuer,
                Expires = DateTime.UtcNow.AddMinutes(_configuration.Minutes),
                SigningCredentials = signingCredentials
            };

            // Gerando token
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            
            // Retornando o token em string
            return tokenHandler.WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using(var rng = RandomNumberGenerator.Create()){
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            // Configurando os parameters ~ Contains a set of parameters that are used by a SecurityTokenHandler when validating a SecurityToken.
            TokenValidationParameters tokenValidationParameters = new TokenValidationParameters{
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.Secret)),
                ValidateLifetime = false
            };

            // Obtendo o SecurityToken e o Principal
            SecurityToken securityToken;

            ClaimsPrincipal principal =  new JwtSecurityTokenHandler().ValidateToken(
                token, 
                tokenValidationParameters,
                out securityToken
            );

            // Convertendo o SecurityToken para JwTSecurityToken e verificando
            var jwtSecurityToken = securityToken as JwtSecurityToken;

            if ( jwtSecurityToken == null || 
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, 
                                                    StringComparison.InvariantCulture))
                throw new SecurityTokenException("Invalid Token");

            // Retornando o principal
            return principal;
        }
    }
}