using GamebookApp.Backend.Data;
using GamebookApp.Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GamebookApp.Backend.Controllers
{
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
    }
}