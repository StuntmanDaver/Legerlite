using LedgerLite.Application;
using LedgerLite.Domain;
using LedgerLite.Tests;
using System;
using System.Threading.Tasks;
using Xunit;

namespace LedgerLite.Tests;

public class TransactionServiceTests
{
    private readonly FakeTransactionRepository _fakeRepo;
    private readonly TransactionService _service;

    public TransactionServiceTests()
    {
        _fakeRepo = new FakeTransactionRepository();
        _service = new TransactionService(_fakeRepo);
    }

    [Fact]
    public async Task AddTransactionAsync_ValidTransaction_Succeeds()
    {
        var transaction = new Transaction(Guid.NewGuid(), DateTime.Now, "Test", "TestCat", 100m, TransactionType.Income);

        await _service.AddTransactionAsync(transaction);

        var all = await _service.GetAllTransactionsAsync();
        Assert.Single(all);
        Assert.Equal(transaction, all[0]);
    }

    [Fact]
    public async Task AddTransactionAsync_InvalidAmount_ThrowsArgumentException()
    {
        var transaction = new Transaction(Guid.NewGuid(), DateTime.Now, "Test", "TestCat", 0m, TransactionType.Income);

        await Assert.ThrowsAsync<ArgumentException>(() => _service.AddTransactionAsync(transaction));
    }

    [Fact]
    public async Task AddTransactionAsync_InvalidType_ThrowsArgumentException()
    {
        var transaction = new Transaction(Guid.NewGuid(), DateTime.Now, "Test", "TestCat", 100m, (TransactionType)3);

        await Assert.ThrowsAsync<ArgumentException>(() => _service.AddTransactionAsync(transaction));
    }

    [Fact]
    public async Task AddTransactionAsync_EmptyDescription_ThrowsArgumentException()
    {
        var transaction = new Transaction(Guid.NewGuid(), DateTime.Now, "", "TestCat", 100m, TransactionType.Income);

        await Assert.ThrowsAsync<ArgumentException>(() => _service.AddTransactionAsync(transaction));
    }

    [Fact]
    public async Task UpdateTransactionAsync_ValidUpdate_Succeeds()
    {
        var id = Guid.NewGuid();
        var original = new Transaction(id, DateTime.Now, "Original", "Cat", 100m, TransactionType.Income);
        await _service.AddTransactionAsync(original);

        var updated = new Transaction(id, DateTime.Now.AddDays(1), "Updated", "NewCat", 200m, TransactionType.Expense);
        await _service.UpdateTransactionAsync(updated);

        var fetched = await _service.GetTransactionAsync(id);
        Assert.Equal(updated, fetched);
    }

    [Fact]
    public async Task DeleteTransactionAsync_ExistingId_RemovesTransaction()
    {
        var id = Guid.NewGuid();
        var transaction = new Transaction(id, DateTime.Now, "Test", "Cat", 100m, TransactionType.Income);
        await _service.AddTransactionAsync(transaction);

        await _service.DeleteTransactionAsync(id);

        var all = await _service.GetAllTransactionsAsync();
        Assert.Empty(all);
    }
}
