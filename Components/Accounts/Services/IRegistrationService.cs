using Users.Data;

namespace Accounts.Services
{
    public interface IRegistrationService
    {
        UserRecord CreateUserWithAccount(string name);
    }
}