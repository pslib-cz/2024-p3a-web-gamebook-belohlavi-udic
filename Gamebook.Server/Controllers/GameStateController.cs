using Gamebook.Server.Data;
using Gamebook.Server.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

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
        var playerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(playerId))
        {
            return NotFound("Player not found");
        }

        var gameState = await _context.GameStates
            .OrderByDescending(g => g.Timestamp)
            .FirstOrDefaultAsync(g => g.PlayerID.ToString() == playerId);

        if (gameState == null)
        {
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
        var playerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(playerId))
        {
            return BadRequest("Invalid player");
        }

        gameState.Timestamp = DateTime.UtcNow;
        _context.GameStates.Add(gameState);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetCurrentGameState), null, gameState);
    }
}