# LedgerLite Implementation Plan

This document outlines a detailed 50-step plan to implement the LedgerLite personal finance tracker, following the layered architecture specified in the PRD and .cursorrules. Steps are grouped by milestones for clarity. Each step includes a brief description of what to do and why.

## Milestone 0: Project Skeleton (Steps 1-8)
- [ ] **Step 1:** Create the solution file `LedgerLite.sln` in the root directory using `dotnet new sln -n LedgerLite`. This sets up the multi-project structure for clean separation of concerns.
- [ ] **Step 2:** Create the Domain project: `dotnet new classlib -n LedgerLite.Domain` and add it to the solution with `dotnet sln add LedgerLite.Domain`. This will hold pure domain models and interfaces.
- [ ] **Step 3:** Create the Application project: `dotnet new classlib -n LedgerLite.Application` and add to solution. This layer will coordinate use cases and validation.
- [ ] **Step 4:** Create the Infrastructure project: `dotnet new classlib -n LedgerLite.Infrastructure` and add to solution. This handles persistence and I/O.
- [ ] **Step 5:** Create the CLI project: `dotnet new console -n LedgerLite.CLI` and add to solution. This is the presentation layer for v1.
- [ ] **Step 6:** Create the Tests project: `dotnet new xunit -n LedgerLite.Tests` and add to solution. This ensures testable code from the start.
- [ ] **Step 7:** Add project references: CLI references Application (`dotnet add LedgerLite.CLI/LedgerLite.CLI.csproj reference LedgerLite.Application/LedgerLite.Application.csproj`), Application references Domain, Infrastructure references Domain and Application, Tests references all except CLI. This enforces dependency inversion.
- [ ] **Step 8:** Remove default Class1.cs files from all class libraries to clean up. Run `dotnet build` to verify the skeleton compiles without errors.

## Milestone 1: Domain Layer Basics (Steps 9-15)
- [ ] **Step 9:** In `LedgerLite.Domain`, create `Transaction.cs` as a record: `public record Transaction(Guid Id, DateTime Date, string Description, string Category, decimal Amount, TransactionType Type);`. This defines the core entity with immutability.
- [ ] **Step 10:** Add `TransactionType.cs` enum: `public enum TransactionType { Income, Expense };`. This encapsulates transaction classification.
- [ ] **Step 11:** Create `ITransactionRepository.cs` interface with methods: `Task AddAsync(Transaction transaction);`, `Task<List<Transaction>> GetAllAsync();` (sorted by Date descending), `Task<Transaction?> GetByIdAsync(Guid id);`, `Task UpdateAsync(Transaction transaction);`, `Task DeleteByIdAsync(Guid id);`. Use async for future-proofing, even if sync in v1.
- [ ] **Step 12:** Create `IReportService.cs` interface: `Task<ReportResult> GenerateMonthlyReportAsync(int year, int month);`. This abstracts reporting logic.
- [ ] **Step 13:** Define `ReportResult.cs` record: `public record ReportResult(decimal TotalIncome, decimal TotalExpense, decimal Net, List<CategoryAmount> TopCategories, int TransactionCount);`. This is the immutable result type for reports.
- [ ] **Step 14:** Create `CategoryAmount.cs` record: `public record CategoryAmount(string Category, decimal Amount);`. Used for top spending categories.
- [ ] **Step 15:** Add XML documentation to public members in Domain (e.g., ///<summary> for properties). Run `dotnet build` to ensure no errors.

## Milestone 1 Continued: Application Layer - TransactionService (Steps 16-22)
- [ ] **Step 16:** In `LedgerLite.Application`, create `TransactionService.cs` class with constructor injecting `ITransactionRepository repository`.
- [ ] **Step 17:** Implement `public async Task AddTransactionAsync(Transaction transaction)`: Validate (Amount > 0, non-empty strings, valid Type), throw `ArgumentException` if invalid, then call `repository.AddAsync(transaction)`.
- [ ] **Step 18:** Implement `public async Task<List<Transaction>> GetAllTransactionsAsync()`: Return `await repository.GetAllAsync()`.
- [ ] **Step 19:** Implement `public async Task UpdateTransactionAsync(Transaction transaction)`: Validate as in Add, find existing via GetById, if null throw NotFound, then `await repository.UpdateAsync(transaction)`.
- [ ] **Step 20:** Implement `public async Task DeleteTransactionAsync(Guid id)`: Call `await repository.DeleteByIdAsync(id)`.
- [ ] **Step 21:** Add validation helpers as private methods (e.g., `ValidateTransaction(Transaction t)`). Ensure business rules like positive Amount are enforced here.
- [ ] **Step 22:** Run `dotnet build` on Application project to verify.

## Milestone 1 Continued: Application Layer - ReportService (Steps 23-26)
- [ ] **Step 23:** In `LedgerLite.Application`, create `ReportService.cs` with constructor injecting `ITransactionRepository`.
- [ ] **Step 24:** Implement `GenerateMonthlyReportAsync`: Get all transactions, filter by year/month using LINQ (`Where(t => t.Date.Year == year && t.Date.Month == month)`), calculate totals (sum Income/Expense Amounts), net = income - expense, top categories (group Expenses by Category, sum Amount, order desc, take 3), count = filtered.Count().
- [ ] **Step 25:** Handle empty data: Totals 0, empty TopCategories.
- [ ] **Step 26:** Run `dotnet build` to verify ReportService.

## Milestone 1: Infrastructure Layer (Steps 27-35)
- [ ] **Step 27:** In `LedgerLite.Infrastructure`, create `FileStorageConfig.cs` record: `public record FileStorageConfig(string DataDirectory = "data", string ExportDirectory = "exports");` with constructor creating directories using `Directory.CreateDirectory`.
- [ ] **Step 28:** Install System.Text.Json if needed (it's built-in), create `JsonTransactionRepository.cs` implementing `ITransactionRepository`.
- [ ] **Step 29:** Private field: `private readonly List<Transaction> _transactions = new();`, `private readonly FileStorageConfig _config;`, constructor loads from file.
- [ ] **Step 30:** Implement `LoadFromFileAsync`: string path = Path.Combine(_config.DataDirectory, "transactions.json"); if File.Exists, read, deserialize JsonSerializer.Deserialize<List<Transaction>>(json), set _transactions, catch exceptions and log warning "Data file unreadable, starting empty.", set empty list.
- [ ] **Step 31:** Implement `SaveToFileAsync`: Ensure dir exists, JsonSerializer.Serialize(_transactions, options with indented), write to file.
- [ ] **Step 32:** `AddAsync`: _transactions.Add(transaction), await SaveToFileAsync().
- [ ] **Step 33:** `GetAllAsync`: return _transactions.OrderByDescending(t => t.Date).ToList().
- [ ] **Step 34:** `GetByIdAsync`: return _transactions.FirstOrDefault(t => t.Id == id).
- [ ] **Step 35:** `UpdateAsync`: var existing = await GetByIdAsync(transaction.Id); if null throw, remove existing, add updated, Save. `DeleteByIdAsync`: remove if found, Save.

## Milestone 1: CLI Basics (Steps 36-42)
- [ ] **Step 36:** In `LedgerLite.CLI`, update `Program.cs`: using statements for Application and Infrastructure, create config = new FileStorageConfig(), repo = new JsonTransactionRepository(config), services, then run menu loop.
- [ ] **Step 37:** Create `Menu.cs` class with fields for services, `Run()` method: while true, print menu options 1-7, read choice, switch on int.Parse(Console.ReadLine()).
- [ ] **Step 38:** Implement option 1: Add - prompt for Type (1=Income,2=Expense), Date (parse yyyy-MM-dd), Description, Category, Amount (decimal.Parse), create Transaction with Guid.NewGuid(), call service.AddAsync, print success.
- [ ] **Step 39:** Implement option 4: List - var txs = await service.GetAllTransactionsAsync(), print table (short Id, Date, Type, Category, Amount formatted, Description), limit to last 20.
- [ ] **Step 40:** Implement option 3: Delete - list recent, prompt Id (Guid.Parse), await service.DeleteTransactionAsync(id), print success or not found.
- [ ] **Step 41:** Create `InputParser.cs` for helper methods like ParseDate, ParseDecimal with reprompt on invalid.
- [ ] **Step 42:** In Menu, use InputParser for all inputs, handle exceptions with user-friendly messages, no crashes.

## Milestone 2: Reporting and Export (Steps 43-47)
- [ ] **Step 43:** In Menu, add option 5: Report - prompt year (int), month (1-12 validate), var result = await reportService.GenerateMonthlyReportAsync(year, month), print totals, net, top categories, count.
- [ ] **Step 44:** In Menu, after report, prompt "Export to CSV? (y/n)", if y, generate filename "exports/Report_{year}_{month:D2}.csv", call CsvExporter.Export(result, filename).
- [ ] **Step 45:** In Infrastructure, implement `CsvExporter.Export(ReportResult result, string path)`: Use StringBuilder, append headers/summary rows, category rows, write with File.WriteAllText, create export dir.
- [ ] **Step 46:** Add option 2: Edit - list recent, prompt Id, get tx = await service.GetByIdAsync(id), if null error, prompt new values (or keep current), create updated Transaction, await service.UpdateAsync.
- [ ] **Step 47:** Add option 7: Quit - break loop.

## Milestone 3-4: Validation, Tests, Polish (Steps 48-50)
- [ ] **Step 48:** Enhance validation in TransactionService: Use DateTime.TryParseExact for dates (yyyy-MM-dd), trim strings, throw specific exceptions like InvalidAmountException.
- [ ] **Step 49:** In Tests, create FakeTransactionRepository implementing ITransactionRepository with in-memory list for mocking. Write TransactionServiceTests: Add success/fail, Update, Delete using fake repo and Assert.
- [ ] **Step 50:** Write ReportServiceTests: Setup fake with sample transactions, assert GenerateMonthlyReport returns correct totals, top categories, handles empty. Run `dotnet test` to verify all pass. Update README with build/run instructions.
