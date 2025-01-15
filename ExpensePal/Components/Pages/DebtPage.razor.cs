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

        private async Task MarkDebtAsPaid(Debt debt)
        {
            // Ensure totals are up-to-date
            CalculateTotals();

            decimal availableBalance = CalculateAvailableBalance();
            if (availableBalance - debt.Amount < 0)
            {
                await JS.InvokeVoidAsync("showAlert", "Insufficient balance to clear this debt.");
                return;
            }

            // Mark the debt as paid and update the list
            debt.Status = "Paid";
            _debtService.SaveDebts(debtList);

            // Update total income by deducting the cleared debt and save updated income in the transaction file
            _debtService.DeductDebtFromIncome(debt.Amount);

            CalculateTotals();
        }

        private void CalculateTotals()
        {
            totalDebt = _debtService.CalculateTotalDebt(debtList.Where(d => d.Status != "Paid").ToList());
        }

        private decimal CalculateAvailableBalance()
        {
            decimal totalIncome = _debtService.CalculateTotalIncome();
            decimal totalExpenses = _debtService.CalculateTotalOutflows();
            return totalIncome + totalDebt - totalExpenses;
        }

        private void OpenModal() => isModalOpen = true;
        private void CloseModal() => isModalOpen = false;
    }
}
