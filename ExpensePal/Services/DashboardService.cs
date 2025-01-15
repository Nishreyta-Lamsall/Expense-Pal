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
            try
            {
                if (File.Exists(TransactionFilePath))
                {
                    try
                    {
                        var json = await File.ReadAllTextAsync(TransactionFilePath);
                        Transactions = JsonSerializer.Deserialize<List<Transaction>>(json) ?? new List<Transaction>();
                    }
                    catch (JsonException jsonEx)
                    {
                        Console.WriteLine($"Error deserializing transactions: {jsonEx.Message}");
                        Transactions = new List<Transaction>();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error reading transactions file: {ex.Message}");
                        Transactions = new List<Transaction>();
                    }
                }

                if (File.Exists(DebtFilePath))
                {
                    try
                    {
                        var json = await File.ReadAllTextAsync(DebtFilePath);
                        Debts = JsonSerializer.Deserialize<List<Debt>>(json) ?? new List<Debt>();
                    }
                    catch (JsonException jsonEx)
                    {
                        Console.WriteLine($"Error deserializing debts: {jsonEx.Message}");
                        Debts = new List<Debt>();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error reading debts file: {ex.Message}");
                        Debts = new List<Debt>();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error loading transactions and debts: {ex.Message}");
            }
        }
    }
}