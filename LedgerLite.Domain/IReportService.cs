using System.Threading.Tasks;

namespace LedgerLite.Domain;

/// <summary>
/// Defines the contract for a service that generates financial reports.
/// </summary>
public interface IReportService
{
    /// <summary>
    /// Asynchronously generates a monthly financial report for the specified year and month.
    /// </summary>
    /// <param name="year">The year for which to generate the report (e.g., 2023).</param>
    /// <param name="month">The month for which to generate the report (1-12).</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the generated report.</returns>
    Task<ReportResult> GenerateMonthlyReportAsync(int year, int month);
}
