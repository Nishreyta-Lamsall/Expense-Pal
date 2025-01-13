using ExpensePal.Model;
using System.Text.Json;
using System.IO;

namespace ExpensePal.Components.Pages
{
    public class DashboardService
    {
        public List<Transaction> Transactions { get; private set; } = new List<Transaction>();
        public List<Debt> Debts { get; private set; } = new List<Debt>();

        private static readonly string TransactionFilePath = Path.Combine(FileSystem.AppDataDirectory, "transaction.json");
        private static readonly string DebtFilePath = Path.Combine(FileSystem.AppDataDirectory, "debt.json");

        public async Task LoadTransactionsAndDebts()
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
        }
    }
}
