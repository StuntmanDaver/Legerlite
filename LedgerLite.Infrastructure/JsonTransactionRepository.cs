using LedgerLite.Domain;
using LedgerLite.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace LedgerLite.Infrastructure;

public class JsonTransactionRepository : ITransactionRepository
{
    private readonly List<Transaction> _transactions = new();
    private readonly FileStorageConfig _config;
    private readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    private void LoadFromFile()
    {
        var path = Path.Combine(_config.DataDirectory, "transactions.json");
        if (File.Exists(path))
        {
            try
            {
                var json = File.ReadAllText(path);
                var loadedTransactions = JsonSerializer.Deserialize<List<Transaction>>(json, _jsonOptions) ?? new List<Transaction>();
                _transactions.Clear();
                _transactions.AddRange(loadedTransactions);
            }
            catch (Exception)
            {
                Console.WriteLine("Warning: Data file unreadable, starting empty.");
                _transactions.Clear();
            }
        }
    }

    public JsonTransactionRepository(FileStorageConfig config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        LoadFromFile(); // Sync load
    }

    private async Task SaveToFileAsync()
    {
        var path = Path.Combine(_config.DataDirectory, "transactions.json");
        Directory.CreateDirectory(_config.DataDirectory);
        var json = JsonSerializer.Serialize(_transactions, _jsonOptions);
        await File.WriteAllTextAsync(path, json);
    }

    public async Task AddAsync(Transaction transaction)
    {
        _transactions.Add(transaction);
        await SaveToFileAsync();
    }

    public Task<List<Transaction>> GetAllAsync()
    {
        return Task.FromResult(_transactions.OrderByDescending(t => t.Date).ToList());
    }

    public Task<Transaction?> GetByIdAsync(Guid id)
    {
        return Task.FromResult(_transactions.FirstOrDefault(t => t.Id == id));
    }

    public async Task UpdateAsync(Transaction transaction)
    {
        var existingIndex = _transactions.FindIndex(t => t.Id == transaction.Id);
        if (existingIndex == -1)
        {
            throw new InvalidOperationException("Transaction not found.");
        }
        _transactions[existingIndex] = transaction;
        await SaveToFileAsync();
    }

    public async Task DeleteByIdAsync(Guid id)
    {
        var existingIndex = _transactions.FindIndex(t => t.Id == id);
        if (existingIndex != -1)
        {
            _transactions.RemoveAt(existingIndex);
        }
        await SaveToFileAsync();
    }
}
