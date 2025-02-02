// Models/ToDoItem.cs
namespace Backend.Models
{
    public class ToDoItem
    {
        public int Id { get; set; }
        public string? Title { get; set; } 
        public string? Description { get; set; } 
        public bool IsCompleted { get; set; } = false;
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string UserId { get; set; } = string.Empty;
    }
}