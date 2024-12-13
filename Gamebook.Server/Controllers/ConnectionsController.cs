using Gamebook.Server.Constants;
using Gamebook.Server.Data;
using Gamebook.Server.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        var connection = await _context.Connections
            .Include(c => c.Room1)
            .Include(c => c.Room2)
            .FirstOrDefaultAsync(c => c.ID == id);

        if (connection == null)
        {
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
    public async Task<ActionResult<Connection>> CreateConnection(Connection connection)
    {
        _context.Connections.Add(connection);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetConnection), new { id = connection.ID }, connection);
    }
}
