using LedgerLite.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LedgerLite.Application;

public class TransactionService
{
    private readonly ITransactionRepository _repository;

    public TransactionService(ITransactionRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task AddTransactionAsync(Transaction transaction)
    {
        ValidateTransaction(transaction);
        await _repository.AddAsync(transaction);
    }

    public async Task<List<Transaction>> GetAllTransactionsAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<Transaction?> GetTransactionAsync(Guid id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task UpdateTransactionAsync(Transaction transaction)
    {
        ValidateTransaction(transaction);
        var existing = await GetTransactionAsync(transaction.Id);
        if (existing == null)
        {
            throw new InvalidOperationException("Transaction not found.");
        }
        await _repository.UpdateAsync(transaction);
    }

    public async Task DeleteTransactionAsync(Guid id)
    {
        await _repository.DeleteByIdAsync(id);
    }

    private static void ValidateTransaction(Transaction transaction)
    {
        if (transaction == null)
            throw new ArgumentNullException(nameof(transaction));

        if (transaction.Amount <= 0)
            throw new ArgumentException("Amount must be greater than zero.", nameof(transaction.Amount));

        if (string.IsNullOrWhiteSpace(transaction.Description))
            throw new ArgumentException("Description cannot be empty.", nameof(transaction.Description));

        if (string.IsNullOrWhiteSpace(transaction.Category))
            throw new ArgumentException("Category cannot be empty.", nameof(transaction.Category));

        if (!Enum.IsDefined(typeof(TransactionType), transaction.Type))
            throw new ArgumentException("Invalid transaction type.", nameof(transaction.Type));
    }
}
