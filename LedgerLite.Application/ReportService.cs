using LedgerLite.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LedgerLite.Application;

public class ReportService : IReportService
{
    private readonly ITransactionRepository _repository;

    public ReportService(ITransactionRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<ReportResult> GenerateMonthlyReportAsync(int year, int month)
    {
        var allTransactions = await _repository.GetAllAsync();
        var filtered = allTransactions
            .Where(t => t.Date.Year == year && t.Date.Month == month)
            .ToList();

        var incomeTransactions = filtered.Where(t => t.Type == TransactionType.Income);
        var expenseTransactions = filtered.Where(t => t.Type == TransactionType.Expense);

        var totalIncome = incomeTransactions.Sum(t => t.Amount);
        var totalExpense = expenseTransactions.Sum(t => t.Amount);
        var net = totalIncome - totalExpense;

        var topCategories = expenseTransactions
            .GroupBy(t => t.Category)
            .Select(g => new CategoryAmount(g.Key, g.Sum(t => t.Amount)))
            .OrderByDescending(c => c.Amount)
            .Take(3)
            .ToList();

        var transactionCount = filtered.Count;

        return new ReportResult(totalIncome, totalExpense, net, topCategories, transactionCount);
    }
}
