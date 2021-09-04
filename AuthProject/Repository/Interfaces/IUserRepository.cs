using AuthProject.Models;

namespace AuthProject.Repository.Interfaces
{
    public interface IUserRepository
    {
        User ValidateCredentials(User user);        // Método usado no login/senha
        User ValidateCredentials(string email);     // Método usado no Refresh
        bool RevokeToken(string email);             // Logout
        User RefreshUserInfo(User user);
    }
}