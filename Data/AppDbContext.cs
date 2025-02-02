using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Backend.Models;

namespace Backend.Data
{
    public class AppDbContext : IdentityDbContext<User>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<ToDoItem> TodoItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ToDoItem>()
                .HasKey(t => t.Id);

            modelBuilder.Entity<ToDoItem>()
                .HasOne<User>() // Remove direct reference to 'User' property
                .WithMany(u => u.ToDoItems)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
