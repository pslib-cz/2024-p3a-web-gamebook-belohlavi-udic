using Gamebook.Server.Constants;
using Gamebook.Server.Data;
using Gamebook.Server.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class ChallengesController : ControllerBase
{
    private readonly GamebookDbContext _context;
    private readonly ILogger<ChallengesController> _logger;

    public ChallengesController(GamebookDbContext context, ILogger<ChallengesController> logger)
    {
        _context = context;
        _logger = logger;
    }


    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Challenge>>> GetChallenges()
    {
        return await _context.Challenges.ToListAsync();
    }


    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Challenge>> GetChallenge(int id)
    {
        var challenge = await _context.Challenges.FindAsync(id);

        if (challenge == null)
        {
            return NotFound();
        }

        return challenge;
    }


    [HttpPost]
    [Authorize(Policy = Policy.Author)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Challenge>> CreateChallenge(Challenge challenge)
    {
        _context.Challenges.Add(challenge);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetChallenge), new { id = challenge.ID }, challenge);
    }
}
