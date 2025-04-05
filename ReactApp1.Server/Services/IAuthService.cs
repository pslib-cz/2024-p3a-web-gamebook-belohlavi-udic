using GamebookApp.Backend.Models;

namespace GamebookApp.Backend.Services
{
    public interface IAuthService
    {
        string HashPassword(string password);
        bool VerifyPassword(string password, string passwordHash);
        string GenerateJwtToken(User user);
    }
}