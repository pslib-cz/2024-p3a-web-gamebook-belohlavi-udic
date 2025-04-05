using GamebookApp.Backend.Data;
using GamebookApp.Backend.Models;
using GamebookApp.Backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System;

namespace GamebookApp.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(AppDbContext context, IAuthService authService, ILogger<AuthController> logger)
        {
            _context = context;
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] User user)
        {
            // Přidejte logování pro ladění
            _logger.LogInformation($"Registrace uživatele: {user.Username}");

            if (await _context.Users.AnyAsync(u => u.Username == user.Username))
            {
                _logger.LogWarning($"Uživatelské jméno {user.Username} již existuje");
                return BadRequest("Username already exists.");
            }

            try
            {
                user.PasswordHash = _authService.HashPassword(user.PasswordHash);

                _context.Users.Add(user);
                await _context.SaveChangesAsync(); // Po tomto kroku by User.Id mělo být nastaveno

                _logger.LogInformation($"Vytvořen uživatel s ID: {user.Id}");

                var player = new Player
                {
                    UserId = user.Id,
                    CurrentRoomId = 1,
                    HP = 100,
                    BearHP = 500
                };

                _context.Players.Add(player);
                await _context.SaveChangesAsync();

                return Ok(new { UserId = user.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chyba při registraci uživatele");
                return StatusCode(500, "Internal server error");
            }
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