using LedgerLite.Domain;
using LedgerLite.Infrastructure;
using LedgerLite.Tests;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace LedgerLite.Tests;

public class JsonTransactionRepositoryTests
{
    private readonly FileStorageConfig _config;
    private readonly string _testPath;

    public JsonTransactionRepositoryTests()
    {
        _config = new FileStorageConfig("test_data", "test_exports");
        _testPath = Path.Combine(_config.DataDirectory, "transactions.json");
        Directory.CreateDirectory(_config.DataDirectory);
    }

    [Fact]
    public async Task LoadFromMissingFile_StartsEmpty()
    {
        if (File.Exists(_testPath)) File.Delete(_testPath);

        var repo = new JsonTransactionRepository(_config);

        var all = await repo.GetAllAsync();
        Assert.Empty(all);
    }

    [Fact]
    public async Task LoadFromCorruptFile_WarnsAndStartsEmpty()
    {
        await File.WriteAllTextAsync(_testPath, "invalid json {");

        var repo = new JsonTransactionRepository(_config);

        var all = await repo.GetAllAsync();
        Assert.Empty(all);
    }

    [Fact]
    public async Task RoundTripSerialization_PreservesData()
    {
        var originalRepo = new JsonTransactionRepository(_config);
        var id = Guid.NewGuid();
        var tx = new Transaction(id, DateTime.Now, "Test", "Cat", 123.45m, TransactionType.Income);
        await originalRepo.AddAsync(tx);

        // Load new repo
        var loadedRepo = new JsonTransactionRepository(_config);

        var loaded = await loadedRepo.GetByIdAsync(id);
        Assert.NotNull(loaded);
        Assert.Equal(tx.Id, loaded.Id);
        Assert.Equal(tx.Date, loaded.Date);
        Assert.Equal(tx.Description, loaded.Description);
        Assert.Equal(tx.Category, loaded.Category);
        Assert.Equal(tx.Amount, loaded.Amount);
        Assert.Equal(tx.Type, loaded.Type);
    }

    private void Dispose()
    {
        if (Directory.Exists(_config.DataDirectory))
        {
            Directory.Delete(_config.DataDirectory, true);
        }
    }
}
