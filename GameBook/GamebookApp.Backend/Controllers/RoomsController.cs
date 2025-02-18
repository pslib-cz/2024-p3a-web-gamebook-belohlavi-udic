using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GamebookApp.Backend.Data;
using GamebookApp.Backend.Models;
using Microsoft.Extensions.Logging;

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

        // GET: api/Rooms
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Room>>> GetRooms()
        {
            return await _context.Rooms.ToListAsync();
        }

        // GET: api/Rooms/5
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