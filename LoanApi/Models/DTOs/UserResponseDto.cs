namespace LoanApi.DTOs;

public class UserResponseDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string UserName { get; set; } = null!;
    public int Age { get; set; }
    public string Email { get; set; } = null!;
    public decimal MonthlyIncome { get; set; }
    public bool IsBlocked { get; set; }
    public bool IsAccountant { get; set; }
}