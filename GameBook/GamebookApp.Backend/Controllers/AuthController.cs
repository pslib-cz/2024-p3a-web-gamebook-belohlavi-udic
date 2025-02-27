using GamebookApp.Backend.Data;
using GamebookApp.Backend.Models;
using GamebookApp.Backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace GamebookApp.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IAuthService _authService;

        public AuthController(AppDbContext context, IAuthService authService)
        {
            _context = context;
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] User user)
        {
            if (await _context.Users.AnyAsync(u => u.Username == user.Username))
            {
                return BadRequest("Username already exists.");
            }

            user.PasswordHash = _authService.HashPassword(user.PasswordHash);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            
            var player = new Player
            {
                UserId = user.Id, 
                CurrentRoomId = 1,
                HP = 100
            };

            _context.Players.Add(player);
            await _context.SaveChangesAsync();

            return Ok(new { UserId = user.Id });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] User user)
        {
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == user.Username);
            if (existingUser == null || !_authService.VerifyPassword(user.PasswordHash, existingUser.PasswordHash))
            {
                return Unauthorized("Invalid username or password.");
            }

            var token = _authService.GenerateJwtToken(existingUser);
            return Ok(new { Token = token });
        }
    }
}
