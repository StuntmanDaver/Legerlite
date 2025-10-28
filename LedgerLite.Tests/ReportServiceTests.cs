using LedgerLite.Application;
using LedgerLite.Domain;
using LedgerLite.Tests;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace LedgerLite.Tests;

public class ReportServiceTests
{
    private readonly FakeTransactionRepository _fakeRepo;
    private readonly ReportService _service;

    public ReportServiceTests()
    {
        _fakeRepo = new FakeTransactionRepository();
        _service = new ReportService(_fakeRepo);
    }

    [Fact]
    public async Task GenerateMonthlyReportAsync_WithData_ReturnsCorrectReport()
    {
        var now = DateTime.Now;
        var year = now.Year;
        var month = now.Month;

        var income1 = new Transaction(Guid.NewGuid(), new DateTime(year, month, 1), "Salary", "Income", 3000m, TransactionType.Income);
        var income2 = new Transaction(Guid.NewGuid(), new DateTime(year, month, 15), "Bonus", "Income", 500m, TransactionType.Income);
        var expense1 = new Transaction(Guid.NewGuid(), new DateTime(year, month, 5), "Groceries", "Food", 200m, TransactionType.Expense);
        var expense2 = new Transaction(Guid.NewGuid(), new DateTime(year, month, 10), "Rent", "Housing", 1200m, TransactionType.Expense);
        var expense3 = new Transaction(Guid.NewGuid(), new DateTime(year, month, 20), "Utilities", "Housing", 150m, TransactionType.Expense);

        await _fakeRepo.AddAsync(income1);
        await _fakeRepo.AddAsync(income2);
        await _fakeRepo.AddAsync(expense1);
        await _fakeRepo.AddAsync(expense2);
        await _fakeRepo.AddAsync(expense3);

        var report = await _service.GenerateMonthlyReportAsync(year, month);

        Assert.Equal(3500m, report.TotalIncome);
        Assert.Equal(1550m, report.TotalExpense);
        Assert.Equal(1950m, report.Net);
        Assert.Equal(5, report.TransactionCount);
        Assert.Equal(2, report.TopCategories.Count);
        Assert.Equal("Housing", report.TopCategories[0].Category);
        Assert.Equal(1350m, report.TopCategories[0].Amount); // Rent + Utilities
        Assert.Equal("Food", report.TopCategories[1].Category);
        Assert.Equal(200m, report.TopCategories[1].Amount);
    }

    [Fact]
    public async Task GenerateMonthlyReportAsync_EmptyMonth_ReturnsZeros()
    {
        var report = await _service.GenerateMonthlyReportAsync(2020, 1); // No data

        Assert.Equal(0m, report.TotalIncome);
        Assert.Equal(0m, report.TotalExpense);
        Assert.Equal(0m, report.Net);
        Assert.Equal(0, report.TransactionCount);
        Assert.Empty(report.TopCategories);
    }

    [Fact]
    public async Task GenerateMonthlyReportAsync_WithMixedMonths_FiltersCorrectly()
    {
        var now = DateTime.Now;
        var year = now.Year;
        var month = now.Month;

        // Current month income
        var currentIncome = new Transaction(Guid.NewGuid(), new DateTime(year, month, 1), "Salary", "Income", 3000m, TransactionType.Income);

        // Previous month expense
        var prevMonth = month - 1;
        if (prevMonth == 0) { prevMonth = 12; year--; }
        var prevExpense = new Transaction(Guid.NewGuid(), new DateTime(year, prevMonth, 1), "Rent", "Housing", 1200m, TransactionType.Expense);

        await _fakeRepo.AddAsync(currentIncome);
        await _fakeRepo.AddAsync(prevExpense);

        var report = await _service.GenerateMonthlyReportAsync(now.Year, now.Month);

        Assert.Equal(3000m, report.TotalIncome);
        Assert.Equal(0m, report.TotalExpense);
        Assert.Equal(3000m, report.Net);
        Assert.Equal(1, report.TransactionCount);
        Assert.Empty(report.TopCategories);
    }
}
