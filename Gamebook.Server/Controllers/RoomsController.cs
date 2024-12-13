using Gamebook.Server.Constants;
using Gamebook.Server.Data;
using Gamebook.Server.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class RoomsController : ControllerBase
{
    private readonly GamebookDbContext _context;
    private readonly ILogger<RoomsController> _logger;

    public RoomsController(GamebookDbContext context, ILogger<RoomsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Gets all rooms
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Room>>> GetRooms()
    {
        return await _context.Rooms
            .Include(r => r.OutgoingConnections)
            .Include(r => r.IncomingConnections)
            .ToListAsync();
    }

    /// <summary>
    /// Gets a specific room by id
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Room>> GetRoom(int id)
    {
        var room = await _context.Rooms
            .Include(r => r.OutgoingConnections)
            .FirstOrDefaultAsync(r => r.ID == id);

        if (room == null)
        {
            return NotFound();
        }

        return room;
    }

    /// <summary>
    /// Creates a new room
    /// </summary>
    [HttpPost]
    [Authorize(Policy = Policy.Author)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Room>> CreateRoom(Room room)
    {
        _context.Rooms.Add(room);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetRoom), new { id = room.ID }, room);
    }

    /// <summary>
    /// Updates a specific room
    /// </summary>
    [HttpPut("{id:int}")]
    [Authorize(Policy = Policy.Author)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateRoom(int id, Room room)
    {
        if (id != room.ID)
        {
            return BadRequest();
        }

        _context.Entry(room).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!RoomExists(id))
            {
                return NotFound();
            }
            throw;
        }

        return NoContent();
    }

    private bool RoomExists(int id)
    {
        return _context.Rooms.Any(e => e.ID == id);
    }
}