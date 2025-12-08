using LoanApi.Models;

namespace LoanApi.Interfaces;

public interface IUserService
{
    Task RegisterAsync(User user, string password);
    Task<string> LoginAsync(string username, string password);
    Task<User> GetByIdAsync(int id);
    Task BlockUserAsync(int id, bool blockStatus);
}