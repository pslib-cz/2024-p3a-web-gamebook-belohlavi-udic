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
    public class PlayersController : ControllerBase
    {
        private readonly GamebookDbContext _dbContext;
        private readonly ILogger<PlayersController> _playersLogger;

        public PlayersController(
            GamebookDbContext dbContext,
            ILogger<PlayersController> logger)
        {
            _dbContext = dbContext;
            _playersLogger = logger;
        }

        // GET: api/players
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Player>>> GetPlayers()
        {
            _playersLogger.LogInformation("Getting all players");
            return await _dbContext.Players
                .Include(p => p.CurrentRoom)
                .Include(p => p.GameStates)
                .ToListAsync();
        }

        // GET: api/players/5
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<Player>> GetPlayer(int id)
        {
            _playersLogger.LogInformation($"Getting player {id}");

            var player = await _dbContext.Players
                .Include(p => p.CurrentRoom)
                .Include(p => p.GameStates)
                .FirstOrDefaultAsync(p => p.ID == id);

            if (player == null)
            {
                _playersLogger.LogWarning($"Player {id} not found");
                return NotFound();
            }

            return player;
        }

        // GET: api/players/current
        [HttpGet("current")]
        [Authorize]
        public async Task<ActionResult<Player>> GetCurrentPlayer()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                _playersLogger.LogWarning("User ID not found in claims");
                return Unauthorized();
            }

            var player = await _dbContext.Players
                .Include(p => p.CurrentRoom)
                .Include(p => p.GameStates)
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (player == null)
            {
                _playersLogger.LogInformation($"Creating new player for user {userId}");
                player = new Player
                {
                    UserId = userId,
                    HP = 100,
                    CurrentRoomID = 1,
                    Status = "Active"
                };

                _dbContext.Players.Add(player);
                await _dbContext.SaveChangesAsync();
            }

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
            _playersLogger.LogInformation($"Processing move request for player {id}");

            var player = await _dbContext.Players.FindAsync(id);
            if (player == null)
            {
                _playersLogger.LogWarning($"Player {id} not found during move");
                return NotFound();
            }

            var connection = await _dbContext.Connections
                .FirstOrDefaultAsync(c => c.ID == request.ConnectionId &&
                                        c.RoomID1 == player.CurrentRoomID);

            if (connection == null)
            {
                _playersLogger.LogWarning($"Invalid movement: connection {request.ConnectionId} for player {id}");
                return BadRequest("Invalid movement");
            }

            player.CurrentRoomID = connection.RoomID2;

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

            _dbContext.GameStates.Add(gameState);
            await _dbContext.SaveChangesAsync();

            _playersLogger.LogInformation($"Player {id} moved to room {connection.RoomID2}");
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
            _playersLogger.LogInformation($"Updating status for player {id}");

            var player = await _dbContext.Players.FindAsync(id);
            if (player == null)
            {
                _playersLogger.LogWarning($"Player {id} not found during status update");
                return NotFound();
            }

            player.Status = request.Status;
            await _dbContext.SaveChangesAsync();

            _playersLogger.LogInformation($"Status updated for player {id}: {request.Status}");
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
            _playersLogger.LogInformation($"Updating HP for player {id}");

            var player = await _dbContext.Players.FindAsync(id);
            if (player == null)
            {
                _playersLogger.LogWarning($"Player {id} not found during HP update");
                return NotFound();
            }

            player.HP = request.HP;
            if (player.HP <= 0)
            {
                _playersLogger.LogInformation($"Player {id} died (HP <= 0)");
                player.Status = "Dead";
                player.HP = 0;
            }

            await _dbContext.SaveChangesAsync();
            _playersLogger.LogInformation($"HP updated for player {id}: {player.HP}");
            return NoContent();
        }
    }
}