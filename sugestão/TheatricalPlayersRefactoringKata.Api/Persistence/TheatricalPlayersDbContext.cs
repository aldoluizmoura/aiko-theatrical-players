using Microsoft.EntityFrameworkCore;

namespace TheatricalPlayersRefactoringKata.Api.Persistence;

public class TheatricalPlayersDbContext : DbContext
{
    public TheatricalPlayersDbContext(DbContextOptions<TheatricalPlayersDbContext> options)
        : base(options)
    {
    }

    public DbSet<StatementEntity> Statements => Set<StatementEntity>();
    public DbSet<StatementPlayEntity> StatementPlays => Set<StatementPlayEntity>();
    public DbSet<StatementPerformanceEntity> StatementPerformances => Set<StatementPerformanceEntity>();
    public DbSet<StatementLineEntity> StatementLines => Set<StatementLineEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<StatementEntity>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Customer).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(32);
            entity.Property(x => x.OutputFilePath).HasMaxLength(500);
            entity.Property(x => x.Error).HasMaxLength(2000);
            entity.HasMany(x => x.Plays).WithOne(x => x.Statement).HasForeignKey(x => x.StatementId);
            entity.HasMany(x => x.Performances).WithOne(x => x.Statement).HasForeignKey(x => x.StatementId);
            entity.HasMany(x => x.Lines).WithOne(x => x.Statement).HasForeignKey(x => x.StatementId);
        });

        modelBuilder.Entity<StatementPlayEntity>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.PlayKey).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Type).HasMaxLength(50).IsRequired();
        });

        modelBuilder.Entity<StatementPerformanceEntity>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.PlayKey).HasMaxLength(100).IsRequired();
        });

        modelBuilder.Entity<StatementLineEntity>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.PlayName).HasMaxLength(200).IsRequired();
        });
    }
}
