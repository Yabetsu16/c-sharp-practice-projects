using Microsoft.EntityFrameworkCore;
using project_1.Models;

namespace project_1.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        public DbSet<TaskItem> Tasks => Set<TaskItem>();
    }
}