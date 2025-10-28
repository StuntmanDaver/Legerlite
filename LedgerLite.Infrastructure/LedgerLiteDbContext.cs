using Microsoft.EntityFrameworkCore;
using LedgerLite.Domain;
using LedgerLite.Infrastructure;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace LedgerLite.Infrastructure;

public class LedgerLiteDbContext : DbContext
{
    public DbSet<Transaction> Transactions { get; set; } = null!;

    private readonly FileStorageConfig? _config;

    public LedgerLiteDbContext(FileStorageConfig config)
    {
        _config = config;
    }

    public LedgerLiteDbContext(DbContextOptions<LedgerLiteDbContext> options) : base(options)
    {
        Transactions = Set<Transaction>();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var dbPath = Path.Combine(_config!.DataDirectory, "ledgerlite.db");
            Directory.CreateDirectory(_config!.DataDirectory);
            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Type).HasConversion<int>();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Category).HasMaxLength(100);
        });
    }
}
