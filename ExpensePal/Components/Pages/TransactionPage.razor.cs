using ExpensePal.Model;
using ExpensePal.Services;
using Microsoft.AspNetCore.Components;
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
        private string filterType;
        private bool isModalOpen = false;
        private string sortOrder = "asc";  // Default sort order

        // Custom Tag and Predefined Tags Handling
        private List<string> availableTags = new List<string> { "Work", "Food", "Entertainment", "Health", "Yearly", "Monthly", "Drinks", "Clothes", "Gadgets", "Miscellaneous", "Fuel", "Rent", "EMI", "Party" };
        private string customTag { get; set; } = ""; // For custom tag input
        private string filterTags { get; set; } = ""; // For filtering by tags
        private List<string> addedTags = new List<string>(); // List of added tags
        private List<string> selectedTags = new();
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
            try
            {
                transactionList = _transactionService.LoadTransactions();
                debtList = _transactionService.LoadDebts();
                filteredTransactions = transactionList;
                CalculateTotals();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing TransactionPage: {ex.Message}");
            }
        }

        private void FilterTransactions()
        {
            try
            {
                filteredTransactions = _transactionService.FilterTransactions(transactionList, filterTitle, filterTags, filterType, sortOrder);

                // Apply date range filter if dates are selected
                if (fromDate.HasValue)
                {
                    filteredTransactions = filteredTransactions.Where(t => t.Date >= fromDate.Value).ToList();
                }
                if (toDate.HasValue)
                {
                    filteredTransactions = filteredTransactions.Where(t => t.Date <= toDate.Value).ToList();
                }

                CalculateTotals();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error filtering transactions: {ex.Message}");
            }
        }

        private void ClearFilters()
        {
            filterTitle = null;
            filterTags = null;
            filterType = null;
            fromDate = null;
            toDate = null;
            sortOrder = "asc";
            filteredTransactions = transactionList;
            CalculateTotals();
        }

        private void HandleTagSelection(ChangeEventArgs e)
        {
            if (e.Value is IEnumerable<object> selectedOptions)
            {
                selectedTags = selectedOptions.Cast<string>().ToList();
            }
        }

        private async Task AddTransaction()
        {
            try
            {
                if (newTransaction.Type == "Expense" && newTransaction.Amount > AvailableBalance)
                {
                    await JS.InvokeVoidAsync("alert", "Insufficient balance");
                    return;
                }

                if (selectedTags.Any())
                {
                    newTransaction.Tags = string.Join(",", selectedTags);
                }

                transactionList.Add(newTransaction);
                _transactionService.SaveTransactions(transactionList);
                newTransaction = new Transaction();
                FilterTransactions();
                CloseModal();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding transaction: {ex.Message}");
            }
        }

        private void OpenModal() => isModalOpen = true;
        private void CloseModal() => isModalOpen = false;

        private void CalculateTotals()
        {
            try
            {
                TotalIncome = _transactionService.CalculateTotal(transactionList, "Income");
                TotalExpense = _transactionService.CalculateTotal(transactionList, "Expense");

                TotalDebt = _transactionService.CalculateTotalDebt(debtList.Where(d => d.Status == "Pending").ToList());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calculating totals: {ex.Message}");
            }
        }

        // Method to add a custom tag
        private void AddCustomTag()
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(customTag) && !availableTags.Contains(customTag))
                {
                    availableTags.Add(customTag);
                    customTag = "";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding custom tag: {ex.Message}");
            }
        }
    }
}
