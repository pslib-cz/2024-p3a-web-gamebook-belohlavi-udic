using Gamebook.Server.Constants;
using Gamebook.Server.Data;
using Gamebook.Server.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Gamebook.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConnectionsController : ControllerBase
    {
        private readonly GamebookDbContext _context;
        private readonly ILogger<ConnectionsController> _logger;

        public ConnectionsController(GamebookDbContext context, ILogger<ConnectionsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Gets all connections
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Connection>>> GetConnections()
        {
            _logger.LogInformation("Getting all connections");
            return await _context.Connections
               .Include(c => c.Room1)
               .Include(c => c.Room2)
               .ToListAsync();
        }

        /// <summary>
        /// Gets a specific connection
        /// </summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Connection>> GetConnection(int id)
        {
            _logger.LogInformation($"Getting connection {id}");
            var connection = await _context.Connections
                .Include(c => c.Room1)
                .Include(c => c.Room2)
                .FirstOrDefaultAsync(c => c.ID == id);

            if (connection == null)
            {
                _logger.LogWarning($"Connection {id} not found");
                return NotFound();
            }

            // Kontrola, zda Room1 a Room2 nejsou null
            if (connection.Room1 == null || connection.Room2 == null)
            {
                _logger.LogError($"Room1 or Room2 is null for connection {id}");
                return StatusCode(StatusCodes.Status500InternalServerError, "Room1 or Room2 is null");
            }

            return connection;
        }

        /// <summary>
        /// Creates a new connection between rooms
        /// </summary>
        [HttpPost]
        [Authorize(Policy = Policy.Author)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Connection>> CreateConnection([FromBody] Connection connection)
        {
            _logger.LogInformation("Creating a new connection");

            // Validate the model
            if (!ModelState.IsValid)
            {
                _logger.LogWarning($"Invalid model state when creating a connection");
                return BadRequest(ModelState);
            }

            // Kontrola, zda RoomID1 a RoomID2 nejsou stejné
            if (connection.RoomID1 == connection.RoomID2)
            {
                _logger.LogWarning($"RoomID1 and RoomID2 cannot be the same when creating a connection");
                return BadRequest("RoomID1 and RoomID2 cannot be the same.");
            }

            // Kontrola, zda RoomID1 a RoomID2 existují
            if (!await _context.Rooms.AnyAsync(r => r.ID == connection.RoomID1))
            {
                _logger.LogWarning($"Room {connection.RoomID1} not found");
                return BadRequest($"Room {connection.RoomID1} not found");
            }

            if (!await _context.Rooms.AnyAsync(r => r.ID == connection.RoomID2))
            {
                _logger.LogWarning($"Room {connection.RoomID2} not found");
                return BadRequest($"Room {connection.RoomID2} not found");
            }

            try
            {
                // Kontrola, zda Room1 a Room2 jsou načteny
                connection.Room1 = await _context.Rooms.FindAsync(connection.RoomID1);
                connection.Room2 = await _context.Rooms.FindAsync(connection.RoomID2);
                if (connection.Room1 == null || connection.Room2 == null)
                {
                    _logger.LogError($"Room1 or Room2 is null for new connection");
                    return StatusCode(StatusCodes.Status500InternalServerError, "Room1 or Room2 is null");
                }

                _context.Connections.Add(connection);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Connection {connection.ID} created successfully");
                return CreatedAtAction(nameof(GetConnection), new { id = connection.ID }, connection);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occured while creating a new connection");
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
            }

        }
    }
}