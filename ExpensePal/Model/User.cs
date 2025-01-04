using ExpensePal.Base;

namespace ExpensePal.Model
{
    public class User
    {
        public Guid UserId { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public Currency Currency { get; set; }
    }
}
