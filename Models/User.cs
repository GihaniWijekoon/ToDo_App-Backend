using Microsoft.AspNetCore.Identity;

namespace Backend.Models
{
    public class User : IdentityUser
    {
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Address { get; set; }
        public ICollection<ToDoItem> ToDoItems { get; set; } = new List<ToDoItem>();
        public required new string PhoneNumber { get; set; }

    }
}
