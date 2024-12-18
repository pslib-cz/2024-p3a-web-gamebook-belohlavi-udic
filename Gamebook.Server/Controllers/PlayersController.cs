using Gamebook.Server.Data;
using Gamebook.Server.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Gamebook.Server.Services;

namespace Gamebook.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlayersController : ControllerBase
    {
        private readonly GamebookDbContext _context;
        private readonly ILogger<PlayersController> _logger;
        private readonly IGameService _gameService;

        public PlayersController(
            GamebookDbContext context,
            ILogger<PlayersController> logger,
            IGameService gameService
            )
        {
            _context = context;
            _logger = logger;
            _gameService = gameService;
        }

        // GET: api/players
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Player>>> GetPlayers()
        {
            _logger.LogInformation("Getting all players");
            return await _context.Players
                .Include(p => p.CurrentRoom)
                .Include(p => p.GameStates)
                .ToListAsync();
        }

        // GET: api/players/5
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<Player>> GetPlayer(int id)
        {
            _logger.LogInformation($"Getting player {id}");

            var player = await _context.Players
                .Include(p => p.CurrentRoom)
                .Include(p => p.GameStates)
                .FirstOrDefaultAsync(p => p.ID == id);

            if (player == null)
            {
                _logger.LogWarning($"Player {id} not found");
                return NotFound();
            }

            return player;
        }

        // GET: api/players/current
        [HttpGet("current")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Player>> GetCurrentPlayer()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("User ID not found in claims");
                return Unauthorized();
            }

            try
            {
                var player = await _gameService.CreateNewGame(userId);
                return Ok(player);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while getting or creating current user");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occured while getting the current player");
            }
        }

        // POST: api/players/reset
        [HttpPost("reset")]
        [Authorize]
        public async Task<ActionResult<Player>> ResetPlayer()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var player = await _context.Players
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (player == null)
            {
                return NotFound();
            }

            player.HP = 100;
            player.CurrentRoomID = 1; // Reset to starting room
            player.Status = "Active";

            // Create a new game state for the reset
            var gameState = new GameState
            {
                PlayerID = player.ID,
                Timestamp = DateTime.UtcNow,
                Data = System.Text.Json.JsonSerializer.Serialize(new
                {
                    Action = "reset",
                    NewHP = player.HP,
                    NewRoom = player.CurrentRoomID
                })
            };

            _context.GameStates.Add(gameState);
            await _context.SaveChangesAsync();

            return player;
        }

        public class PlayerMoveRequest
        {
            public required int ConnectionId { get; set; }
        }

        // PUT: api/players/5/move
        [HttpPut("{id}/move")]
        [Authorize]
        public async Task<IActionResult> MovePlayer(int id, [FromBody] PlayerMoveRequest request)
        {
            _logger.LogInformation($"Processing move request for player {id}");

            var player = await _context.Players.FindAsync(id);
            if (player == null)
            {
                _logger.LogWarning($"Player {id} not found during move");
                return NotFound();
            }

            // Verify the connection exists and is valid from current room
            var connection = await _context.Connections
                .FirstOrDefaultAsync(c => c.ID == request.ConnectionId &&
                                        c.RoomID1 == player.CurrentRoomID);

            if (connection == null)
            {
                _logger.LogWarning($"Invalid movement: connection {request.ConnectionId} for player {id}");
                return BadRequest("Invalid movement");
            }

            // Move player to new room
            player.CurrentRoomID = connection.RoomID2;

            // Record the move in game state
            var gameState = new GameState
            {
                PlayerID = player.ID,
                Timestamp = DateTime.UtcNow,
                Data = System.Text.Json.JsonSerializer.Serialize(new
                {
                    Action = "move",
                    FromRoom = connection.RoomID1,
                    ToRoom = connection.RoomID2
                })
            };

            _context.GameStates.Add(gameState);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Player {id} moved to room {connection.RoomID2}");
            return NoContent();
        }

        public class UpdatePlayerStatusRequest
        {
            public required string Status { get; set; }
        }

        // PUT: api/players/5/status
        [HttpPut("{id}/status")]
        [Authorize]
        public async Task<IActionResult> UpdatePlayerStatus(int id, [FromBody] UpdatePlayerStatusRequest request)
        {
            _logger.LogInformation($"Updating status for player {id}");

            var player = await _context.Players.FindAsync(id);
            if (player == null)
            {
                _logger.LogWarning($"Player {id} not found during status update");
                return NotFound();
            }

            player.Status = request.Status;
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Status updated for player {id}: {request.Status}");
            return NoContent();
        }

        public class UpdatePlayerHPRequest
        {
            public required int HP { get; set; }
        }

        // PUT: api/players/5/hp
        [HttpPut("{id}/hp")]
        [Authorize]
        public async Task<IActionResult> UpdatePlayerHP(int id, [FromBody] UpdatePlayerHPRequest request)
        {
            _logger.LogInformation($"Updating HP for player {id}");

            var player = await _context.Players.FindAsync(id);
            if (player == null)
            {
                _logger.LogWarning($"Player {id} not found during HP update");
                return NotFound();
            }

            player.HP = request.HP;
            if (player.HP <= 0)
            {
                _logger.LogInformation($"Player {id} died (HP <= 0)");
                player.Status = "Dead";
                player.HP = 0;
            }

            // Record HP change in game state
            var gameState = new GameState
            {
                PlayerID = player.ID,
                Timestamp = DateTime.UtcNow,
                Data = System.Text.Json.JsonSerializer.Serialize(new
                {
                    Action = "updateHP",
                    OldHP = player.HP,
                    NewHP = request.HP,
                    IsDead = player.HP <= 0
                })
            };

            _context.GameStates.Add(gameState);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"HP updated for player {id}: {player.HP}");
            return NoContent();
        }

        // DELETE: api/players/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeletePlayer(int id)
        {
            var player = await _context.Players.FindAsync(id);
            if (player == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (player.UserId != userId)
            {
                return Forbid();
            }

            _context.Players.Remove(player);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}