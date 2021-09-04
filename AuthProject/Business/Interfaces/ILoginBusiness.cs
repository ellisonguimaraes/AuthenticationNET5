using AuthProject.Models.DTO;

namespace AuthProject.Business.Interfaces
{
    public interface ILoginBusiness
    {
        TokenDTO ValidateCredentials(UserDTO user);     // Método usado na validação login/senha
        TokenDTO ValidateCredentials(TokenDTO token);   // Método usado na validação por refreshtoken
        bool RevokeToken(string email);
    }
}