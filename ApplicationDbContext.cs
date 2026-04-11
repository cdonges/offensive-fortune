using Microsoft.EntityFrameworkCore;

namespace offensive_fortune;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
        : base(options)
    {
    }

    public DbSet<RequestLog> RequestLogs { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<RequestLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.IpAddress).IsRequired().HasMaxLength(45);
            entity.Property(e => e.Timestamp).IsRequired();
            entity.Property(e => e.Path).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Method).IsRequired().HasMaxLength(10);
        });
    }
}
