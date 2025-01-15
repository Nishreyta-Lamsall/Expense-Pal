using ExpensePal.Model;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Text.Json;

namespace ExpensePal.Components.Pages
{
    public partial class Dashboard
    {
        private List<Transaction> Transactions = new List<Transaction>();
        private List<Debt> Debts = new List<Debt>();
        private List<Transaction> FilteredTransactions = new List<Transaction>();
        private List<Debt> FilteredDebts = new List<Debt>();

        private decimal totalIncome;
        private decimal totalExpense;
        private decimal totalDebt;
        private decimal paidDebt;

        public decimal TotalIncome => totalIncome;
        public decimal TotalExpense => totalExpense;
        public decimal TotalDebt => totalDebt;

        public decimal PaidDebt => paidDebt;

        public decimal AvailableBalance => TotalIncome + TotalDebt - TotalExpense;

        private static readonly string TransactionFilePath = Path.Combine(FileSystem.AppDataDirectory, "transaction.json");
        private static readonly string DebtFilePath = Path.Combine(FileSystem.AppDataDirectory, "debt.json");

        private DateTime? StartDateTransaction { get; set; }
        private DateTime? EndDateTransaction { get; set; }

        private DateTime? StartDateDebt { get; set; }
        private DateTime? EndDateDebt { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await LoadTransactionsAndDebts();
            CalculateTotals();
            ShowAllDebts();
            ShowHighestTransactions();
        }

        private async Task LoadTransactionsAndDebts()
        {
            if (File.Exists(TransactionFilePath))
            {
                var json = await File.ReadAllTextAsync(TransactionFilePath);
                Transactions = JsonSerializer.Deserialize<List<Transaction>>(json) ?? new List<Transaction>();
            }

            if (File.Exists(DebtFilePath))
            {
                var json = await File.ReadAllTextAsync(DebtFilePath);
                Debts = JsonSerializer.Deserialize<List<Debt>>(json) ?? new List<Debt>();
            }
            StateHasChanged();
        }

        private void CalculateTotals()
        {
            totalIncome = Transactions.Where(t => t.Type == "Income").Sum(t => t.Amount);
            totalExpense = Transactions.Where(t => t.Type == "Expense").Sum(t => t.Amount);
            totalDebt = Debts.Where(d => d.Status == "Pending").Sum(d => d.Amount);
            paidDebt = Debts.Where(d => d.Status == "Paid").Sum(d => d.Amount);
            StateHasChanged();
        }

        private void ShowHighestTransactions()
        {
            FilteredTransactions = Transactions.OrderByDescending(t => t.Amount).Take(5).ToList();
        }

        private void ShowLowestTransactions()
        {
            FilteredTransactions = Transactions.OrderBy(t => t.Amount).Take(5).ToList();
        }

        private void ShowHighestDebts()
        {
            FilteredDebts = Debts.OrderByDescending(d => d.Amount).Take(5).ToList();
        }

        private void ShowLowestDebts()
        {
            FilteredDebts = Debts.OrderBy(d => d.Amount).Take(5).ToList();
        }

        private void ShowOverdue()
        {
            FilteredDebts = Debts.Where(d => d.Status == "Pending").ToList();
        }

        private void ShowAllDebts()
        {
            FilteredDebts = Debts;
        }

        private void FilterTransactionsByDate()
        {
            FilteredTransactions = Transactions
                .Where(t => (!StartDateTransaction.HasValue || t.Date >= StartDateTransaction.Value) &&
                            (!EndDateTransaction.HasValue || t.Date <= EndDateTransaction.Value))
                .ToList();
        }

        private void FilterDebtsByDate()
        {
            FilteredDebts = Debts
                .Where(d => (!StartDateDebt.HasValue || d.DueDate >= StartDateDebt.Value) &&
                            (!EndDateDebt.HasValue || d.DueDate <= EndDateDebt.Value))
                .ToList();
        }

    }
} 