# LedgerLite

LedgerLite is a local-first personal finance tracker written in C#/.NET.

You enter income and expenses, categorize them, and generate monthly reports (spend by category, total in/out, net). Data is stored on disk (JSON in v1, upgradable to SQLite/LiteDB), and you can export summaries to CSV.

This project is intentionally built like real production software:
- Clean separation between domain logic, persistence, and UI.
- Testable services.
- Storage abstraction so we can swap JSON → SQLite with minimal changes.
- Console UI first, then WPF/WinForms desktop on top of the same core.

This is not a toy exercise. This is training you to be employable in C#/.NET.

## 1. Project Goals

### Core goals
- Add/edit/delete transactions
- Persist transactions locally
- Generate monthly spending/income reports
- Export report to CSV
- Be cleanly unit testable

### Stretch
- Custom categories
- Undo last action
- Desktop GUI (WPF/WinForms)
- Swap persistence to SQLite/EF Core

### Explicit non-goals (for v1)
- Bank syncing / Plaid
- Multi-user auth
- Cloud sync
- AI insights
- Mobile app

## 2. Features

### Transactions
- Add an income or expense
- Edit fields on an existing transaction
- Delete a transaction
- View recent transactions in a table

Each transaction includes:
- `Id` (Guid)
- `Date` (DateTime)
- `Description` (string)
- `Category` (string)
- `Amount` (decimal, always positive)
- `Type` (`Income` or `Expense`)

### Reporting
- Ask "What did I spend in Month X / Year Y?"
- Get:
  - Total income
  - Total expenses
  - Net (income - expenses)
  - Top 3 categories by spend
  - Count of transactions
- Optionally export to CSV

### Persistence
- v1: JSON file at `data/transactions.json`
- v2: SQLite or LiteDB
- The rest of the code talks to storage through an interface, so we can swap the backend without breaking UI/business logic

### Export
- Writes CSV to `exports/Report_YYYY_MM.csv`
- CSV includes per-category totals and summary numbers

## 3. Tech Stack

### Runtime
- .NET 8
- C#

### Storage
- `System.Text.Json` for serialization
- Local file system for persistence

### Application / Business Logic
- Domain models + services for:
  - Transaction CRUD
  - Reporting (LINQ aggregation, filtering by month)
  - CSV generation

### UI
- Initial UI: Console app (menu-driven)
- Future UI: WPF/WinForms desktop client that reuses the same services

### Testing
- xUnit for unit tests
- Fake in-memory repositories to test logic without touching disk

## 4. Solution Structure

We're using a multi-project solution even for a "simple console app."  
Reason: This is how you learn real architecture.

```
LedgerLite.sln
├─ LedgerLite.Domain/          // Pure domain models & interfaces
│  ├─ Transaction.cs
│  ├─ TransactionType.cs
│  ├─ ITransactionRepository.cs
│  └─ IReportService.cs
├─ LedgerLite.Application/     // Use case services (logic)
│  ├─ TransactionService.cs
│  └─ ReportService.cs
├─ LedgerLite.Infrastructure/  // Persistence / IO / external concerns
│  ├─ JsonTransactionRepository.cs
│  ├─ FileStorageConfig.cs
│  └─ CsvExporter.cs
├─ LedgerLite.CLI/             // Console frontend (presentation layer)
│  ├─ Program.cs
│  ├─ Menu.cs
│  └─ InputParser.cs
└─ LedgerLite.Tests/           // Automated tests
   ├─ TransactionServiceTests.cs
   ├─ ReportServiceTests.cs
   └─ JsonTransactionRepositoryTests.cs
```

### Responsibilities by layer

**LedgerLite.Domain**
- Knows what a Transaction is
- Knows what a ReportResult is
- Declares the ITransactionRepository contract
- Has no idea where data comes from

**LedgerLite.Application**
- Coordinates behavior
- Applies validation / rules
- Asks repositories to save/load
- Produces report summaries

**LedgerLite.Infrastructure**
- Knows how to actually write/read JSON to disk
- Knows how to write CSV exports
- Will later know how to talk to SQLite

**LedgerLite.CLI**
- Collects user input
- Displays tables / summaries
- Calls services in Application layer
- Handles "bad input, try again" instead of crashing

**LedgerLite.Tests**
- Proves the logic is correct and stays correct

## 5. How to Run (v1 Console App)

1. Install .NET 8 SDK.
2. Clone the repo.
3. Restore + build:
   ```
   dotnet restore
   dotnet build
   ```
4. Run the console app:
   ```
   dotnet run --project LedgerLite.CLI
   ```
5. You should see a menu similar to:
   1. Add Transaction
   2. Edit Transaction
   3. Delete Transaction
   4. List Recent Transactions
   5. Run Monthly Report
   6. Export Monthly Report to CSV
   7. Quit
6. Enter a number to perform an action.

## 6. Data Storage

### Default path

Transactions are persisted to:

`data/transactions.json`

### Behavior

On startup:
- If the file exists and is valid, load it.
- If it doesn’t exist, start empty.
- If it’s corrupted, app should warn and start with an empty in-memory list (no crash).

On every add/edit/delete:
- The repository writes the entire updated list back to disk.

This keeps persistence logic contained to the Infrastructure layer.

### Example transactions.json

```json
[
  {
    "Id": "4c7a5d1a-4d0b-4e1c-a7aa-0d57f6cf5e0a",
    "Date": "2025-09-14T00:00:00",
    "Description": "Groceries - Publix",
    "Category": "Groceries",
    "Amount": 82.45,
    "Type": "Expense"
  },
  {
    "Id": "e204f6dc-1bd5-49e8-a0a0-748e4d2043cf",
    "Date": "2025-09-15T00:00:00",
    "Description": "Paycheck September",
    "Category": "Salary",
    "Amount": 3200.00,
    "Type": "Income"
  }
]
```

## 7. Monthly Report Output

When you run a monthly report, you’ll be asked:
- Year (YYYY)
- Month (1-12)

The console then prints:
- Total Income
- Total Expenses
- Net (Income - Expenses)
- Top 3 Spending Categories:
  - Category name
  - Total spent in that category during that month
- Number of transactions in that month

You’ll then be prompted:
- Export to CSV? (y/n)

If "y", a file like this will be created:

`exports/Report_2025_09.csv`

The CSV will contain category totals and summary metrics so you can open it in Excel or Google Sheets.

## 8. Testing

Run tests:

```
dotnet test
```

### What we test:
- Report math (totals, net, top categories)
- Filtering by month/year
- CRUD logic (add/edit/delete) in TransactionService
- JSON round-trip serialization (no precision loss in decimal, dates load correctly)

### What we don’t test:
- Console input/output formatting
- Manual interaction / prompts

Why: unit tests should test logic, not your typing skills.

## 9. Roadmap

### Phase 1: Console App (MVP)
- CLI menu for CRUD
- JSON-backed persistence
- Monthly reporting
- CSV export
- Basic unit tests

### Phase 2: Storage Upgrade
- Add SqliteTransactionRepository using SQLite or LiteDB
- Keep the same ITransactionRepository interface
- Switch implementations via config

This proves you understand persistence abstraction.

### Phase 3: Desktop UI
- Add LedgerLite.Desktop using WPF or WinForms
- Reuse TransactionService and ReportService for logic
- UI becomes:
  - transaction grid
  - "Run Report" button
  - "Export CSV" button
- You are now showing MVVM/MVP separation

### Phase 4: Importer
- Add "Import CSV from bank" command
- Map / clean external CSV columns into internal Transaction objects
- Bulk insert

This demonstrates basic ETL/data engineering skills.

## 10. Why This Project Matters (Career Positioning)

By the time Phase 2 or 3 is done, you can honestly say in an interview:
- You’ve designed and implemented a layered .NET application.
- You’ve handled persistence, serialization, and data integrity.
- You’ve produced analytical reports using LINQ and domain logic instead of hardcoding.
- You’ve written and run unit tests against business logic.
- You’ve cleanly separated UI from logic and logic from storage.

That jumps you out of "beginner" and into "I can ship production-grade internal tools," which is what most C#/.NET jobs actually are.

## 11. Status / Next Step

Immediate next action is to scaffold the solution and core classes:

1. Create the .sln and projects (Domain, Application, Infrastructure, CLI, Tests)
2. Add these initial types:
   - Transaction
   - TransactionType
   - ITransactionRepository
   - TransactionService
3. Build the first console menu in Program.cs that can:
   - Add a transaction
   - List recent transactions
   - Save/load from transactions.json

After that baseline runs end-to-end, we add:
- delete/edit
- monthly report
- CSV export
- tests

You get something working quickly, and then you tighten it like an engineer, not like a tutorial.
