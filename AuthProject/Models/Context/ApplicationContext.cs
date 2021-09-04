using Microsoft.EntityFrameworkCore;

namespace AuthProject.Models.Context
{
    public class ApplicationContext : DbContext
    {
        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options) {}

        public DbSet<User> Users {get; set;}
    }
}