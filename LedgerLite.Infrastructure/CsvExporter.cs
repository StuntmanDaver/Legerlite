using LedgerLite.Domain;
using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

namespace LedgerLite.Infrastructure;

public static class CsvExporter
{
    public static async Task ExportReportAsync(ReportResult report, int year, int month, FileStorageConfig config)
    {
        var filename = $"Report_{year}_{month:D2}.csv";
        var path = Path.Combine(config.ExportDirectory, filename);
        Directory.CreateDirectory(config.ExportDirectory);

        using var writer = new StreamWriter(path);
        await writer.WriteLineAsync($"Monthly Financial Report - {year}-{month:D2}");

        await writer.WriteLineAsync($"Total Income,{report.TotalIncome:C}");
        await writer.WriteLineAsync($"Total Expense,{report.TotalExpense:C}");
        await writer.WriteLineAsync($"Net Balance,{report.Net:C}");
        await writer.WriteLineAsync($"Total Transactions,{report.TransactionCount}");

        await writer.WriteLineAsync();

        await writer.WriteLineAsync("Top Expense Categories");
        await writer.WriteLineAsync("Category,Amount");
        foreach (var category in report.TopCategories)
        {
            await writer.WriteLineAsync($"{category.Category},{category.Amount:C}");
        }
    }
}
