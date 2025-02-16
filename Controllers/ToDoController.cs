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

        // [HttpGet]
        // public async Task<ActionResult<IEnumerable<ToDoItemDto>>> GetTodos()
        // {
        //     // Extract userId from the token
        //     var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //     if (userId == null)
        //     {
        //         return Unauthorized();
        //     }

        //     // Log the extracted userId for debugging
        //     _logger.LogInformation("Extracted userId from token: {UserId} ====================", userId);

        //     // Fetch tasks for the logged-in user
        //     var todos = await _context.ToDoItems
        //         .Where(t => t.UserId == userId) // Filter by userId
        //         .Select(t => new ToDoItemDto
        //         {
        //             Id = t.Id,
        //             Title = t.Title,
        //             Description = t.Description,
        //             IsCompleted = t.IsCompleted,
        //             CreatedAt = t.CreatedAt,
        //             CompletedAt = t.CompletedAt
        //         })
        //         .ToListAsync();

        //     return Ok(todos);
        // }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<ToDoItemDto>>> GetTodos(string userId)
        {
            // Ensure the request includes a valid token
            if (User.Identity?.IsAuthenticated != true)
            {
                return Unauthorized("Authentication required.");
            }

            // Log the userId received from the route for debugging
            _logger.LogInformation("Requested To-Dos for userId: {UserId}", userId);

            // Fetch tasks for the requested userId (from route parameter, NOT the token)
            var todos = await _context.ToDoItems
                .Where(t => t.UserId == userId) // Strictly using userId from the route
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

        // [HttpPost]
        // public async Task<ActionResult<ToDoItemDto>> CreateTodo(ToDoItem todo)
        // {
        //     var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //     if (userId == null)
        //     {
        //         return Unauthorized();
        //     }

        //     todo.UserId = userId;
        //     todo.CreatedAt = DateTime.UtcNow;

        //     _context.ToDoItems.Add(todo);  // Renamed to ToDoItems
        //     await _context.SaveChangesAsync();

        //     var todoDto = new ToDoItemDto
        //     {
        //         Id = todo.Id,
        //         Title = todo.Title,
        //         Description = todo.Description,
        //         IsCompleted = todo.IsCompleted,
        //         CreatedAt = todo.CreatedAt,
        //         CompletedAt = todo.CompletedAt
        //     };

        //     return CreatedAtAction(nameof(GetTodo), new { id = todo.Id }, todoDto);
        // }

        [HttpPost("user/{userId}")]  // API endpoint: /api/ToDo/user/{userId}
        public async Task<ActionResult<ToDoItemDto>> CreateTodo(string userId, ToDoItem todo)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return BadRequest("User ID is required in the URL.");
            }

            // Validate the authenticated user (still needed for security)
            if (User.Identity?.IsAuthenticated != true)
            {
                return Unauthorized("Authentication required.");
            }

            // Assign userId from the URL instead of decoding it from the token
            todo.UserId = userId;
            todo.CreatedAt = DateTime.UtcNow;

            _context.ToDoItems.Add(todo);
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

            return CreatedAtAction(nameof(GetTodos), new { userId = userId }, todoDto);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateToDoItem(int id, [FromBody] ToDoItem updatedItem)
        {
            if (updatedItem == null || id <= 0)
            {
                return BadRequest("Invalid request.");
            }

            var existingItem = await _context.ToDoItems.FindAsync(id);
            if (existingItem == null)
            {
                return NotFound("ToDo item not found.");
            }

            // Only update specific fields
            if (!string.IsNullOrEmpty(updatedItem.Title))
            {
                existingItem.Title = updatedItem.Title;
            }
            if (!string.IsNullOrEmpty(updatedItem.Description))
            {
                existingItem.Description = updatedItem.Description;
            }
            
            // Optional: Update completion status if it's included
            if (updatedItem.IsCompleted != existingItem.IsCompleted)
            {
                existingItem.IsCompleted = updatedItem.IsCompleted;
                existingItem.CompletedAt = updatedItem.IsCompleted ? DateTime.UtcNow : null;
            }

            // Don't update UserId - keep the existing one
            // existingItem.UserId = updatedItem.UserId ?? existingItem.UserId;

            try
            {
                await _context.SaveChangesAsync();
                return Ok(existingItem);
            }
            catch (Exception ex)
            {
                // Add better error logging
                _logger.LogError(ex, "Error updating todo item {Id}", id);
                return StatusCode(500, new { message = "Error updating todo item", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTodo(int id)
        {
           // var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
           _logger.LogInformation("id==", id);
            var todo = await _context.ToDoItems  // Renamed to ToDoItems
                .FirstOrDefaultAsync(t => t.Id == id );

            if (todo == null)
            {
                return NotFound();
            }

            _context.ToDoItems.Remove(todo);  // Renamed to ToDoItems
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut("toggle-completion/{id}")]
        public async Task<IActionResult> ToggleCompletion(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Invalid task ID.");
            }

            var existingItem = await _context.ToDoItems.FindAsync(id);
            if (existingItem == null)
            {
                return NotFound("ToDo item not found.");
            }

            // Toggle the completion status
            existingItem.IsCompleted = !existingItem.IsCompleted;
            existingItem.CompletedAt = existingItem.IsCompleted ? DateTime.UtcNow : null;

            try
            {
                await _context.SaveChangesAsync();
                return Ok(new { 
                    isCompleted = existingItem.IsCompleted,
                    message = $"Task {(existingItem.IsCompleted ? "completed" : "reopened")} successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling completion status for todo item {Id}", id);
                return StatusCode(500, new { message = "Error updating todo item", error = ex.Message });
            }
        }
    }
}
