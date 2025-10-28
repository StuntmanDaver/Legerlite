using System.Collections.Generic;

namespace LedgerLite.Domain;

/// <summary>
/// Represents the result of a monthly financial report.
/// </summary>
public record ReportResult(
    /// <summary>
    /// Gets the total income amount for the report period.
    /// </summary>
    decimal TotalIncome,
    /// <summary>
    /// Gets the total expense amount for the report period.
    /// </summary>
    decimal TotalExpense,
    /// <summary>
    /// Gets the net balance for the report period (TotalIncome - TotalExpense).
    /// </summary>
    decimal Net,
    /// <summary>
    /// Gets the top spending categories for the report period, limited to expenses.
    /// </summary>
    List<CategoryAmount> TopCategories,
    /// <summary>
    /// Gets the total number of transactions in the report period.
    /// </summary>
    int TransactionCount
);
