using System;

namespace LedgerLite.Domain;

/// <summary>
/// Represents a financial transaction in the personal finance tracker.
/// </summary>
public record Transaction(
    /// <summary>
    /// Gets the unique identifier of the transaction.
    /// </summary>
    Guid Id,
    /// <summary>
    /// Gets the date when the transaction occurred.
    /// </summary>
    DateTime Date,
    /// <summary>
    /// Gets the description of the transaction.
    /// </summary>
    string Description,
    /// <summary>
    /// Gets the category of the transaction (e.g., Groceries, Salary).
    /// </summary>
    string Category,
    /// <summary>
    /// Gets the amount of the transaction. Always positive; sign determined by Type.
    /// </summary>
    decimal Amount,
    /// <summary>
    /// Gets the type of the transaction (Income or Expense).
    /// </summary>
    TransactionType Type
);
