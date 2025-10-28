using LedgerLite.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LedgerLite.Tests;

public class FakeTransactionRepository : ITransactionRepository
{
    private readonly List<Transaction> _transactions = new();

    public Task AddAsync(Transaction transaction)
    {
        _transactions.Add(transaction);
        return Task.CompletedTask;
    }

    public Task<List<Transaction>> GetAllAsync()
    {
        return Task.FromResult(_transactions.OrderByDescending(t => t.Date).ToList());
    }

    public Task<Transaction?> GetByIdAsync(Guid id)
    {
        return Task.FromResult(_transactions.FirstOrDefault(t => t.Id == id));
    }

    public Task UpdateAsync(Transaction transaction)
    {
        var index = _transactions.FindIndex(t => t.Id == transaction.Id);
        if (index != -1)
        {
            _transactions[index] = transaction;
        }
        return Task.CompletedTask;
    }

    public Task DeleteByIdAsync(Guid id)
    {
        var index = _transactions.FindIndex(t => t.Id == id);
        if (index != -1)
        {
            _transactions.RemoveAt(index);
        }
        return Task.CompletedTask;
    }
}
