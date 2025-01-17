using ExpensePal.Model;
using System.Text.Json;

namespace ExpensePal.Services
{
    public class DashboardService
    {
        // Paths for transaction and debt data files
        private static readonly string TransactionFilePath = Path.Combine(FileSystem.AppDataDirectory, "transaction.json");
        private static readonly string DebtFilePath = Path.Combine(FileSystem.AppDataDirectory, "debt.json");

        // Properties to hold loaded transactions and debts
        public List<Transaction> Transactions { get; private set; } = new List<Transaction>();
        public List<Debt> Debts { get; private set; } = new List<Debt>();


        // Loads transactions and debts asynchronously from their respective files.
        public async Task LoadTransactionsAndDebts()
        {
            Transactions = await LoadFromFileAsync<Transaction>(TransactionFilePath);
            Debts = await LoadFromFileAsync<Debt>(DebtFilePath);
        }

        private async Task<List<T>> LoadFromFileAsync<T>(string filePath)
        {
            if (!File.Exists(filePath)) return new List<T>();

            try
            {
                var json = await File.ReadAllTextAsync(filePath);
                return JsonSerializer.Deserialize<List<T>>(json) ?? new List<T>();
            }
            catch
            {
                return new List<T>();
            }
        }

        public decimal CalculateTotal(string type)
        {
            return Transactions.Where(t => t.Type == type).Sum(t => t.Amount);
        }

        public decimal CalculateTotalDebt(string status)
        {
            return Debts.Where(d => d.Status == status).Sum(d => d.Amount);
        }

        public List<Transaction> FilterTransactionsByDate(DateTime? startDate, DateTime? endDate)
        {
            return Transactions
                .Where(t => (!startDate.HasValue || t.Date >= startDate.Value) &&
                            (!endDate.HasValue || t.Date <= endDate.Value))
                .ToList();
        }

        public List<Debt> FilterDebtsByDate(DateTime? startDate, DateTime? endDate)
        {
            return Debts
                .Where(d => (!startDate.HasValue || d.DueDate >= startDate.Value) &&
                            (!endDate.HasValue || d.DueDate <= endDate.Value))
                .ToList();
        }

        public List<Transaction> GetTopTransactions(int count, bool ascending)
        {
            return ascending
                ? Transactions.OrderBy(t => t.Amount).Take(count).ToList()
                : Transactions.OrderByDescending(t => t.Amount).Take(count).ToList();
        }

        public List<Debt> GetTopDebts(int count, bool ascending)
        {
            return ascending
                ? Debts.OrderBy(d => d.Amount).Take(count).ToList()
                : Debts.OrderByDescending(d => d.Amount).Take(count).ToList();
        }
    }
}
