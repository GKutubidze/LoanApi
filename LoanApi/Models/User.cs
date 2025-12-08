using LoanApi.Enums;
using System.Collections.Generic;

namespace LoanApi.Models;

public class User
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string UserName { get; set; }
    public int Age { get; set; }
    public string Email { get; set; }
    public decimal MonthlyIncome { get; set; }
    public bool IsBlocked { get; set; } = false;
    public string PasswordHash { get; set; }
    
    public UserRole Role { get; set; } = UserRole.User; // როლი აქ არის!
    
    public List<Loan> Loans { get; set; } = new List<Loan>();
}