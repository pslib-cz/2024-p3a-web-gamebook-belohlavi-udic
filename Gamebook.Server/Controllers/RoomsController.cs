using Gamebook.Server.Constants;
using Gamebook.Server.Data;
using Gamebook.Server.DTOs; // Add DTOs namespace
using Gamebook.Server.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Gamebook.Server.Controllers
{
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
        public async Task<ActionResult<RoomDTO>> GetRoom(int id)
        {
            var room = await _context.Rooms
                .Include(r => r.OutgoingConnections)
                .Include(r => r.IncomingConnections)
                .FirstOrDefaultAsync(r => r.ID == id);

            if (room == null)
            {
                return NotFound();
            }

            // Map to DTO
            var roomDto = new RoomDTO
            {
                ID = room.ID,
                Name = room.Name,
                Description = room.Description,
                Exits = room.Exits,
                OutgoingConnections = room.OutgoingConnections.Select(c => new ConnectionDTO
                {
                    ID = c.ID,
                    RoomID2 = c.RoomID2,
                    ConnectionType = c.ConnectionType
                }).ToList(),
                IncomingConnections = room.IncomingConnections.Select(c => new ConnectionDTO
                {
                    ID = c.ID,
                    RoomID2 = c.RoomID1, // Note: RoomID1 for incoming connections
                    ConnectionType = c.ConnectionType
                }).ToList()
            };

            return Ok(roomDto);
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

            var existingRoom = await _context.Rooms.FindAsync(id);
            if (existingRoom == null)
            {
                return NotFound();
            }

            // Update only changed properties
            _context.Entry(existingRoom).CurrentValues.SetValues(room);
            _context.Entry(existingRoom).Property(r => r.Name).IsModified = room.Name != existingRoom.Name;
            _context.Entry(existingRoom).Property(r => r.Description).IsModified = room.Description != existingRoom.Description;
            _context.Entry(existingRoom).Property(r => r.Exits).IsModified = room.Exits != existingRoom.Exits;

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
}