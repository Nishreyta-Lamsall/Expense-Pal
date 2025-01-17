using ExpensePal.Model;
using ExpensePal.Services;
using Microsoft.AspNetCore.Components;

namespace ExpensePal.Components.Pages
{
    public partial class Dashboard
    {
        //Dependency Injection
        [Inject] private DashboardService DashboardService { get; set; }

        // Lists to store filtered transactions and debts for display.
        private List<Transaction> FilteredTransactions = new List<Transaction>();
        private List<Debt> FilteredDebts = new List<Debt>();

        // Variables to store key financial metrics like total income, expenses, debts, and paid debts.
        private decimal totalIncome;
        private decimal totalExpense;
        private decimal totalDebt;
        private decimal paidDebt;

        // Properties to expose the calculated financial metrics for binding in the UI.
        public decimal TotalIncome => totalIncome;
        public decimal TotalExpense => totalExpense;
        public decimal TotalDebt => totalDebt;
        public decimal PaidDebt => paidDebt;
        public decimal AvailableBalance => TotalIncome + TotalDebt - TotalExpense;

        // Nullable DateTime properties for filtering transactions and debts by date range.
        private DateTime? StartDateTransaction { get; set; }
        private DateTime? EndDateTransaction { get; set; }
        private DateTime? StartDateDebt { get; set; }
        private DateTime? EndDateDebt { get; set; }

        // Lifecycle method to initialize the component and load financial data.
        protected override async Task OnInitializedAsync()
        {
            await DashboardService.LoadTransactionsAndDebts();
            totalIncome = DashboardService.CalculateTotal("Income");
            totalExpense = DashboardService.CalculateTotal("Expense");
            totalDebt = DashboardService.CalculateTotalDebt("Pending");
            paidDebt = DashboardService.CalculateTotalDebt("Paid");
            FilteredTransactions = DashboardService.Transactions;
            FilteredDebts = DashboardService.Debts;
        }

        private void FilterTransactionsByDate()
        {
            FilteredTransactions = DashboardService.FilterTransactionsByDate(StartDateTransaction, EndDateTransaction);
        }

        private void FilterDebtsByDate()
        {
            FilteredDebts = DashboardService.FilterDebtsByDate(StartDateDebt, EndDateDebt);
        }

        private void ShowHighestTransactions()
        {
            FilteredTransactions = DashboardService.GetTopTransactions(5, ascending: false);
        }

        private void ShowLowestTransactions()
        {
            FilteredTransactions = DashboardService.GetTopTransactions(5, ascending: true);
        }

        private void ShowHighestDebts()
        {
            FilteredDebts = DashboardService.GetTopDebts(5, ascending: false);
        }

        private void ShowLowestDebts()
        {
            FilteredDebts = DashboardService.GetTopDebts(5, ascending: true);
        }

        private void ShowAllDebts()
        {
            FilteredDebts = DashboardService.Debts;
        }

        private void ShowOverdue()
        {
            FilteredDebts = DashboardService.Debts.Where(d => d.Status == "Pending").ToList();
        }
    }
}
