namespace LoanApi.DTOs;

public class UserRegisterDto
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string UserName { get; set; } = null!;
    public int Age { get; set; }
    public string Email { get; set; } = null!;
    public decimal MonthlyIncome { get; set; }
    public string Password { get; set; } = null!;

    // ეს ველი მხოლოდ Accountants შექმნისთვის გჭირდება
    public bool IsAccountant { get; set; } = false;
}