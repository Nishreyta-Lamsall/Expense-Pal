using ExpensePal.Model;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace ExpensePal.Services
{
    public class DebtService
    {
        // File paths for storing debts and transactions
        private static readonly string DebtFilePath = Path.Combine(FileSystem.AppDataDirectory, "debt.json");
        private static readonly string TransactionFilePath = Path.Combine(FileSystem.AppDataDirectory, "transaction.json");

        // Load debts from the JSON file
        public List<Debt> LoadDebts()
        {
            try
            {
                if (File.Exists(DebtFilePath))
                {
                    var json = File.ReadAllText(DebtFilePath);
                    return JsonSerializer.Deserialize<List<Debt>>(json) ?? new List<Debt>(); // Deserialize debts or return an empty list
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading debts: {ex.Message}"); 
            }

            return new List<Debt>(); 
        }

        // Save debts to the JSON file
        public void SaveDebts(List<Debt> debts)
        {
            try
            {
                var json = JsonSerializer.Serialize(debts); 
                File.WriteAllText(DebtFilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving debts: {ex.Message}"); 
            }
        }

        // Filter debts based on title, status, and sort order
        public List<Debt> FilterDebts(List<Debt> debts, string filterTitle, string filterStatus, string sortOrder)
        {
            var filtered = debts.Where(d =>
                (string.IsNullOrWhiteSpace(filterTitle) || d.Title?.Contains(filterTitle, StringComparison.OrdinalIgnoreCase) == true) &&
                (string.IsNullOrWhiteSpace(filterStatus) || d.Status?.Contains(filterStatus, StringComparison.OrdinalIgnoreCase) == true)
            ).ToList();

            // Sort debts based on the specified order
            return sortOrder switch
            {
                "Ascending" => filtered.OrderBy(d => d.DueDate).ToList(),
                "Descending" => filtered.OrderByDescending(d => d.DueDate).ToList(),
                _ => filtered // Return unsorted list if no valid sort order is provided
            };
        }

        // Calculate the total amount of all debts
        public decimal CalculateTotalDebt(List<Debt> debts)
        {
            return debts.Sum(d => d.Amount); 
        }

        // Calculate the available balance after deducting total debt and expenses
        public decimal CalculateAvailableBalance(decimal totalDebt)
        {
            decimal totalIncome = CalculateTotalIncome(); 
            decimal totalExpenses = CalculateTotalOutflows(); 
            return totalIncome - totalExpenses - totalDebt; 
        }

        public decimal CalculateTotalIncome()
        {
            return GetTransactions("Income").Sum(t => t.Amount); 
        }

        public decimal CalculateTotalOutflows()
        {
            return GetTransactions("Expense").Sum(t => t.Amount); 
        }

        // Deduct a specified debt amount from the first income transaction
        public void DeductDebtFromIncome(decimal debtAmount)
        {
            try
            {
                var transactions = LoadTransactions(); // Load all transactions
                var incomeTransaction = transactions.FirstOrDefault(t => t.Type == "Income"); // Find the first income transaction

                if (incomeTransaction != null)
                {
                    incomeTransaction.Amount -= debtAmount; 
                    SaveTransactions(transactions); // Save the updated transactions
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deducting debt from income: {ex.Message}"); 
            }
        }

        // Load transactions from the JSON file
        private List<Transaction> LoadTransactions()
        {
            try
            {
                if (File.Exists(TransactionFilePath))
                {
                    var json = File.ReadAllText(TransactionFilePath);
                    return JsonSerializer.Deserialize<List<Transaction>>(json) ?? new List<Transaction>(); 
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading transactions: {ex.Message}"); 
            }

            return new List<Transaction>(); 
        }

        // Save transactions to the JSON file
        private void SaveTransactions(List<Transaction> transactions)
        {
            try
            {
                var json = JsonSerializer.Serialize(transactions); 
                File.WriteAllText(TransactionFilePath, json); 
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving transactions: {ex.Message}"); 
            }
        }

        // Retrieve transactions of a specific type (e.g., Income or Expense)
        private List<Transaction> GetTransactions(string type)
        {
            return LoadTransactions().Where(t => t.Type == type).ToList(); 
        }
    }
}
