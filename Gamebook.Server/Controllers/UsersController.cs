using Gamebook.Server.Constants;
using Gamebook.Server.Models;
using Gamebook.Server.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Role = Gamebook.Server.Models.Role;

namespace Gamebook.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly ILogger<UsersController> _logger;
        private readonly RoleManager<Role> _roleManager;

        public UsersController(UserManager<User> userManager, ILogger<UsersController> logger, RoleManager<Role> roleManager)
        {
            _userManager = userManager;
            _logger = logger;
            _roleManager = roleManager;
        }

        [HttpGet]
        public async Task<ActionResult<ListResult<UserListVM>>> GetUsers(string? username, string? email, string? roleId, UsersOrderBy order = UsersOrderBy.Id, int? page = null, int? size = null)
        {
            var query = _userManager.Users.AsQueryable(); // Remove .Include(x => x.Roles)
            _logger.LogInformation("Getting users");
            int total = await query.CountAsync(); // Use CountAsync()

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
                // Validace, zda roleId je validní GUID
                if (!Guid.TryParse(roleId, out Guid parsedRoleId))
                {
                    _logger.LogWarning($"Invalid roleId format: {roleId}");
                    return BadRequest("Invalid roleId format");
                }

                // Get users in the specified role
                var usersInRole = await _userManager.GetUsersInRoleAsync(roleId);
                var userIdsInRole = usersInRole.Select(u => u.Id);
                query = query.Where(u => userIdsInRole.Contains(u.Id));
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
                Roles = null // Remove Roles from here
            }).Skip((page ?? 0) * (size ?? 10)).Take(size ?? 10).ToListAsync();

            // Populate Roles for each user
            foreach (var user in users)
            {
                user.Roles = await _userManager.GetRolesAsync(await _userManager.FindByIdAsync(user.Id));
            }

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
        public async Task<IActionResult> GetUserById(string id)
        {
            _logger.LogInformation($"Getting user {id}");

            // Validate id format
            if (!Guid.TryParse(id, out _))
            {
                _logger.LogWarning($"Invalid id format: {id}");
                return BadRequest("Invalid id format");
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                _logger.LogWarning($"User {id} not found");
                return NotFound();
            }
            _logger.LogInformation($"User {id} found");

            // Return DTO instead of the whole User object
            var userDTO = new UserDTO
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email
            };

            return Ok(userDTO);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = Policy.Admin)]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] User user)
        {
            _logger.LogInformation($"Updating user {id}");
            if (id != user.Id.ToString())
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
        public async Task<IActionResult> DeleteUser(string id)
        {
            _logger.LogInformation($"Deleting user {id}");

            // Validate id format
            if (!Guid.TryParse(id, out _))
            {
                _logger.LogWarning($"Invalid id format: {id}");
                return BadRequest("Invalid id format");
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                _logger.LogWarning($"User {id} not found");
                return NotFound();
            }
            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                _logger.LogInformation($"User {id} deleted");
                return NoContent();
            }
            _logger.LogWarning($"Failed to delete user {id}");
            return BadRequest(result.Errors);
        }

        [HttpGet("{id}/role")]
        [Authorize(Policy = Policy.Admin)]
        public async Task<IActionResult> GetUserRoles(string id)
        {
            _logger.LogInformation($"Getting roles for user {id}");

            // Validate id format
            if (!Guid.TryParse(id, out _))
            {
                _logger.LogWarning($"Invalid id format: {id}");
                return BadRequest("Invalid id format");
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                _logger.LogWarning($"User {id} not found");
                return NotFound();
            }

            // Use UserManager.GetRolesAsync() to get the roles
            var roles = await _userManager.GetRolesAsync(user);

            _logger.LogInformation($"Found {roles.Count} roles for user {id}");
            return Ok(roles);
        }

        [HttpPost("{id}/role")]
        [Authorize(Policy = Policy.Admin)]
        public async Task<IActionResult> AddUserRole(string id, [FromBody] string roleName)
        {
            _logger.LogInformation($"Adding role {roleName} to user {id}");

            // Validate id format
            if (!Guid.TryParse(id, out _))
            {
                _logger.LogWarning($"Invalid id format: {id}");
                return BadRequest("Invalid id format");
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                _logger.LogWarning($"User {id} not found");
                return NotFound();
            }

            // Check if role exists
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                _logger.LogWarning($"Role {roleName} does not exist");
                return BadRequest($"Role {roleName} does not exist");
            }

            var result = await _userManager.AddToRoleAsync(user, roleName);
            if (result.Succeeded)
            {
                _logger.LogInformation($"Role {roleName} added to user {id}");
                return Ok();
            }
            _logger.LogWarning($"Failed to add role {roleName} to user {id}");
            return BadRequest(result.Errors);
        }

        [HttpDelete("{id}/role")]
        [Authorize(Policy = Policy.Admin)]
        public async Task<IActionResult> RemoveUserRole(string id, [FromBody] string roleName)
        {
            _logger.LogInformation($"Removing role {roleName} from user {id}");

            // Validate id format
            if (!Guid.TryParse(id, out _))
            {
                _logger.LogWarning($"Invalid id format: {id}");
                return BadRequest("Invalid id format");
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                _logger.LogWarning($"User {id} not found");
                return NotFound();
            }

            // Check if role exists
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                _logger.LogWarning($"Role {roleName} does not exist");
                return BadRequest($"Role {roleName} does not exist");
            }

            var result = await _userManager.RemoveFromRoleAsync(user, roleName);
            if (result.Succeeded)
            {
                _logger.LogInformation($"Role {roleName} removed from user {id}");
                return NoContent();
            }
            _logger.LogWarning($"Failed to remove role {roleName} from user {id}");
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

    // DTO for returning user data without sensitive information
    public class UserDTO
    {
        public required string Id { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
    }
}