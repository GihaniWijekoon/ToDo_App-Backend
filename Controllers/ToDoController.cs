using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.Models;
using System.Security.Claims;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ToDoController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ToDoController> _logger;

        public ToDoController(AppDbContext context, ILogger<ToDoController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ToDoItemDto>>> GetTodos()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized();
            }

            var todos = await _context.ToDoItems  // Renamed to ToDoItems
                .Where(t => t.UserId == userId)
                .Select(t => new ToDoItemDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    IsCompleted = t.IsCompleted,
                    CreatedAt = t.CreatedAt,
                    CompletedAt = t.CompletedAt
                })
                .ToListAsync();

            return Ok(todos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ToDoItemDto>> GetTodo(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var todo = await _context.ToDoItems  // Renamed to ToDoItems
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (todo == null)
            {
                return NotFound();
            }

            return Ok(new ToDoItemDto
            {
                Id = todo.Id,
                Title = todo.Title,
                Description = todo.Description,
                IsCompleted = todo.IsCompleted,
                CreatedAt = todo.CreatedAt,
                CompletedAt = todo.CompletedAt
            });
        }

        [HttpPost]
        public async Task<ActionResult<ToDoItemDto>> CreateTodo(ToDoItem todo)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized();
            }

            todo.UserId = userId;
            todo.CreatedAt = DateTime.UtcNow;

            _context.ToDoItems.Add(todo);  // Renamed to ToDoItems
            await _context.SaveChangesAsync();

            var todoDto = new ToDoItemDto
            {
                Id = todo.Id,
                Title = todo.Title,
                Description = todo.Description,
                IsCompleted = todo.IsCompleted,
                CreatedAt = todo.CreatedAt,
                CompletedAt = todo.CompletedAt
            };

            return CreatedAtAction(nameof(GetTodo), new { id = todo.Id }, todoDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateToDoItem(int id, [FromBody] ToDoItem updatedItem)
        {
            if (updatedItem == null || id <= 0)
            {
                return BadRequest("Invalid request.");
            }

            var existingItem = await _context.ToDoItems.FindAsync(id);  // Renamed to ToDoItems
            if (existingItem == null)
            {
                return NotFound("ToDo item not found.");
            }

            if (!string.IsNullOrEmpty(updatedItem.Title))
            {
                existingItem.Title = updatedItem.Title;
            }
            if (!string.IsNullOrEmpty(updatedItem.Description))
            {
                existingItem.Description = updatedItem.Description;
            }
            if (updatedItem.CompletedAt.HasValue)
            {
                existingItem.CompletedAt = updatedItem.CompletedAt;
            }

            existingItem.IsCompleted = updatedItem.IsCompleted;
            existingItem.UserId = updatedItem.UserId ?? existingItem.UserId;

            await _context.SaveChangesAsync();

            return Ok(existingItem);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTodo(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var todo = await _context.ToDoItems  // Renamed to ToDoItems
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (todo == null)
            {
                return NotFound();
            }

            _context.ToDoItems.Remove(todo);  // Renamed to ToDoItems
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
