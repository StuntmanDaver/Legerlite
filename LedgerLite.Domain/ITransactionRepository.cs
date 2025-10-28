using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LedgerLite.Domain;

/// <summary>
/// Defines the contract for a repository that manages transactions.
/// </summary>
public interface ITransactionRepository
{
    /// <summary>
    /// Asynchronously adds a new transaction to the repository.
    /// </summary>
    /// <param name="transaction">The transaction to add.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task AddAsync(Transaction transaction);

    /// <summary>
    /// Asynchronously retrieves all transactions, sorted by date in descending order.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains the list of all transactions.</returns>
    Task<List<Transaction>> GetAllAsync();

    /// <summary>
    /// Asynchronously retrieves a transaction by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the transaction.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the transaction if found, otherwise null.</returns>
    Task<Transaction?> GetByIdAsync(Guid id);

    /// <summary>
    /// Asynchronously updates an existing transaction in the repository.
    /// </summary>
    /// <param name="transaction">The updated transaction details.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task UpdateAsync(Transaction transaction);

    /// <summary>
    /// Asynchronously deletes a transaction by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the transaction to delete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DeleteByIdAsync(Guid id);
}
