using GameBook.Server.Data;
using GameBook.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GameBook.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlayersController : ControllerBase
    {
        private readonly GameBookContext _context;

        public PlayersController(GameBookContext context)
        {
            _context = context;
        }

        // GET: api/Players
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Player>>> GetPlayers()
        {
            return await _context.Players
                .Include(p => p.GameStates)
                .ToListAsync();
        }

        // GET: api/Players/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Player>> GetPlayer(int id)
        {
            var player = await _context.Players
                .Include(p => p.GameStates)
                .FirstOrDefaultAsync(p => p.ID == id);

            if (player == null)
            {
                return NotFound();
            }

            return player;
        }

        // POST: api/Players/Register
        [HttpPost("Register")]
        public async Task<ActionResult<Player>> RegisterPlayer([FromBody] RegisterModel model)
        {
            if (await _context.Players.AnyAsync(p => p.Username == model.Username))
            {
                return BadRequest("Uživatelské jméno již existuje");
            }

            var player = new Player
            {
                Username = model.Username,
                PasswordHash = model.Password,
                HP = 100,
                Status = "Active",
                CurrentRoomID = 1  // Explicitně nastavíme startovní místnost
            };

            _context.Players.Add(player);
            await _context.SaveChangesAsync();

            // Načteme hráče včetně aktuální místnosti
            var createdPlayer = await _context.Players
                .FirstOrDefaultAsync(p => p.ID == player.ID);

            return CreatedAtAction(nameof(GetPlayer), new { id = player.ID }, createdPlayer);
        }

        // POST: api/Players/Login
        [HttpPost("Login")]
        public async Task<ActionResult<Player>> LoginPlayer([FromBody] LoginModel login)
        {
            var player = await _context.Players
                .FirstOrDefaultAsync(p => p.Username == login.Username && p.PasswordHash == login.Password);

            if (player == null)
            {
                return NotFound("Nesprávné uživatelské jméno nebo heslo");
            }

            return player;
        }

        // PUT: api/Players/5/Move
        [HttpPut("{id}/Move")]
        public async Task<IActionResult> MovePlayer(int id, [FromBody] int newRoomId)
        {
            var player = await _context.Players.FindAsync(id);
            if (player == null)
            {
                return NotFound();
            }

            // Ověříme, zda místnost existuje
            var newRoom = await _context.Rooms.FindAsync(newRoomId);
            if (newRoom == null)
            {
                return BadRequest("Cílová místnost neexistuje");
            }

            // Ověříme, zda existuje spojení mezi místnostmi
            var connection = await _context.Connections
                .AnyAsync(c =>
                    (c.RoomID1 == player.CurrentRoomID && c.RoomID2 == newRoomId) ||
                    (c.RoomID2 == player.CurrentRoomID && c.RoomID1 == newRoomId));

            if (!connection)
            {
                return BadRequest("Nelze se přesunout do této místnosti - neexistuje spojení");
            }

            player.CurrentRoomID = newRoomId;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PlayerExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        private bool PlayerExists(int id)
        {
            return _context.Players.Any(e => e.ID == id);
        }
    }

    public class LoginModel
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterModel
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}