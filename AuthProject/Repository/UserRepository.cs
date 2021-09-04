using System;
using System.Text;
using System.Security.Cryptography;
using System.Net.Mime;
using AuthProject.Models;
using AuthProject.Models.Context;
using AuthProject.Repository.Interfaces;
using System.Linq;

namespace AuthProject.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationContext _context;

        public UserRepository(ApplicationContext context)
        {
            _context = context;
        }

        public User ValidateCredentials(User user)
        {
            // Criptografando a senha com SHA256CryptoServiceProvider
            var passwordEncripted = ComputeHash(user.Password, new SHA256CryptoServiceProvider());

            // Retorna o usuário com base no email e senha
            return _context.Users.FirstOrDefault(u => u.Email.Equals(user.Email) && u.Password.Equals(passwordEncripted));
        }

        public User ValidateCredentials(string email)
        {
            return _context.Users.SingleOrDefault(u => u.Email.Equals(email));
        }

        public bool RevokeToken(string email)
        {
            User user = _context.Users.SingleOrDefault(u => u.Email.Equals(email));

            if (user == null) return false;

            // Seta refreshToken para nulo e salva no banco
            user.RefreshToken = null;
            _context.SaveChanges();

            return true;
        }

        public User RefreshUserInfo(User user)
        {
            // Verifica se existe o usuário com o ID indicado existe, se não retorna null
            if(!_context.Users.Any(u => u.Id.Equals(user.Id))) return null;

            // Pegar usuário no banco
            User result = _context.Users.SingleOrDefault(u => u.Id.Equals(user.Id));

            // Faz as alterações em result e salva mudanças no banco
            if (result != null) {
                try {
                    _context.Entry(result).CurrentValues.SetValues(user);
                    _context.SaveChanges();
                } catch (Exception) {
                    throw;
                }
            }
            return result;
        }   

        private string ComputeHash(string password, SHA256CryptoServiceProvider algorithm)
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(password);
            byte[] hashedBytes = algorithm.ComputeHash(inputBytes);
            return BitConverter.ToString(hashedBytes);
        }
    }
}