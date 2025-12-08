using LoanApi.Enums;

namespace LoanApi.DTOs;

public class LoanCreateDto
{
    public LoanType LoanType { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = null!;
    public int LoanPeriod { get; set; }
}