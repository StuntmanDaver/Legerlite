# Personal Finance Tracker (LedgerLite) — Product Requirements Document (PRD)

## 1. Product Summary

**Product name (working):** LedgerLite  
**Platform (v1):** .NET console app (C#)  
**Platform (v2):** Desktop UI (WinForms or WPF) on top of same core logic

**What it does:**  
LedgerLite lets a user record financial transactions (income and expenses), categorize them, store them locally, and generate simple reports (spend by month, top categories, etc.). Data is persisted locally (JSON first, then upgrade to SQLite/LiteDB). User can export summaries to CSV.

**Why it exists:**  
The point is not “a budgeting app.”  
The point is to learn to design, model, persist, query, and present data — which is 80/20 of real software. You’ll also practice separation of concerns so that the same business logic can be reused in a future GUI or API.

---

## 2. Goals / Non-Goals

### 2.1 Goals (must have for v1 console)
1. A user can add an income or expense transaction.
2. A user can edit or delete an existing transaction.
3. All transactions are saved and loaded between runs.
4. A user can run basic reports:
   - total spent in a given month
   - total income in a given month
   - net balance (income - expenses)
   - top 3 categories by spend in that month
5. A user can export a report to CSV.

### 2.2 Stretch Goals (nice to have in v1.5+ console)
1. Basic input validation (no negative amounts for income, date must parse, etc.).
2. Undo last action in current session.
3. Custom categories management (add/remove categories).

### 2.3 Non-Goals (explicitly out of scope for now)
- Bank account syncing / Plaid / API integrations.
- Multi-user accounts / auth / passwords.
- Cloud sync.
- Mobile app.
- AI insights.

---

## 3. User Personas

### Persona A: “The tracker”
Wants to track spending manually every day/week. Needs fast entry and simple summaries.

### Persona B: “The reviewer”
Does bulk entry at end of month and just wants to know “Where did money go?” / “Am I net positive?”

For this project, **you are both personas.** You’ll design UX for fast add and fast reporting.

---

## 4. Core Use Cases / User Stories

**US-01 Add transaction**  
As a user, I can add a transaction with:
- date
- description
- category
- amount
- type (Income or Expense)

So that I can log money movement.

**US-02 Edit transaction**  
As a user, I can update an existing transaction’s fields if I made a mistake.

**US-03 Delete transaction**  
As a user, I can remove a transaction that shouldn’t be there.

**US-04 View all transactions**  
As a user, I can list all transactions sorted by date, most recent first.

**US-05 Monthly report**  
As a user, I can ask:
- “Show me spending for September 2025”
And get:
- Total income that month  
- Total expenses that month  
- Net (income - expenses)  
- Top 3 categories by total spend  

**US-06 Export report**  
As a user, I can export that monthly report to CSV so I can open it in Excel/Sheets.

**US-07 Persistent storage**  
As a user, when I close and reopen the app, my data is still there.

---

## 5. Feature Requirements

### 5.1 Transaction Management
- Add Transaction
  - CLI flow: prompt user for each field
- Edit Transaction
  - User selects transaction by ID/index
- Delete Transaction
  - User selects transaction by ID/index
- List Transactions
  - Show last N (default 20) in a table-like format

Validation rules:
- `Amount` must be > 0
- `Type` must be `Income` or `Expense`
- `Date` must be parseable (YYYY-MM-DD)
- `Category` must be a non-empty string

### 5.2 Categories
- Predefined category set on first run:
  - Income: `Salary`, `Gift`, `Refund`, `Other`
  - Expense: `Rent`, `Groceries`, `EatingOut`, `Utilities`, `Transport`, `Entertainment`, `Health`, `Other`

- Stretch: allow user to add/remove categories later and persist that list separately.

### 5.3 Reporting
- Input: Year + Month (e.g. `2025-09`)
- Output:
  - Total income in that month
  - Total expenses in that month
  - Net balance
  - Top 3 categories (by total spend amount, descending)
  - Count of transactions in that period

- Display in console as text.
- Offer “Export to CSV? (y/n)”.

### 5.4 Export
- CSV format should include:
  - `category, total_spent`
  - `total_income`
  - `total_expense`
  - `net`

- CSV file is written to:  
  `exports/Report_YYYY_MM.csv`

- If `exports/` doesn’t exist, create it.

### 5.5 Persistence
**Phase 1 Storage = JSON file**
- File path: `data/transactions.json`
- On app start: load file if exists, else start empty list
- On data change (add/edit/delete): save file

**Phase 2 Storage = SQLite or LiteDB**
- Replace file I/O with DB context layer
- No UI changes: the rest of the app uses repository interfaces, so storage swap is clean

---

## 6. High-Level Architecture

We’re going to build like pros, even for a console app.  
We will separate business logic from UI and storage.

### 6.1 Layers

#### Domain Layer (Core Logic, no IO)
- Entities:
  - `Transaction`
  - `Category` (optional stretch)
- Value Objects / Enums:
  - `TransactionType` (Income, Expense)
- Business rules:
  - Calculations for reports
- Interfaces:
  - `ITransactionRepository`
  - `IReportService`

This layer should **not** know about JSON, files, console, or SQLite.

#### Infrastructure Layer (Persistence / File / DB)
- `JsonTransactionRepository : ITransactionRepository`
  - Knows where `transactions.json` lives
  - Handles serialization
- (Later) `SqliteTransactionRepository : ITransactionRepository`
  - Uses SQLite / EF Core
  - Swap-in with no changes to other layers

#### Application Layer (Use Cases / Services)
- `TransactionService`
  - Add/Edit/Delete/List logic
  - Calls repository
- `ReportService`
  - Monthly summaries, top categories, CSV generation

#### Presentation Layer (Console UI for v1)
- Menu, prompts, printing tables
- Converts user input (strings) → domain models
- Catches exceptions and prints friendly errors
- Calls Application Layer services

**Why this matters:**  
With this layout we can:
- replace console UI with WPF later
- expose the same services in a Web API
- change persistence from JSON → SQLite
with minimal rewrite.

That’s senior-level thinking.

---

## 7. Data Model

### 7.1 `Transaction`
Fields:
- `Id` (Guid)
- `Date` (DateTime)
- `Description` (string)
- `Category` (string)
- `Amount` (decimal)
- `Type` (enum TransactionType: Income or Expense)

Rules:
- `Amount` is always positive
- Interpretation:
  - `Income` increases balance
  - `Expense` decreases balance

### 7.2 Example JSON record
```json
{
  "Id": "4c7a5d1a-4d0b-4e1c-a7aa-0d57f6cf5e0a",
  "Date": "2025-09-14T00:00:00",
  "Description": "Groceries - Publix",
  "Category": "Groceries",
  "Amount": 82.45,
  "Type": "Expense"
}
```

### 7.3 Storage layout (Phase 1)

transactions.json:
```json
[
  {
    "Id": "guid-1",
    "Date": "2025-09-01T00:00:00",
    "Description": "Paycheck",
    "Category": "Salary",
    "Amount": 3200.00,
    "Type": "Income"
  },
  {
    "Id": "guid-2",
    "Date": "2025-09-02T00:00:00",
    "Description": "Rent",
    "Category": "Rent",
    "Amount": 1400.00,
    "Type": "Expense"
  }
]
```

## 8. Console UX / Interaction Flow

### Main menu (loop)
1. Add Transaction
2. Edit Transaction
3. Delete Transaction
4. List Recent Transactions
5. Run Monthly Report
6. Export Report to CSV
7. Quit

### 8.1 Add Transaction Flow

System asks:
- Type (Income / Expense)
- Date (YYYY-MM-DD)
- Description
- Category (show list, allow custom)
- Amount

Then:
- Validate
- Save
- Confirm: Transaction added. ID: {guid}

### 8.2 Edit Transaction Flow
- Show last N transactions with IDs/index
- Ask: “Which ID do you want to edit?”
- For each field, show current value and allow overwrite or [Enter] to keep
- Validate + save

### 8.3 Delete Transaction Flow
- Show last N with IDs/index
- Ask ID
- Confirm “Are you sure (y/n)?”
- Delete + save

### 8.4 List Transactions
- Print table:
  - ID (shortened, e.g. first 8 chars)
  - Date
  - Type
  - Category
  - Amount
  - Description
- Most recent first
- Show count

### 8.5 Monthly Report Flow
- Ask: “Enter year (YYYY):”
- Ask: “Enter month (1-12):”
- Output:
  - Total Income: $X
  - Total Expense: $Y
  - Net: $Z
  - Top Categories:
    - Groceries: $400
    - Rent: $1400
    - Transport: $120
- Ask: “Export to CSV? (y/n)”
  - If yes, write to exports/Report_2025_09.csv

## 9. Error Handling & Validation Requirements
- If the JSON file is missing or corrupted:
  - App should not crash.
  - App should warn: "Data file unreadable, starting with empty ledger."
  - Continue with an empty list in memory.
- If user enters malformed date:
  - Reprompt instead of throwing.
- If user enters non-numeric amount:
  - Reprompt.
- If user tries to edit/delete an ID that doesn’t exist:
  - Show "No transaction found with that ID."

We want graceful, predictable behavior. This trains production thinking.

## 10. Testing Requirements

We will write unit tests for:
1. **ReportService**
   - Given a list of transactions, can it:
     - calculate total expenses/income?
     - filter by month/year?
     - compute top spending categories correctly?
2. **TransactionService**
   - AddTransaction rejects invalid data (negative amount, invalid type, etc.)
   - EditTransaction actually persists updated values
   - DeleteTransaction actually removes the correct ID
3. **Serialization round-trip**
   - Save to JSON then load should preserve data faithfully
   - Decimal precision should not get mangled

We do **not** test console prompts in unit tests (I/O). We test logic and behavior.

**Framework:** xUnit  
**Mocks/Fakes:** in-memory implementation of ITransactionRepository for tests

## 11. Technical Stack / Libraries

### Runtime
- .NET 8 (LTS path, modern C# features)

### Language
- C#

### Persistence (Phase 1)
- System.Text.Json for serialization
- Local file storage

### Reporting
- LINQ for aggregation
- System.IO for CSV export

### Testing
- xUnit
- (Optional) FluentAssertions for nicer assertions
- Hand-rolled fakes for repos

### Phase 2+ (not day one but planned)
- SQLite or LiteDB persistence instead of flat file
- EF Core (if SQLite route)
- WPF/WinForms desktop client consuming the same TransactionService and ReportService

## 12. Milestones

### Milestone 0 – Repo Skeleton
- Create solution:
  - LedgerLite.Domain (entities/interfaces)
  - LedgerLite.Infrastructure (Json repository)
  - LedgerLite.Application (services)
  - LedgerLite.CLI (console UI)
  - LedgerLite.Tests (xUnit tests)
- Add project references

### Milestone 1 – Transactions CRUD (Console)
- Implement Transaction entity and TransactionService 
- Implement JsonTransactionRepository 
- Console can Add/List/Delete transactions
- Data persists to file

### Milestone 2 – Reporting
- Implement ReportService 
- Monthly summary works and is printed in console
- CSV export works

### Milestone 3 – Edit + Validation
- Implement edit transaction
- Add validation (date, amount, etc.)
- Improve console UX flow and error handling

### Milestone 4 – Tests
- Add xUnit tests for ReportService and TransactionService 
- Add in-memory fake repo for tests

### Milestone 5 – GUI Ready (Architecture, not UI yet)
- Confirm that none of the business logic depends on console
- Confirm services are cleanly injectable
- Prepare for LedgerLite.Desktop (WPF/WinForms) that calls the same Application layer services

## 13. Definition of Done for v1 (Console)
- I can run the console app and:
  - add income/expense
  - list my last 20 transactions
  - fix a typo in one
  - delete one
  - generate a September 2025 report
  - export that report to a CSV file
  - quit
- I can relaunch the app and the data is still there.
- I can run tests and they pass.

At that point, the project demonstrates:
- object modeling
- persistence abstraction
- LINQ data analysis
- clean business logic
- testability

## 14. Roadmap / Future Work
1. **SQLite swap**  
   Swap JSON to SQLite via EF Core. No change to the console UI. Demonstrates persistence-layer abstraction.
2. **Desktop UI (WPF or WinForms)**  
   Build a CRUD grid view + “Run Report” button UI. Reuse the same Application layer. Demonstrates MVVM-ish separation and desktop app skills.
3. **Import bank CSV**  
   Add a CLI option to bulk-import from a bank CSV export. Demonstrates mapping dirty external data into a clean internal model.
4. **Tag anomalies for audit**  
   Allow user to flag suspicious expenses. Later, an AI layer can generate insights from those flags.

## 15. Immediate Next Step

We begin with Milestone 0 & Milestone 1:
- Create the solution + projects.
- Define:
  - Transaction 
  - TransactionType 
  - ITransactionRepository 
  - TransactionService 
  - basic Program.cs menu loop
- Implement JsonTransactionRepository .

After that, the app will already be able to:
- Add a transaction
- Save to disk
- List transactions
- Delete transactions

From there we move to reporting.  
This is where you stop being “learning C# syntax” and start being “I ship software.”
