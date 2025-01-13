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

        public List<Debt> LoadDebts()
        {
            if (File.Exists(DebtFilePath))
            {
                var json = File.ReadAllText(DebtFilePath);

                if (!string.IsNullOrWhiteSpace(json))
                {
                    try
                    {
                        var debts = JsonSerializer.Deserialize<List<Debt>>(json);
                        return debts ?? new List<Debt>();
                    }
                    catch (JsonException)
                    {
                        return new List<Debt>();
                    }
                }
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
    }
}
