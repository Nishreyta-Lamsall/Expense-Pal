using ExpensePal.Model;
using ExpensePal.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpensePal.Components.Pages
{
    public partial class DebtPage
    {
        // Inject DebtService
        [Inject] private DebtService _debtService { get; set; }

        private List<Debt> debtList = new();
        private List<Debt> filteredDebts = new();
        private Debt newDebt = new();
        private string filterTitle;
        private string filterStatus;
        private string sortOrder = "Ascending";
        private bool isModalOpen = false;

        private decimal totalDebt;
        public decimal TotalDebt => totalDebt;
        protected override void OnInitialized()
        {
            try
            {
                debtList = _debtService.LoadDebts();
                filteredDebts = debtList;
                CalculateTotals();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing DebtPage: {ex.Message}");
            }
        }

        private void FilterDebts()
        {
            try
            {
                filteredDebts = _debtService.FilterDebts(debtList, filterTitle, filterStatus, sortOrder);
                CalculateTotals();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error filtering debts: {ex.Message}");
            }
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
            try
            {
                debtList.Add(newDebt);
                newDebt = new Debt();
                FilterDebts();
                _debtService.SaveDebts(debtList);
                CloseModal();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding new debt: {ex.Message}");
            }
        }

        private async Task MarkDebtAsPaid(Debt debt)
        {
            try
            {
                CalculateTotals();
                decimal availableBalance = CalculateAvailableBalance();

                if (availableBalance - debt.Amount < 0)
                {
                    await JS.InvokeVoidAsync("showAlert", "Insufficient balance to clear this debt.");
                    return;
                }

                debt.Status = "Paid";
                _debtService.SaveDebts(debtList);
                _debtService.DeductDebtFromIncome(debt.Amount);
                CalculateTotals();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error marking debt as paid: {ex.Message}");
            }
        }

        private void CalculateTotals()
        {
            try
            {
                totalDebt = _debtService.CalculateTotalDebt(debtList.Where(d => d.Status != "Paid").ToList());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calculating totals: {ex.Message}");
            }
        }

        private decimal CalculateAvailableBalance()
        {
            try
            {
                decimal totalIncome = _debtService.CalculateTotalIncome();
                decimal totalExpenses = _debtService.CalculateTotalOutflows();
                return totalIncome + totalDebt - totalExpenses;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calculating available balance: {ex.Message}");
                return 0;
            }
        }

        private void OpenModal() => isModalOpen = true;
        private void CloseModal() => isModalOpen = false;
    }
}
