using Microsoft.EntityFrameworkCore;
using Domain.Entities;

namespace Infrastructure
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
        {
        }
        public DbSet<TimescaleFile> Files { get; set; }
        public DbSet<TimescaleResult> Results { get; set; }
        public DbSet<TimescaleValue> Values { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<TimescaleFile>(entity =>
            {
                entity.HasMany(e => e.Values)
                .WithOne(v => v.File)
                .IsRequired();
                entity.HasMany(e => e.Results)
                .WithOne(v => v.File)
                .IsRequired();

                entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            });
            modelBuilder.Entity<TimescaleValue>();
            modelBuilder.Entity<TimescaleResult>();
        }
    }
}
