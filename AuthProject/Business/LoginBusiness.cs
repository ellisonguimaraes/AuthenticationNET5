using System.Security.AccessControl;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AuthProject.Business.Interfaces;
using AuthProject.Models;
using AuthProject.Models.Configuration;
using AuthProject.Models.DTO;
using AuthProject.Repository.Interfaces;
using AuthProject.Services.Interfaces;

namespace AuthProject.Business
{
    public class LoginBusiness : ILoginBusiness
    {
        private const string DATE_FORMAT = "yyyy-MM-dd HH:mm:ss"; // Data no formato armazenado
        private readonly TokenConfiguration _configuration;
        private readonly IUserRepository _repository;
        private readonly ITokenService _tokenService;

        public LoginBusiness(TokenConfiguration configuration, IUserRepository repository, ITokenService tokenService)
        {
            _configuration = configuration;
            _repository = repository;
            _tokenService = tokenService;
        }

        public TokenDTO ValidateCredentials(UserDTO userDTO)
        {
            // Validando as credenciais
            User user = _repository.ValidateCredentials(new User { Email = userDTO.Email, Password = userDTO.Password });

            if (user == null) return null;

            List<Claim> claims = new List<Claim>{
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.Email)
            };

            // Obtendo o Token de Acesso e o RefreshToken
            string accessToken = _tokenService.GenerateAccessToken(claims);
            string refreshToken = _tokenService.GenerateRefreshToken();

            // Gerando as datas de criação e expiração do Token de Acesso
            DateTime createDate = DateTime.Now;
            DateTime expirationDate = createDate.AddMinutes(_configuration.Minutes);

            // Atualizando as informações na base de dados
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.Now.AddDays(_configuration.DaysToExpiry);
            _repository.RefreshUserInfo(user);

            // Retornando
            return new TokenDTO(
                authenticated: true,
                createdDate: createDate.ToString(DATE_FORMAT),
                expirationDate: expirationDate.ToString(DATE_FORMAT),
                accessToken: accessToken,
                refreshToken: refreshToken
            );
        }

        public TokenDTO ValidateCredentials(TokenDTO token)
        {
            // Obtendo o Token de Acesso e o RefreshToken
            string accessToken = token.AccessToken;
            string refreshToken = token.RefreshToken;

            // Obtendo o principal com base no Token de Acesso
            var principal = _tokenService.GetPrincipalFromExpiredToken(accessToken);

            // Obtendo user autenticado e retornando um user.
            var email = principal.Identity.Name;
            User user = _repository.ValidateCredentials(email); 

            // É verificado se user n é nulo, ou se o refreshtoken passado não corresponde com o do banco, ou se expirado.
            if (user == null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.Now)
                return null;

            // Gerando novos Tokens
            accessToken = _tokenService.GenerateAccessToken(principal.Claims);
            refreshToken = _tokenService.GenerateRefreshToken();

            // Gerando as datas de criação e expiração do Token de Acesso
            DateTime createDate = DateTime.Now;
            DateTime expirationDate = createDate.AddMinutes(_configuration.Minutes);

            // Atualizando as informações na base de dados
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.Now.AddDays(_configuration.DaysToExpiry);
            _repository.RefreshUserInfo(user);

            // Retornando
            return new TokenDTO(
                authenticated: true,
                createdDate: createDate.ToString(DATE_FORMAT),
                expirationDate: expirationDate.ToString(DATE_FORMAT),
                accessToken: accessToken,
                refreshToken: refreshToken
            );
        }

        public bool RevokeToken(string email)
        {
            return _repository.RevokeToken(email);
        }
    }
}