using GamebookApp.Backend.Data;
using GamebookApp.Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Route("api/[controller]")]
[ApiController]
public class RoomsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<RoomsController> _logger;

    public RoomsController(AppDbContext context, ILogger<RoomsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Room>>> GetRooms()
    {
        return await _context.Rooms
            .Include(r => r.Exits)
            .ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Room>> GetRoom(int id)
    {
        _logger.LogInformation($"GetRoom called with id: {id}");

        var room = await _context.Rooms
            .Include(r => r.Exits)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (room == null)
        {
            _logger.LogError($"Room with id: {id} not found.");
            return NotFound();
        }

        _logger.LogInformation($"Room found: Name: {room.Name}, ImagePath: {room.ImagePath}");
        return room;
    }

    [HttpPost]
    public async Task<ActionResult<Room>> CreateRoom(Room room)
    {
        _context.Rooms.Add(room);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetRoom), new { id = room.Id }, room);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateRoom(int id, Room room)
    {
        if (id != room.Id)
            return BadRequest();

        _context.Entry(room).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!RoomExists(id))
                return NotFound();
            throw;
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRoom(int id)
    {
        var room = await _context.Rooms.FindAsync(id);
        if (room == null)
            return NotFound();

        _context.Rooms.Remove(room);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    private bool RoomExists(int id)
    {
        return _context.Rooms.Any(e => e.Id == id);
    }
}
