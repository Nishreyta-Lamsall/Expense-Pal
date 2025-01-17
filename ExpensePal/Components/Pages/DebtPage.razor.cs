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
        // Dependency injection 
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

        //Component is initialized which loads debts and updates totals.
        protected override void OnInitialized()
        {
            try
            {
                debtList = _debtService.LoadDebts();
                filteredDebts = debtList;
                UpdateTotals();
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
                UpdateTotals();
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
            UpdateTotals();
        }

        private async Task AddDebt()
        {
            try
            {
                debtList.Add(newDebt);
                _debtService.SaveDebts(debtList);
                newDebt = new Debt();
                FilterDebts();
                CloseModal();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding new debt: {ex.Message}");
            }
        }

        // Marks a debt as paid, if the user has enough available balance.
        private async Task MarkDebtAsPaid(Debt debt)
        {
            try
            {
                decimal availableBalance = CalculateAvailableBalance();

                if (availableBalance - debt.Amount < 0)
                {
                    await JS.InvokeVoidAsync("showAlert", "Insufficient balance to clear this debt.");
                    return;
                }

                debt.Status = "Paid";
                _debtService.SaveDebts(debtList);
                _debtService.DeductDebtFromIncome(debt.Amount);
                UpdateTotals();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error marking debt as paid: {ex.Message}");
            }
        }

        private void UpdateTotals()
        {
            totalDebt = _debtService.CalculateTotalDebt(debtList.Where(d => d.Status != "Paid").ToList());
        }

        private decimal CalculateAvailableBalance()
        {
            return _debtService.CalculateAvailableBalance(totalDebt);
        }

        // Opens the modal for adding a new debt.
        private void OpenModal() => isModalOpen = true;

        // Closes the modal.
        private void CloseModal() => isModalOpen = false;
    }
}
