using ExpensePal.Model;
using System.Text.Json;

namespace ExpensePal.Services
{
    public class TransactionService
    {
        private readonly string _transactionFilePath;
        private readonly string _debtFilePath;

        public TransactionService(string transactionFilePath, string debtFilePath)
        {
            _transactionFilePath = transactionFilePath;
            _debtFilePath = debtFilePath;
        }

        public List<Transaction> LoadTransactions()
        {
            if (File.Exists(_transactionFilePath))
            {
                var json = File.ReadAllText(_transactionFilePath);
                return string.IsNullOrWhiteSpace(json)
                    ? new List<Transaction>()
                    : JsonSerializer.Deserialize<List<Transaction>>(json) ?? new List<Transaction>();
            }

            // Default data if no file exists
            return new List<Transaction>
            {
                new Transaction { Date = DateTime.Now.AddDays(-2), Title = "Salary", Description = "Monthly salary", Amount = 3000, Type = "Income", Tags = "work, salary" },
                new Transaction { Date = DateTime.Now.AddDays(-1), Title = "Grocery", Description = "Weekly groceries", Amount = 150, Type = "Expense", Tags = "food, grocery" },
                new Transaction { Date = DateTime.Now, Title = "Electric Bill", Description = "Monthly bill", Amount = 75, Type = "Expense", Tags = "utilities, electric" }
            };
        }

        public List<Debt> LoadDebts()
        {
            if (File.Exists(_debtFilePath))
            {
                var json = File.ReadAllText(_debtFilePath);
                return string.IsNullOrWhiteSpace(json)
                    ? new List<Debt>()
                    : JsonSerializer.Deserialize<List<Debt>>(json) ?? new List<Debt>();
            }

            return new List<Debt>();
        }

        public void SaveTransactions(List<Transaction> transactions)
        {
            var json = JsonSerializer.Serialize(transactions);
            File.WriteAllText(_transactionFilePath, json);
        }

        public decimal CalculateTotal(List<Transaction> transactions, string type)
        {
            return transactions.Where(t => t.Type == type).Sum(t => t.Amount);
        }

        public decimal CalculateTotalDebt(List<Debt> debts)
        {
            return debts.Sum(d => d.Amount);
        }

        public List<Transaction> FilterTransactions(
            List<Transaction> transactions,
            string filterTitle,
            string filterTags,
            string filterType,
            string sortOrder)
        {
            var filtered = transactions.Where(t =>
            (string.IsNullOrWhiteSpace(filterTitle) || (t.Title != null && t.Title.Contains(filterTitle, StringComparison.OrdinalIgnoreCase))) &&
            (string.IsNullOrWhiteSpace(filterTags) || (t.Tags != null && t.Tags.Contains(filterTags, StringComparison.OrdinalIgnoreCase))) &&
            (string.IsNullOrEmpty(filterType) || t.Type == filterType)).ToList();


            return sortOrder == "asc" ? filtered.OrderBy(t => t.Date).ToList() : filtered.OrderByDescending(t => t.Date).ToList();
        }
    }
}
