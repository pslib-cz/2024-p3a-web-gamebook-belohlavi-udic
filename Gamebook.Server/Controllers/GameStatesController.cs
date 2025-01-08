using Gamebook.Server.Data;
using Gamebook.Server.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Gamebook.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GameStatesController : ControllerBase
    {
        private readonly GamebookDbContext _context;
        private readonly ILogger<GameStatesController> _logger;

        public GameStatesController(GamebookDbContext context, ILogger<GameStatesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("current")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GameState>> GetCurrentGameState()
        {
            // Získání userId z ClaimTypes.Name
            var userId = User.FindFirstValue(ClaimTypes.Name);
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("User ID not found in claims");
                return Unauthorized("User ID not found");
            }

            // Vyhledání uživatele podle userId
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                _logger.LogWarning($"User with ID {userId} not found");
                return NotFound("User not found");
            }

            // Použití playerId z nalezeného uživatele
            var gameState = await _context.GameStates
               .Include(g => g.Player)
               .Where(g => g.Player.UserId == userId) // Použij userId pro vyhledání
               .OrderByDescending(g => g.Timestamp)
               .FirstOrDefaultAsync();

            if (gameState == null)
            {
                _logger.LogWarning("No game state found for the current user");
                return NotFound("No game state found");
            }
            return gameState;
        }

        [HttpPost("save")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<GameState>> SaveGameState([FromBody] GameState gameState)
        {
            // Získání userId z ClaimTypes.Name
            var userId = User.FindFirstValue(ClaimTypes.Name);
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("User ID not found in claims");
                return BadRequest("Invalid player");
            }

            // Vyhledání uživatele podle userId
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                _logger.LogWarning($"User with ID {userId} not found");
                return BadRequest("Invalid player");
            }

            // Vyhledání hráče podle user.Id (což je playerId)
            var player = await _context.Players.FirstOrDefaultAsync(p => p.UserId == userId);
            if (player == null)
            {
                _logger.LogWarning($"Player for user {userId} not found");
                return BadRequest("Invalid player");
            }

            // Kontrola, zda gameState.PlayerID je validní
            if (gameState.PlayerID != player.ID)
            {
                _logger.LogWarning($"Invalid PlayerID in GameState");
                return BadRequest("Invalid PlayerID");
            }

            gameState.Timestamp = DateTime.UtcNow;
            _context.GameStates.Add(gameState);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCurrentGameState), null, gameState);
        }
    }
}