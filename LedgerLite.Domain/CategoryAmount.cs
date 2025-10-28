namespace LedgerLite.Domain;

/// <summary>
/// Represents a category and its total amount, used in financial reports.
/// </summary>
public record CategoryAmount(
    /// <summary>
    /// Gets the name of the category.
    /// </summary>
    string Category,
    /// <summary>
    /// Gets the total amount associated with the category.
    /// </summary>
    decimal Amount
);
