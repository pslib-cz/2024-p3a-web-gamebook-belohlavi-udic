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

            try
            {
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