using ExpensePal.Services;
using ExpensePal.Model;
using ExpensePal.Base; // namespace to access the Currency enum

namespace ExpensePal.Components.Pages
{
    public partial class Login
    {
        private User Users { get; set; } = new();

        private string ErrorMessage { get; set; } = string.Empty;

        private List<Currency> Currencies { get; set; } = Enum.GetValues(typeof(Currency)).Cast<Currency>().ToList();

        private void HandleLogin()
        {

            if (Users.Currency == 0) // 0 is a placeholder for "Select Currency"
            {
                ErrorMessage = "Please select a currency.";
                return;
            }

            if (UserService.Login(Users))
            {
                Nav.NavigateTo("/dashboard");
            }
            else
            {
                ErrorMessage = "Username or password is invalid.";
            }
        }
    }
}
