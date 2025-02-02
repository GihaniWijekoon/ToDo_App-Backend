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

        // GET: api/todo
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ToDoItemDto>>> GetTodos()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized();
            }

            var todos = await _context.TodoItems
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


        // GET: api/todo/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ToDoItemDto>> GetTodo(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var todo = await _context.TodoItems
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


        // POST: api/todo
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

            _context.TodoItems.Add(todo);
            await _context.SaveChangesAsync();

            // ✅ Convert ToDoItem to ToDoItemDto
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


        // PUT: api/todo/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTodo(int id, ToDoItem todo)
        {
            if (id != todo.Id)
            {
                return BadRequest();
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var existingTodo = await _context.TodoItems
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (existingTodo == null)
            {
                return NotFound();
            }

            // Only update fields if they are not null
            if (!string.IsNullOrEmpty(todo.Title))
            {
                existingTodo.Title = todo.Title;
            }

            if (!string.IsNullOrEmpty(todo.Description))
            {
                existingTodo.Description = todo.Description;
            }

            // Update IsCompleted only if it's provided
            if (todo.IsCompleted != existingTodo.IsCompleted)
            {
                existingTodo.IsCompleted = todo.IsCompleted;
                if (todo.IsCompleted && !existingTodo.CompletedAt.HasValue)
                {
                    existingTodo.CompletedAt = DateTime.UtcNow;
                }
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TodoExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }



        // DELETE: api/todo/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTodo(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var todo = await _context.TodoItems
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (todo == null)
            {
                return NotFound();
            }

            _context.TodoItems.Remove(todo);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TodoExists(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return _context.TodoItems.Any(t => t.Id == id && t.UserId == userId);
        }
    }
}