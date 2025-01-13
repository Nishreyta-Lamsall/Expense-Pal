using ExpensePal.Model;
using ExpensePal.Services;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpensePal.Components.Pages
{
    public partial class DebtPage
    {
        private readonly DebtService _debtService;
        private List<Debt> debtList = new();
        private List<Debt> filteredDebts = new();
        private Debt newDebt = new();
        private string filterTitle;
        private string filterStatus;
        private string sortOrder = "Ascending";
        private bool isModalOpen = false;

        private decimal totalDebt;

        public decimal TotalDebt => totalDebt;

        public DebtPage()
        {
            _debtService = new DebtService();
        }

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

        private void MarkAsPaid(Debt debt)
        {
            // Update the debt status
            debt.Status = "Paid";

            // Recalculate total debt
            CalculateTotals();

            // Save the updated list of debts
            _debtService.SaveDebts(debtList);
        }


        private void OpenModal() => isModalOpen = true;
        private void CloseModal() => isModalOpen = false;

        private void CalculateTotals()
        {
            // Only sum debts that have status "Overdue" or "Pending"
            totalDebt = _debtService.CalculateTotalDebt(debtList.Where(d => d.Status != "Paid").ToList());
        }


    }
}
