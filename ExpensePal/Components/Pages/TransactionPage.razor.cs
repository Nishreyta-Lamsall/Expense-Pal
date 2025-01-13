using ExpensePal.Model;
using ExpensePal.Services;
using Microsoft.JSInterop;

namespace ExpensePal.Components.Pages
{
    public partial class TransactionPage
    {
        private readonly TransactionService _transactionService;
        private List<Transaction> transactionList;
        private List<Transaction> filteredTransactions;
        private List<Debt> debtList;
        private Transaction newTransaction = new();
        private DateTime? fromDate;
        private DateTime? toDate;
        private string filterTitle;
        private string filterTags;
        private string filterType;
        private bool isModalOpen = false;
        private string sortOrder = "asc";  // Default sort order

        public decimal TotalIncome { get; private set; }
        public decimal TotalExpense { get; private set; }
        public decimal TotalDebt { get; private set; }
        public decimal AvailableBalance => TotalIncome + TotalDebt - TotalExpense;

        public int TotalTransactions => filteredTransactions?.Count ?? 0;

        public TransactionPage()
        {
            _transactionService = new TransactionService(
                Path.Combine(FileSystem.AppDataDirectory, "transaction.json"),
                Path.Combine(FileSystem.AppDataDirectory, "debt.json"));
        }

        protected override void OnInitialized()
        {
            transactionList = _transactionService.LoadTransactions();
            debtList = _transactionService.LoadDebts();
            filteredTransactions = transactionList;
            CalculateTotals();
        }

        private void FilterTransactions()
        {
            filteredTransactions = _transactionService.FilterTransactions(transactionList, filterTitle, filterTags, filterType, sortOrder);
            CalculateTotals();
        }

        private void ClearFilters()
        {
            filterTitle = null;
            filterTags = null;
            filterType = null;
            sortOrder = "asc";
            filteredTransactions = transactionList;
            CalculateTotals();
        }

        private async Task AddTransaction()
        {
            if (newTransaction.Type == "Expense" && newTransaction.Amount > AvailableBalance)
            {
                await JS.InvokeVoidAsync("alert", "Insufficient balance");
                return;
            }

            transactionList.Add(newTransaction);
            _transactionService.SaveTransactions(transactionList);
            newTransaction = new Transaction();
            FilterTransactions();
            CloseModal();
        }

        private void OpenModal() => isModalOpen = true;
        private void CloseModal() => isModalOpen = false;

        private void CalculateTotals()
        {
            TotalIncome = _transactionService.CalculateTotal(transactionList, "Income");
            TotalExpense = _transactionService.CalculateTotal(transactionList, "Expense");
            TotalDebt = _transactionService.CalculateTotalDebt(debtList);
        }
    }
}
