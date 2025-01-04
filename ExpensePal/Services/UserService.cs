using ExpensePal.Abstraction;
using ExpensePal.Model;
using ExpensePal.Services.Interface;

namespace ExpensePal.Services
{
    public class UserService : UserBase, IUser
    {
        private List<User> _users;

        public const string SeedUsername = "user";
        public const string SeedPassword = "pass";

        public UserService()
        {
            _users = LoadUsers();

            if (!_users.Any())
            {
                _users.Add(new User { UserName = SeedUsername, Password = SeedPassword });
                SaveUsers(_users);
            }

        }
        public bool Login(User user)
        {
            if (string.IsNullOrEmpty(user.UserName) || string.IsNullOrEmpty(user.Password))
            {
                return false; // Invalid input.
            }

            // Check if the username and password match any user in the list.
            return _users.Any(u => u.UserName == user.UserName && u.Password == user.Password);
        }
    }
}
