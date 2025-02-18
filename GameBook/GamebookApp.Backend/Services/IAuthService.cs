using GamebookApp.Backend.Models;
using System.Threading.Tasks;

namespace GamebookApp.Backend.Services
{
    public interface IAuthService
    {
        string GenerateJwtToken(User user);
        string HashPassword(string password);
        bool VerifyPassword(string enteredPassword, string storedHash);
    }
}