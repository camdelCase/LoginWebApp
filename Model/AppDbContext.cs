using Microsoft.EntityFrameworkCore;

namespace LoginWebApp.Model
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(user =>
            {
                user.Property(u => u.Username).HasMaxLength(128);
                user.Property(u => u.Email).HasMaxLength(256);
                user.HasIndex(u => u.Username).IsUnique();
            });
        }
    }
}
