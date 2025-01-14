using ExpensePal.Model;
using ExpensePal.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace ExpensePal.Components.Pages
{
    public partial class DebtPage
    {
        private readonly DebtService _debtService;
        private List<Debt> debtList = new();
        private List<Debt> filteredDebts = new();
        private Debt newDebt = new();
        private string filterTitle;
        private string filterStatus;
        private string sortOrder = "Ascending";
        private bool isModalOpen = false;

        private decimal totalDebt;

        public decimal TotalDebt => totalDebt;

        public DebtPage()
        {
            _debtService = new DebtService();
        }

        protected override void OnInitialized()
        {
            debtList = _debtService.LoadDebts();
            filteredDebts = debtList;
            CalculateTotals();
        }

        private void FilterDebts()
        {
            filteredDebts = _debtService.FilterDebts(debtList, filterTitle, filterStatus, sortOrder);
            CalculateTotals();
        }

        private void ClearFilters()
        {
            filterTitle = null;
            filterStatus = null;
            sortOrder = "Ascending";
            filteredDebts = debtList;
            CalculateTotals();
        }

        private async Task AddDebt()
        {
            debtList.Add(newDebt);
            newDebt = new Debt();
            FilterDebts();
            _debtService.SaveDebts(debtList);
            CloseModal();
        }

        private decimal CalculateTotalIncome()
        {
            string transactionFilePath = Path.Combine(FileSystem.AppDataDirectory, "transaction.json");

            if (File.Exists(transactionFilePath))
            {
                var json = File.ReadAllText(transactionFilePath);

                if (!string.IsNullOrWhiteSpace(json))
                {
                    try
                    {
                        Console.WriteLine("Read transaction.json: " + json);

                        var transactions = JsonSerializer.Deserialize<List<Transaction>>(json);

                        if (transactions == null)
                        {
                            Console.WriteLine("Error: Transactions deserialization failed.");
                            return 0;
                        }

                        var totalIncome = transactions
                            .Where(t => t.Type == "Income")
                            .Sum(t => t.Amount); // Sum all income amounts

                        Console.WriteLine("Total Income: " + totalIncome);
                        return totalIncome;
                    }
                    catch (JsonException ex)
                    {
                        Console.WriteLine("Error reading transaction.json: " + ex.Message);
                    }
                }
            }

            return 0; // Return 0 if file doesn't exist or has errors
        }




        private void MarkAsPaid(Debt debt)
        {
            // Calculate total income from transaction.json
            var totalIncome = CalculateTotalIncome();

            // Check if income is sufficient to pay the debt
            if (totalIncome < debt.Amount)
            {
                // Show an error message (you can replace this with a UI notification)
                JS.InvokeVoidAsync("showAlert", "Insufficient balance");
                return;
            }

            // Deduct the debt amount from total income
            totalIncome -= debt.Amount;

            // Update the transaction data (assuming you are saving the new income after deduction)
            UpdateTransactionFile(totalIncome);

            // Mark the debt as paid
            debt.Status = "Paid";

            // Save the updated debts
            _debtService.SaveDebts(debtList);

            // Recalculate total debt
            CalculateTotals();

            // Recalculate total income immediately to ensure it's updated
            decimal updatedIncome = CalculateTotalIncome();

            // Log updated income for debugging
            Console.WriteLine($"Updated Total Income after payment: {updatedIncome}");

            // Log success or update UI
            Console.WriteLine("Debt paid successfully.");
        }



        private void UpdateTransactionFile(decimal newTotalIncome)
        {
            string transactionFilePath = Path.Combine(FileSystem.AppDataDirectory, "transaction.json");

            if (File.Exists(transactionFilePath))
            {
                try
                {
                    var json = File.ReadAllText(transactionFilePath);
                    var transactions = JsonSerializer.Deserialize<List<Transaction>>(json) ?? new List<Transaction>();

                    // Update all "Income" transactions with the new total income
                    var incomeTransactions = transactions.Where(t => t.Type == "Income").ToList();
                    foreach (var incomeTransaction in incomeTransactions)
                    {
                        // Update each income transaction with the new total income
                        Console.WriteLine($"Old Income Amount: {incomeTransaction.Amount}");
                        Console.WriteLine($"New Total Income: {newTotalIncome}");
                        incomeTransaction.Amount = newTotalIncome; // Update each income transaction

                        // You can adjust the logic if you have specific rules for updating multiple income entries
                    }

                    var updatedJson = JsonSerializer.Serialize(transactions, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(transactionFilePath, updatedJson);

                    Console.WriteLine("Updated transaction.json: " + updatedJson);
                }
                catch (JsonException ex)
                {
                    Console.WriteLine("Error updating transaction.json: " + ex.Message);
                }
            }
        }



        
        private void OpenModal() => isModalOpen = true;
        private void CloseModal() => isModalOpen = false;

        private void CalculateTotals()
        {
            // Only sum debts that have status "Overdue" or "Pending"
            totalDebt = _debtService.CalculateTotalDebt(debtList.Where(d => d.Status != "Paid").ToList());
        }


    }
}
