using ExpensePal.Model;
using ExpensePal.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpensePal.Components.Pages
{
    public partial class TransactionPage
    {
        // Dependency injection 
        [Inject] private TransactionService _transactionService { get; set; }

        // Local variables to hold the list of transactions and debts
        private List<Transaction> transactionList = new();
        private List<Transaction> filteredTransactions = new();
        private List<Debt> debtList = new();
        private Transaction newTransaction = new();

        // Filter and sorting parameters for transactions
        private string filterTitle;
        private string filterTags;
        private string filterType;
        private string sortOrder = "Ascending";
        private DateTime? fromDate;
        private DateTime? toDate;
        private bool isModalOpen = false;

        // Custom tag and tag selection for transactions
        private string customTag = "";
        private List<string> selectedTags = new();
        private List<string> availableTags = new()
        {
            "Work", "Food", "Entertainment", "Health", "Yearly", "Monthly",
            "Drinks", "Clothes", "Gadgets", "Miscellaneous", "Fuel", "Rent", "EMI", "Party"
        };

        public decimal TotalIncome { get; private set; }
        public decimal TotalExpense { get; private set; }
        public decimal TotalDebt { get; private set; }
        public decimal AvailableBalance => TotalIncome + TotalDebt - TotalExpense;

        public int TotalTransactions => filteredTransactions.Count;

        // Lifecycle method that initializes the page, loading data
        protected override async Task OnInitializedAsync()
        {
            try
            {
                transactionList = await _transactionService.LoadTransactionsAsync();
                debtList = await _transactionService.LoadDebtsAsync();
                filteredTransactions = transactionList;
                UpdateTotals();
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
                filteredTransactions = _transactionService.FilterTransactions(
                    transactionList, filterTitle, filterTags, filterType, fromDate, toDate, sortOrder
                );
                UpdateTotals();
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
            UpdateTotals();
        }

        // Method to add a new transaction after validation
        private async Task AddTransaction()
        {
            try
            {
                if (newTransaction.Type == "Expense" && newTransaction.Amount > AvailableBalance)
                {
                    await JS.InvokeVoidAsync("alert", "Insufficient balance.");
                    return;
                }

                if (selectedTags.Any())
                {
                    newTransaction.Tags = string.Join(",", selectedTags);
                }

                transactionList = await _transactionService.AddTransactionAsync(newTransaction, transactionList);
                newTransaction = new Transaction();
                FilterTransactions();
                CloseModal();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding transaction: {ex.Message}");
            }
        }

        private void UpdateTotals()
        {
            TotalIncome = _transactionService.CalculateTotal(transactionList, "Income");
            TotalExpense = _transactionService.CalculateTotal(transactionList, "Expense");
            TotalDebt = _transactionService.CalculateTotalDebt(debtList.Where(d => d.Status == "Pending").ToList());
        }

        private void AddCustomTag()
        {
            if (!string.IsNullOrWhiteSpace(customTag) && !availableTags.Contains(customTag))
            {
                availableTags.Add(customTag);
                customTag = string.Empty;
            }
        }

        private void HandleTagSelection(ChangeEventArgs e)
        {
            if (e.Value is IEnumerable<object> selectedOptions)
            {
                selectedTags = selectedOptions.Cast<string>().ToList();
            }
        }

        // Method to open the modal for adding a new transaction
        private void OpenModal() => isModalOpen = true;

        // Method to close the modal
        private void CloseModal() => isModalOpen = false;
    }
}
