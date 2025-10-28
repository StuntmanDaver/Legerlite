using LedgerLite.Domain;
using LedgerLite.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace LedgerLite.Infrastructure;

public class SqliteTransactionRepository : ITransactionRepository
{
    protected LedgerLiteDbContext _context;
    private readonly FileStorageConfig _config;
    private readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public SqliteTransactionRepository(FileStorageConfig config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _context = new LedgerLiteDbContext(config);

        // Handle DB creation or corruption
        var dbPath = Path.Combine(_config.DataDirectory, "ledgerlite.db");
        try
        {
            _context.Database.EnsureCreated();
        }
        catch
        {
            if (File.Exists(dbPath))
            {
                File.Delete(dbPath);
                Console.WriteLine("Warning: Recreated corrupted database file.");
            }
            _context.Database.EnsureCreated();
        }

        // Migrate from JSON if applicable
        MigrateFromJsonAsync().Wait();
    }

    protected async Task MigrateFromJsonAsync()
    {
        var jsonPath = Path.Combine(_config.DataDirectory, "transactions.json");
        if (!File.Exists(jsonPath) || await _context.Transactions.AnyAsync())
        {
            return;
        }

        try
        {
            var json = await File.ReadAllTextAsync(jsonPath);
            var transactions = JsonSerializer.Deserialize<List<Transaction>>(json, _jsonOptions) ?? new List<Transaction>();

            if (transactions.Any())
            {
                await _context.Transactions.AddRangeAsync(transactions);
                await _context.SaveChangesAsync();
                File.Delete(jsonPath);
                Console.WriteLine("Migrated transactions from JSON to SQLite.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Failed to migrate from JSON: {ex.Message}");
        }
    }

    public async Task AddAsync(Transaction transaction)
    {
        await _context.Transactions.AddAsync(transaction);
        await _context.SaveChangesAsync();
    }

    public async Task<List<Transaction>> GetAllAsync()
    {
        return await _context.Transactions
            .OrderByDescending(t => t.Date)
            .ToListAsync();
    }

    public async Task<Transaction?> GetByIdAsync(Guid id)
    {
        return await _context.Transactions.FindAsync(id);
    }

    public async Task UpdateAsync(Transaction transaction)
    {
        _context.Transactions.Update(transaction);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteByIdAsync(Guid id)
    {
        var transaction = await _context.Transactions.FindAsync(id);
        if (transaction != null)
        {
            _context.Transactions.Remove(transaction);
            await _context.SaveChangesAsync();
        }
    }
}
