using LoanApi.Models;

namespace LoanApi.Interfaces;

public interface IJwtService
{
    string GenerateToken(User user);
}