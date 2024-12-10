using Gamebook.Server.Constants;
using Gamebook.Server.Models;
using Gamebook.Server.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Gamebook.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly ILogger<UsersController> _logger;
        public UsersController(UserManager<User> userManager, ILogger<UsersController> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<ListResult<UserListVM>>> GetUsers(string? username, string? email, string? roleId, UsersOrderBy order = UsersOrderBy.Id, int? page = null, int? size = null)
        {
            var query = _userManager.Users.Include(x => x.Roles).AsQueryable();
            _logger.LogInformation("Getting users");
            int total = await query.CountAsync();
            if (!string.IsNullOrWhiteSpace(username))
            {
                query = query.Where(u => u.UserName!.Contains(username));
            }
            if (!string.IsNullOrWhiteSpace(email))
            {
                query = query.Where(u => u.Email!.Contains(email));
            }
            if (!string.IsNullOrWhiteSpace(roleId))
            {
                query = query.Where(u => u.Roles!.Any(r => r.Id == roleId));
            }
            query = order switch
            {
                UsersOrderBy.Id => query.OrderBy(u => u.Id),
                UsersOrderBy.IdDesc => query.OrderByDescending(u => u.Id),
                UsersOrderBy.Username => query.OrderBy(u => u.UserName),
                UsersOrderBy.UsernameDesc => query.OrderByDescending(u => u.UserName),
                UsersOrderBy.Email => query.OrderBy(u => u.Email),
                UsersOrderBy.EmailDesc => query.OrderByDescending(u => u.Email),
                _ => query.OrderBy(u => u.Id)
            };
            var users = await query.Select(u => new UserListVM
            {
                Id = u.Id,
                UserName = u.UserName!,
                Email = u.Email!,
                Roles = u.Roles
            }).Skip((page ?? 0) * (size ?? 10)).Take(size ?? 10).ToListAsync();
            _logger.LogInformation($"Found {users.Count} users");
            return Ok(new ListResult<UserListVM>
            {
                Total = total,
                Items = users,
                Count = users.Count,
                Page = page ?? 0,
                Size = size ?? 10
            });
        }

        [HttpPost]
        [Authorize(Policy = Policy.Admin)]
        public async Task<IActionResult> CreateUser([FromBody] User user)
        {
            _logger.LogInformation($"Creating user {user.UserName}");
            var result = await _userManager.CreateAsync(user);
            if (result.Succeeded)
            {
                _logger.LogInformation($"User {user.UserName} created with id: {user.Id}");
                return Ok(user);
            }
            _logger.LogWarning($"Failed to create user {user.UserName}");
            return BadRequest(result.Errors);
        }

        [HttpGet("{id}")]
        [Authorize(Policy = Policy.Admin)]
        public async Task<IActionResult> GetUserById(Guid id)
        {
            _logger.LogInformation($"Getting user {id}");
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                _logger.LogWarning($"User {id} not found");
                return NotFound();
            }
            _logger.LogInformation($"User {id} found");
            return Ok(user);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = Policy.Admin)]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] User user)
        {
            _logger.LogInformation($"Updating user {id}");
            if (id != user.Id)
            {
                _logger.LogWarning($"User {id} request is not valid");
                return BadRequest();
            }
            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                _logger.LogInformation($"User {id} updated");
                return Ok(user);
            }
            _logger.LogWarning($"Failed to update user {id}");
            return BadRequest(result.Errors);
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = Policy.Admin)]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            _logger.LogInformation($"Deleting user {id}");
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                _logger.LogWarning($"User {id} not found");
                return NotFound();
            }
            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                _logger.LogInformation($"User {id} deleted");
                return Ok();
            }
            _logger.LogWarning($"Failed to delete user {id}");
            return BadRequest(result.Errors);
        }

        [HttpGet("{id}/role")]
        [Authorize(Policy = Policy.Admin)]
        public async Task<IActionResult> GetUserRoles(Guid id)
        {
            _logger.LogInformation($"Getting roles for user {id}");
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                _logger.LogWarning($"User {id} not found");
                return NotFound();
            }
            var roles = await _userManager.GetRolesAsync(user);
            _logger.LogInformation($"Found {roles.Count} roles for user {id}");
            return Ok(roles);
        }

        [HttpPost("{id}/role")]
        [Authorize(Policy = Policy.Admin)]
        public async Task<IActionResult> AddUserRole(Guid id, [FromBody] string role)
        {
            _logger.LogInformation($"Adding role {role} to user {id}");
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                _logger.LogWarning($"User {id} not found");
                return NotFound();
            }
            var result = await _userManager.AddToRoleAsync(user, role);
            if (result.Succeeded)
            {
                _logger.LogInformation($"Role {role} added to user {id}");
                return Ok();
            }
            _logger.LogWarning($"Failed to add role {role} to user {id}");
            return BadRequest(result.Errors);
        }

        [HttpDelete("{id}/role")]
        [Authorize(Policy = Policy.Admin)]
        public async Task<IActionResult> RemoveUserRole(Guid id, [FromBody] string role)
        {
            _logger.LogInformation($"Removing role {role} from user {id}");
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                _logger.LogWarning($"User {id} not found");
                return NotFound();
            }
            var result = await _userManager.RemoveFromRoleAsync(user, role);
            if (result.Succeeded)
            {
                _logger.LogInformation($"Role {role} removed from user {id}");
                return Ok();
            }
            _logger.LogWarning($"Failed to remove role {role} from user {id}");
            return BadRequest(result.Errors);
        }
    }

    public enum UsersOrderBy
    {
        Id,
        IdDesc,
        Username,
        UsernameDesc,
        Email,
        EmailDesc
    }
}
