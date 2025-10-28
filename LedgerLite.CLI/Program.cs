using LedgerLite.Application;
using LedgerLite.CLI;
using LedgerLite.Domain;
using LedgerLite.Infrastructure;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace LedgerLite.CLI;

class Program
{
    static async Task Main(string[] args)
    {
        var config = FileStorageConfig.Default;
        ITransactionRepository repository;
        var storageType = Environment.GetEnvironmentVariable("LEDGERLITE_STORAGE")?.ToLower() ?? "sqlite";
        if (storageType == "json")
        {
            repository = new JsonTransactionRepository(config);
        }
        else
        {
            repository = new SqliteTransactionRepository(config);
        }
        var transactionService = new TransactionService(repository);
        var reportService = new ReportService(repository);

        Console.WriteLine("Welcome to LedgerLite - Personal Finance Tracker");
        await RunMenuAsync(transactionService, reportService, config);
    }

    private static async Task RunMenuAsync(TransactionService transactionService, ReportService reportService, FileStorageConfig config)
    {
        while (true)
        {
            Console.WriteLine("\n--- Menu ---");
            Console.WriteLine("1. Add Transaction");
            Console.WriteLine("2. Edit Transaction (Not implemented)");
            Console.WriteLine("3. Delete Transaction (Not implemented)");
            Console.WriteLine("4. List Recent Transactions");
            Console.WriteLine("5. Generate Report");
            Console.WriteLine("6. Export Report (Run report first)");
            Console.WriteLine("7. Quit");
            Console.Write("Enter your choice (1-7): ");

            if (int.TryParse(Console.ReadLine() ?? string.Empty, out int choice) && choice >= 1 && choice <= 7)
            {
                try
                {
                    switch (choice)
                    {
                        case 1:
                            await AddTransactionAsync(transactionService);
                            break;
                        case 2:
                            await EditTransactionAsync(transactionService);
                            break;
                        case 3:
                            await DeleteTransactionAsync(transactionService);
                            break;
                        case 4:
                            await ListTransactionsAsync(transactionService);
                            break;
                        case 5:
                            await GenerateReportAsync(reportService, config);
                            break;
                        case 6:
                            Console.WriteLine("Please run option 5 (Report) first, then choose to export.");
                            break;
                        case 7:
                            Console.WriteLine("Goodbye!");
                            return;
                    }
                }
                catch (ArgumentException ex)
                {
                    Console.WriteLine($"Invalid input: {ex.Message}. Please try again.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}. Please try again.");
                }
            }
            else
            {
                Console.WriteLine("Invalid choice. Please enter a number between 1 and 7.");
            }
        }
    }

    private static async Task AddTransactionAsync(TransactionService service)
    {
        var type = ReadValidatedEnum<TransactionType>("Transaction Type (1=Income, 2=Expense): ", TransactionType.Income);

        var date = ReadValidatedDate("Date (yyyy-MM-dd) [today]: ", DateTime.Today);

        var description = ReadValidatedString("Description: ", string.Empty);

        var category = ReadValidatedString("Category: ", string.Empty);

        var amount = ReadValidatedDecimal("Amount (>0): ", 0m);

        var transaction = new Transaction(Guid.NewGuid(), date, description, category, amount, type);
        await service.AddTransactionAsync(transaction);
        Console.WriteLine("Transaction added successfully!");
    }

    private static async Task ListTransactionsAsync(TransactionService service)
    {
        var transactions = await service.GetAllTransactionsAsync();
        var recent = transactions.TakeLast(20).ToList();

        if (!recent.Any())
        {
            Console.WriteLine("No transactions found.");
            return;
        }

        Console.WriteLine("\nRecent Transactions:");
        Console.WriteLine("| ID (short) | Date       | Type    | Category     | Amount    | Description          |");
        Console.WriteLine(new string('-', 70));

        foreach (var t in recent)
        {
            var shortId = t.Id.ToString()[..8];
            var dateStr = t.Date.ToString("yyyy-MM-dd");
            var amountStr = t.Amount.ToString("C", CultureInfo.InvariantCulture);
            Console.WriteLine($"| {shortId,-10} | {dateStr,-10} | {t.Type,-7} | {t.Category,-12} | {amountStr,-10} | {t.Description,-20} |");
        }
    }

    private static async Task GenerateReportAsync(ReportService service, FileStorageConfig config)
    {
        var year = ReadValidatedInt("Year (e.g., 2023) [current]: ", 1900, 2100, DateTime.Now.Year);

        var month = ReadValidatedInt("Month (1-12) [current]: ", 1, 12, DateTime.Now.Month);

        var report = await service.GenerateMonthlyReportAsync(year, month);

        Console.WriteLine($"\nReport for {year}-{month:D2}:");
        Console.WriteLine($"Total Income: {report.TotalIncome:C}");
        Console.WriteLine($"Total Expense: {report.TotalExpense:C}");
        Console.WriteLine($"Net Balance: {report.Net:C}");
        Console.WriteLine($"Total Transactions: {report.TransactionCount}");

        if (report.TopCategories.Any())
        {
            Console.WriteLine("\nTop Expense Categories:");
            foreach (var cat in report.TopCategories)
            {
                Console.WriteLine($"  {cat.Category}: {cat.Amount:C}");
            }
        }
        else
        {
            Console.WriteLine("\nNo expense categories.");
        }

        Console.Write("Export to CSV? (y/n): ");
        if ((Console.ReadLine() ?? string.Empty).Trim().ToLower() == "y")
        {
            await CsvExporter.ExportReportAsync(report, year, month, config);
            var filename = $"Report_{year}_{month:D2}.csv";
            Console.WriteLine($"Report exported to exports/{filename}");
        }
    }

    private static async Task EditTransactionAsync(TransactionService service)
    {
        await ListTransactionsAsync(service);

        Console.Write("Enter short ID to edit (or 'q' to cancel): ");
        var input = Console.ReadLine()?.Trim();
        if (string.IsNullOrEmpty(input) || input.ToLower() == "q")
        {
            Console.WriteLine("Edit canceled.");
            return;
        }

        if (!Guid.TryParse(input, out Guid id) && input.Length >= 8)
        {
            // Try to find by short ID
            var allTx = await service.GetAllTransactionsAsync();
            var match = allTx.FirstOrDefault(t => t.Id.ToString().StartsWith(input, StringComparison.OrdinalIgnoreCase));
            if (match != null)
            {
                id = match.Id;
            }
        }

        var existing = await service.GetTransactionAsync(id);
        if (existing == null)
        {
            Console.WriteLine("Transaction not found. Please try again.");
            return;
        }

        // Reprompt fields
        var type = ReadValidatedEnum<TransactionType>("Type (1=Income, 2=Expense) [{existing.Type}]: ", existing.Type);
        var date = ReadValidatedDate("Date (yyyy-MM-dd) [{existing.Date:yyyy-MM-dd}]: ", existing.Date);
        var description = ReadValidatedString("Description [{existing.Description}]: ", existing.Description);
        var category = ReadValidatedString("Category [{existing.Category}]: ", existing.Category);
        var amount = ReadValidatedDecimal("Amount (>0) [{existing.Amount}]: ", existing.Amount);

        var updated = new Transaction(id, date, description, category, amount, type);
        await service.UpdateTransactionAsync(updated);
        Console.WriteLine("Transaction updated successfully!");
    }

    private static async Task DeleteTransactionAsync(TransactionService service)
    {
        await ListTransactionsAsync(service);

        Console.Write("Enter short ID to delete (or 'q' to cancel): ");
        var input = Console.ReadLine()?.Trim();
        if (string.IsNullOrEmpty(input) || input.ToLower() == "q")
        {
            Console.WriteLine("Delete canceled.");
            return;
        }

        if (!Guid.TryParse(input, out Guid id) && input.Length >= 8)
        {
            var allTx = await service.GetAllTransactionsAsync();
            var match = allTx.FirstOrDefault(t => t.Id.ToString().StartsWith(input, StringComparison.OrdinalIgnoreCase));
            if (match != null)
            {
                id = match.Id;
            }
        }

        Console.Write("Delete this transaction? (y/n): ");
        if ((Console.ReadLine() ?? string.Empty).Trim().ToLower() == "y")
        {
            await service.DeleteTransactionAsync(id);
            Console.WriteLine("Transaction deleted successfully!");
        }
        else
        {
            Console.WriteLine("Delete canceled.");
        }
    }

    private static TransactionType ReadValidatedEnum<TEnum>(string prompt, TransactionType defaultValue) where TEnum : struct, Enum
    {
        while (true)
        {
            Console.Write(prompt);
            var input = (Console.ReadLine() ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(input))
            {
                return defaultValue;
            }
            if (int.TryParse(input, out int choice) && Enum.IsDefined(typeof(TransactionType), choice - 1))
            {
                return (TransactionType)(choice - 1);
            }
            Console.WriteLine("Invalid type. Enter 1 or 2, or press Enter to keep current.");
        }
    }

    private static DateTime ReadValidatedDate(string prompt, DateTime defaultValue)
    {
        while (true)
        {
            Console.Write(prompt);
            var input = (Console.ReadLine() ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(input))
            {
                return defaultValue;
            }
            if (DateTime.TryParseExact(input, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
            {
                return date;
            }
            Console.WriteLine("Invalid date. Use yyyy-MM-dd, or press Enter to keep current.");
        }
    }

    private static string ReadValidatedString(string prompt, string defaultValue)
    {
        while (true)
        {
            Console.Write(prompt);
            var input = (Console.ReadLine() ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(input))
            {
                return defaultValue;
            }
            if (!string.IsNullOrWhiteSpace(input))
            {
                return input;
            }
            Console.WriteLine("Cannot be empty. Press Enter to keep current or enter value.");
        }
    }

    private static decimal ReadValidatedDecimal(string prompt, decimal defaultValue)
    {
        while (true)
        {
            Console.Write(prompt);
            var input = (Console.ReadLine() ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(input))
            {
                return defaultValue;
            }
            if (decimal.TryParse(input, NumberStyles.Currency, CultureInfo.InvariantCulture, out decimal amount) && amount > 0)
            {
                return amount;
            }
            Console.WriteLine("Invalid amount (>0). Press Enter to keep current or enter positive number.");
        }
    }

    private static int ReadValidatedInt(string prompt, int min, int max, int defaultValue = 0)
    {
        while (true)
        {
            Console.Write(prompt);
            var input = (Console.ReadLine() ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(input))
            {
                return defaultValue;
            }
            if (int.TryParse(input, out int value) && value >= min && value <= max)
            {
                return value;
            }
            Console.WriteLine($"Invalid value. Enter between {min} and {max}, or press Enter for default.");
        }
    }
}
