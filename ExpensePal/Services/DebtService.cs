using ExpensePal.Model;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace ExpensePal.Services
{
    public class DebtService
    {
        private static readonly string DebtFilePath = Path.Combine(FileSystem.AppDataDirectory, "debt.json");
        private static readonly string TransactionFilePath = Path.Combine(FileSystem.AppDataDirectory, "transaction.json");

        public List<Debt> LoadDebts()
        {
            if (File.Exists(DebtFilePath))
            {
                var json = File.ReadAllText(DebtFilePath);
                return JsonSerializer.Deserialize<List<Debt>>(json) ?? new List<Debt>();
            }

            return new List<Debt>();
        }

        public void SaveDebts(List<Debt> debts)
        {
            var json = JsonSerializer.Serialize(debts);
            File.WriteAllText(DebtFilePath, json);
        }

        public List<Debt> FilterDebts(List<Debt> debts, string filterTitle, string filterStatus, string sortOrder)
        {
            var filtered = debts.Where(d =>
                (string.IsNullOrWhiteSpace(filterTitle) || (d.Title != null && d.Title.Contains(filterTitle, StringComparison.OrdinalIgnoreCase))) &&
                (string.IsNullOrWhiteSpace(filterStatus) || (d.Status != null && d.Status.Contains(filterStatus, StringComparison.OrdinalIgnoreCase)))
            ).ToList();

            if (sortOrder == "Ascending")
            {
                return filtered.OrderBy(d => d.DueDate).ToList();
            }
            else if (sortOrder == "Descending")
            {
                return filtered.OrderByDescending(d => d.DueDate).ToList();
            }

            return filtered;
        }

        public decimal CalculateTotalDebt(List<Debt> debts)
        {
            return debts.Sum(d => d.Amount);
        }

        public decimal CalculateTotalIncome()
        {
            if (File.Exists(TransactionFilePath))
            {
                var json = File.ReadAllText(TransactionFilePath);
                var transactions = JsonSerializer.Deserialize<List<Transaction>>(json) ?? new List<Transaction>();

                return transactions.Where(t => t.Type == "Income").Sum(t => t.Amount);
            }

            return 0;
        }

        public decimal CalculateTotalOutflows()
        {
            if (File.Exists(TransactionFilePath))
            {
                var json = File.ReadAllText(TransactionFilePath);
                var transactions = JsonSerializer.Deserialize<List<Transaction>>(json) ?? new List<Transaction>();

                return transactions.Where(t => t.Type == "Expense").Sum(t => t.Amount);
            }

            return 0;
        }

        // Deduct debt from total income
        public void DeductDebtFromIncome(decimal debtAmount)
        {
            // Load existing transactions
            var transactions = LoadTransactions();

            // Find the income transaction (assuming the first income transaction represents the total income)
            var incomeTransaction = transactions.FirstOrDefault(t => t.Type == "Income");

            if (incomeTransaction != null)
            {
                // Deduct the debt amount from the income
                incomeTransaction.Amount -= debtAmount;

                // Save the updated transactions list back to the file
                SaveTransactions(transactions);
            }
        }

        // Load transactions from the file
        private List<Transaction> LoadTransactions()
        {
            if (File.Exists(TransactionFilePath))
            {
                var json = File.ReadAllText(TransactionFilePath);
                return JsonSerializer.Deserialize<List<Transaction>>(json) ?? new List<Transaction>();
            }

            return new List<Transaction>();
        }

        // Save transactions to the file
        private void SaveTransactions(List<Transaction> transactions)
        {
            var json = JsonSerializer.Serialize(transactions);
            File.WriteAllText(TransactionFilePath, json);
        }
    }
}
