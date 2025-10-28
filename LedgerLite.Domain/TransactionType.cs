namespace LedgerLite.Domain;

/// <summary>
/// Defines the type of a financial transaction.
/// </summary>
public enum TransactionType
{
    /// <summary>
    /// An income transaction that increases the balance.
    /// </summary>
    Income,
    /// <summary>
    /// An expense transaction that decreases the balance.
    /// </summary>
    Expense
}
