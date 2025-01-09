﻿using Gamebook.Server.Constants;
using Gamebook.Server.Data;
using Gamebook.Server.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Gamebook.Server.Services;
using System.Text.Json;

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
        [Authorize(Policy = Policy.Admin)] // Přidána autorizace pro admina
        public async Task<ActionResult<IEnumerable<Player>>> GetPlayers()
        {
            _logger.LogInformation("Getting all players");
            try
            {
                return await _context.Players
                   .Include(p => p.CurrentRoom)
                   .Include(p => p.GameStates)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while getting all players");
                return StatusCode(500, new { message = "Error while getting players" });
            }
        }

        // GET: api/players/5
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<Player>> GetPlayer(int id)
        {
            _logger.LogInformation($"Getting player {id}");

            try
            {
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
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error while getting player {id}");
                return StatusCode(500, new { message = $"Error while getting player {id}" });
            }
        }

        // GET: api/players/current
        [HttpGet("current")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<object>> GetCurrentPlayer()
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

                // Ensure CurrentRoom is included
                var playerWithRoom = await _context.Players
                    .Include(p => p.CurrentRoom)
                    .FirstOrDefaultAsync(p => p.ID == player.ID);

                if (playerWithRoom == null)
                {
                    _logger.LogError($"Player with ID {player.ID} not found after creation.");
                    return NotFound(new { message = "Player not found." });
                }

                // Debugging: Log the player and room information
                _logger.LogInformation($"GetCurrentPlayer: Player ID: {playerWithRoom.ID}, CurrentRoomID: {playerWithRoom.CurrentRoomID}");

                var result = new
                {
                    playerId = playerWithRoom.ID,
                    initialRoomId = playerWithRoom.CurrentRoomID,
                    gameStateId = playerWithRoom.GameStates.OrderByDescending(gs => gs.Timestamp).FirstOrDefault()?.ID,
                    playerHp = playerWithRoom.HP,
                    playerStatus = playerWithRoom.Status
                };

                // Debugging: Log the response object
                _logger.LogInformation($"GetCurrentPlayer: Response: {JsonSerializer.Serialize(result)}");

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while getting or creating current user");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = $"An error occurred while getting the current player: {ex.Message}" });
            }
        }

        // POST: api/players/reset
        [HttpPost("reset")]
        [Authorize]
        public async Task<ActionResult<Player>> ResetPlayer()
        {
            var userId = User.FindFirstValue(ClaimTypes.Name); // Změna na ClaimTypes.Name
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("User ID not found in claims");
                return Unauthorized();
            }

            try
            {
                var player = await _context.Players
                     .FirstOrDefaultAsync(p => p.UserId == userId);

                if (player == null)
                {
                    _logger.LogWarning($"Player for user {userId} not found");
                    return NotFound();
                }

                // Ověření, zda userId v requestu odpovídá userId přihlášeného uživatele
                if (player.UserId != userId)
                {
                    _logger.LogWarning($"User {userId} is not authorized to reset this player");
                    return Forbid();
                }

                player.HP = 100;
                player.CurrentRoomID = 1;
                player.Status = "Active";

                var gameState = new GameState
                {
                    PlayerID = player.ID,
                    Timestamp = DateTime.UtcNow,
                    Data = JsonSerializer.Serialize(new
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while resetting player");
                return StatusCode(500, new { message = $"Error while resetting player: {ex.Message}" });
            }
        }

        public class PlayerMoveRequest
        {
            public required int ConnectionId { get; set; }
        }

        // PUT: api/players/current/move
        [HttpPut("current/move")]
        [Authorize]
        public async Task<IActionResult> MovePlayer([FromBody] PlayerMoveRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.Name);
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("User ID not found in claims");
                return Unauthorized();
            }

            var player = await _context.Players.FirstOrDefaultAsync(p => p.UserId == userId);
            if (player == null)
            {
                _logger.LogWarning($"Player for current user {userId} not found during move");
                return NotFound();
            }

            _logger.LogInformation($"Processing move request for player {player.ID}");

            try
            {
                var connection = await _context.Connections
                    .FirstOrDefaultAsync(c => c.ID == request.ConnectionId &&
                                            c.RoomID1 == player.CurrentRoomID);

                if (connection == null)
                {
                    _logger.LogWarning($"Invalid movement: connection {request.ConnectionId} for player {player.ID}");
                    return BadRequest(new { message = "Invalid movement" });
                }

                // Call the MovePlayer method from GameService
                var (newRoomId, playerHp, playerStatus) = await _gameService.MovePlayer(player, connection);

                // Return NoContent to indicate success without specific content
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while moving player");
                return StatusCode(500, new { message = "Error while moving player" });
            }

        }

        public class UpdatePlayerStatusRequest
        {
            public required string Status { get; set; }
        }

        // PUT: api/players/current/status
        [HttpPut("current/status")]
        [Authorize]
        public async Task<IActionResult> UpdatePlayerStatus([FromBody] UpdatePlayerStatusRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.Name);
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("User ID not found in claims");
                return Unauthorized();
            }

            var player = await _context.Players
                .FirstOrDefaultAsync(p => p.UserId == userId);
            if (player == null)
            {
                _logger.LogWarning($"Player for current user {userId} not found during status update");
                return NotFound();
            }
            _logger.LogInformation($"Updating status for player {player.ID}");

            try
            {
                player.Status = request.Status;
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Status updated for player {player.ID}: {request.Status}");
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while updating player status");
                // Detailnější chybová zpráva
                return StatusCode(500, new { message = $"Error while updating player status: {ex.Message}" });
            }
        }

        public class UpdatePlayerHPRequest
        {
            public required int HP { get; set; }
        }

        // PUT: api/players/current/hp
        [HttpPut("current/hp")]
        [Authorize]
        public async Task<IActionResult> UpdatePlayerHP([FromBody] UpdatePlayerHPRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.Name);
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("User ID not found in claims");
                return Unauthorized();
            }
            var player = await _context.Players
                .FirstOrDefaultAsync(p => p.UserId == userId);
            if (player == null)
            {
                _logger.LogWarning($"Player for current user {userId} not found during HP update");
                return NotFound();
            }

            _logger.LogInformation($"Updating HP for player {player.ID}");

            try
            {
                player.HP = request.HP;
                if (player.HP <= 0)
                {
                    _logger.LogInformation($"Player {player.ID} died (HP <= 0)");
                    player.Status = "Dead";
                    player.HP = 0;
                }

                // Record HP change in game state
                var gameState = new GameState
                {
                    PlayerID = player.ID,
                    Timestamp = DateTime.UtcNow,
                    Data = JsonSerializer.Serialize(new
                    {
                        Action = "updateHP",
                        OldHP = player.HP,
                        NewHP = request.HP,
                        IsDead = player.HP <= 0
                    })
                };

                _context.GameStates.Add(gameState);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"HP updated for player {player.ID}: {player.HP}");
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while updating player HP");
                // Detailnější chybová zpráva
                return StatusCode(500, new { message = $"Error while updating player HP: {ex.Message}" });
            }
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

            var userId = User.FindFirstValue(ClaimTypes.Name); // Změna na ClaimTypes.Name
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }
            // Admin může smazat jakéhokoli hráče, ostatní uživatelé jen sami sebe
            if (User.IsInRole(Gamebook.Server.Constants.Role.Admin) || player.UserId == userId)
            {
                _context.Players.Remove(player);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            else
            {
                return Forbid();
            }
        }
    }
}