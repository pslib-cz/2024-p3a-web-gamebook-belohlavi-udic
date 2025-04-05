using GamebookApp.Backend.Data;
using GamebookApp.Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GamebookApp.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlayersController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<PlayersController> _logger;

        public PlayersController(AppDbContext context, ILogger<PlayersController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Player>> GetPlayer(int id)
        {
            var player = await _context.Players.FindAsync(id);
            if (player == null)
            {
                return NotFound();
            }
            return player;
        }

        [HttpPut("{id}/move/{roomId}")]
        public async Task<IActionResult> MovePlayer(int id, int roomId)
        {
            _logger.LogInformation($"MovePlayer called with id: {id}, roomId: {roomId}");

            var player = await _context.Players.FindAsync(id);
            if (player == null)
            {
                _logger.LogError($"Player with id: {id} not found.");
                return NotFound("Player not found.");
            }

            var roomExists = await _context.Rooms.AnyAsync(r => r.Id == roomId);
            if (!roomExists)
            {
                _logger.LogError($"Room with id: {roomId} not found.");
                return BadRequest("Invalid room ID.");
            }

            player.CurrentRoomId = roomId;

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Player movement successful");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving player movement");
                return StatusCode(500, "Internal server error");
            }

            return NoContent();
        }

        [HttpPut("{id}/hp")]
        public async Task<IActionResult> UpdatePlayerHp(int id, [FromBody] UpdateHpModel model)
        {
            var player = await _context.Players.FindAsync(id);
            if (player == null)
                return NotFound("Player not found.");

            player.HP = model.HP;

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Player {id} HP updated to {model.HP}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating player HP");
                return StatusCode(500, "Internal server error");
            }

            return Ok(player);
        }

        [HttpPut("{id}/reset")]
        public async Task<IActionResult> ResetPlayer(int id)
        {
            var player = await _context.Players.FindAsync(id);
            if (player == null)
                return NotFound("Player not found.");

            player.CurrentRoomId = 1;
            player.HP = 100;
            player.BearHP = 500;

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Player {id} reset successful");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting player");
                return StatusCode(500, "Internal server error");
            }

            return Ok(player);
        }
    }

    public class UpdateHpModel
    {
        public int HP { get; set; }
    }
}