using Gamebook.Server.Constants;
using Gamebook.Server.Data;
using Gamebook.Server.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Gamebook.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly GamebookDbContext _context;
        private readonly ILogger<FilesController> _logger;
        public FilesController(GamebookDbContext context, ILogger<FilesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/Files
        [HttpGet]
        public async Task<ActionResult<ListResult<FileListVM>>> GetFiles(string? name, string? ownerId, string? type, string? ownerName, FilesOrderBy? order = FilesOrderBy.Id, int? page = null, int? size = null)
        {
            var query = _context.Files.Select(f => new FileListVM 
            {
                FileId = f.FileId,
                Name = f.Name,
                Size = f.Size,
                ContentType = f.ContentType,
                CreatedAt = f.CreatedAt,
                CreatedBy = f.CreatedBy,
                CreatedById = f.CreatedById
            }).Include(f => f.CreatedBy).AsQueryable();
            _logger.LogInformation("Getting files");
            int total = await query.CountAsync();
            if (!string.IsNullOrWhiteSpace(name))
            {
                query = query.Where(f => f.Name.Contains(name));
            }
            if (ownerId != String.Empty)
            {
                query = query.Where(f => f.CreatedById == ownerId);
            }
            if (!string.IsNullOrWhiteSpace(ownerName))
            {
                query = query.Where(f => f.CreatedBy.UserName.Contains(ownerName));
            }
            if (!string.IsNullOrWhiteSpace(type))
            {
                query = query.Where(f => f.ContentType.Contains(type));
            }
            query = order switch
            {
                FilesOrderBy.Id => query.OrderBy(f => f.FileId),
                FilesOrderBy.IdDesc => query.OrderByDescending(f => f.FileId),
                FilesOrderBy.Name => query.OrderBy(f => f.Name),
                FilesOrderBy.NameDesc => query.OrderByDescending(f => f.Name),
                FilesOrderBy.Owner => query.OrderBy(f => f.CreatedBy.UserName),
                FilesOrderBy.OwnerDesc => query.OrderByDescending(f => f.CreatedBy.UserName),
                FilesOrderBy.Type => query.OrderBy(f => f.ContentType),
                FilesOrderBy.TypeDesc => query.OrderByDescending(f => f.ContentType),
                FilesOrderBy.Created => query.OrderBy(f => f.CreatedAt),
                FilesOrderBy.CreatedDesc => query.OrderByDescending(f => f.CreatedAt),
                _ => query.OrderBy(f => f.FileId)
            };
            var files = await query.Skip((page ?? 0) * (size ?? 10)).Take(size ?? 10).ToListAsync();
            _logger.LogInformation($"Found {files.Count} files");
            return Ok(new ListResult<FileListVM>
            {
                Total = total,
                Items = files,
                Page = page ?? 0,
                Size = size ?? 10
            });
        }

        // GET: api/Files/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Models.File>> GetFile(Guid id)
        {
            var file = await _context.Files.FindAsync(id);
            _logger.LogInformation($"Getting file {id}");
            if (file == null)
            {
                _logger.LogWarning($"File {id} not found");
                return NotFound();
            }
            return file;
        }

        // GET: api/Files/5/download
        [HttpGet("{id}/download")]
        public async Task<ActionResult> DownloadFile(Guid id)
        {
            var file = await _context.Files.FindAsync(id);
            _logger.LogInformation($"Downloading file {id}");
            if (file == null)
            {
                _logger.LogWarning($"File {id} not found");
                return NotFound();
            }
            return File(file.Content, file.ContentType, file.Name);
        }
        
        // POST: api/Files
        [HttpPost]
        [Authorize(Policy = Policy.Author)]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult> PostFile(IFormFile file)
        {
            _logger.LogInformation("Uploading file");
            if (file == null)
            {
                _logger.LogWarning("No file uploaded");
                return BadRequest();
            }
            var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                _logger.LogWarning("No user found");
                return Unauthorized();
            }
            var newFile = new Models.File
            {
                Name = file.FileName,
                Size = file.Length,
                ContentType = file.ContentType,
                CreatedAt = DateTime.Now,
                Content = memoryStream.ToArray(),
                CreatedById = userId
            };
            _context.Files.Add(newFile);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Uploaded file {newFile.FileId}");
            return CreatedAtAction("GetFile", new { id = newFile.FileId }, newFile);
        }
        
        [HttpDelete("{id}")]
        [Authorize(Policy = Policy.Author)]
        public async Task<ActionResult> DeleteFile(Guid id)
        {
            var file = await _context.Files.FindAsync(id);
            _logger.LogInformation($"Deleting file {id}");
            if (file == null)
            {
                _logger.LogWarning($"File {id} not found");
                return NotFound();
            }
            _context.Files.Remove(file);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Deleted file {id}");
            return Ok(new { id = file.FileId});
        }

        [HttpPut("{id}")]
        [Authorize(Policy = Policy.Author)]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult> PutFile(Guid id, IFormFile file)
        {
            var existingFile = await _context.Files.FindAsync(id);
            _logger.LogInformation($"Updating file {id}");
            if (existingFile == null)
            {
                _logger.LogWarning($"File {id} not found");
                return NotFound();
            }
            if (file == null)
            {
                _logger.LogWarning("No file uploaded");
                return BadRequest();
            }
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                _logger.LogWarning("No user found");
                return Unauthorized();
            }
            var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            existingFile.Name = file.FileName;
            existingFile.Size = file.Length;
            existingFile.ContentType = file.ContentType;
            existingFile.Content = memoryStream.ToArray();
            existingFile.CreatedAt = DateTime.Now;
            existingFile.CreatedById = userId;
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Updated file {id}");
            return Ok(existingFile);
        }
    }

    public enum FilesOrderBy
    {
        Id,
        IdDesc,
        Name,
        NameDesc,
        Owner,
        OwnerDesc,
        Type,
        TypeDesc,
        Created,
        CreatedDesc
    }
}
