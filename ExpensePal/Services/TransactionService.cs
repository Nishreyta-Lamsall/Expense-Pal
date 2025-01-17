using ExpensePal.Model;
using System.Text.Json;

namespace ExpensePal.Services
{
    public class TransactionService
    {
        // File paths for storing transaction and debt data
        private static readonly string TransactionFilePath = Path.Combine(FileSystem.AppDataDirectory, "transaction.json");
        private static readonly string DebtFilePath = Path.Combine(FileSystem.AppDataDirectory, "debt.json");

        // Load transactions from the JSON file asynchronously
        public async Task<List<Transaction>> LoadTransactionsAsync()
        {
            if (File.Exists(TransactionFilePath))
            {
                var json = await File.ReadAllTextAsync(TransactionFilePath);
                return string.IsNullOrWhiteSpace(json)
                    ? new List<Transaction>() // Return empty list if file is empty
                    : JsonSerializer.Deserialize<List<Transaction>>(json) ?? new List<Transaction>(); 
            }

            return new List<Transaction>(); // Return empty list if file doesn't exist
        }

        // Load debts from the JSON file asynchronously
        public async Task<List<Debt>> LoadDebtsAsync()
        {
            if (File.Exists(DebtFilePath))
            {
                var json = await File.ReadAllTextAsync(DebtFilePath);
                return string.IsNullOrWhiteSpace(json)
                    ? new List<Debt>() 
                    : JsonSerializer.Deserialize<List<Debt>>(json) ?? new List<Debt>(); 
            }

            return new List<Debt>(); 
        }

        // Add a new transaction to the list and save it to the JSON file
        public async Task<List<Transaction>> AddTransactionAsync(Transaction transaction, List<Transaction> transactions)
        {
            transactions.Add(transaction); 
            var json = JsonSerializer.Serialize(transactions); 
            await File.WriteAllTextAsync(TransactionFilePath, json); 
            return transactions; 
        }

        public List<Transaction> FilterTransactions(
            List<Transaction> transactions,
            string filterTitle,
            string filterTags,
            string filterType,
            DateTime? fromDate,
            DateTime? toDate,
            string sortOrder)
        {
            // Apply filters for title, tags, and type
            var filtered = transactions.Where(t =>
                (string.IsNullOrWhiteSpace(filterTitle) || t.Title?.Contains(filterTitle, StringComparison.OrdinalIgnoreCase) == true) &&
                (string.IsNullOrWhiteSpace(filterTags) || t.Tags?.Contains(filterTags, StringComparison.OrdinalIgnoreCase) == true) &&
                (string.IsNullOrEmpty(filterType) || t.Type == filterType)).ToList();

            // Filter by date range if provided
            if (fromDate.HasValue)
                filtered = filtered.Where(t => t.Date >= fromDate.Value).ToList();

            if (toDate.HasValue)
                filtered = filtered.Where(t => t.Date <= toDate.Value).ToList();

            // Sort the filtered transactions by date
            return sortOrder == "asc"
                ? filtered.OrderBy(t => t.Date).ToList()
                : filtered.OrderByDescending(t => t.Date).ToList();
        }

        // Calculate the total amount for transactions of a specific type
        public decimal CalculateTotal(List<Transaction> transactions, string type)
        {
            return transactions.Where(t => t.Type == type).Sum(t => t.Amount); 
        }

        // Calculate the total amount of all debts
        public decimal CalculateTotalDebt(List<Debt> debts)
        {
            return debts.Sum(d => d.Amount); 
        }
    }
}
