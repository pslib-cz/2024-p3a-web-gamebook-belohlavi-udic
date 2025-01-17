using GameBook.Server.Data;
using GameBook.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GameBook.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GameStatesController : ControllerBase
    {
        private readonly GameBookContext _context;

        public GameStatesController(GameBookContext context)
        {
            _context = context;
        }

        // GET: api/GameStates/Player/5
        [HttpGet("Player/{playerId}")]
        public async Task<ActionResult<IEnumerable<GameState>>> GetPlayerGameStates(int playerId)
        {
            return await _context.GameStates
                .Where(g => g.PlayerID == playerId)
                .OrderByDescending(g => g.Timestamp)
                .ToListAsync();
        }

        // POST: api/GameStates
        [HttpPost]
        public async Task<ActionResult<GameState>> SaveGameState(GameState gameState)
        {
            gameState.Timestamp = DateTime.UtcNow;
            _context.GameStates.Add(gameState);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPlayerGameStates),
                new { playerId = gameState.PlayerID }, gameState);
        }
    }
}