using LoanApi.Enums;

namespace LoanApi.DTOs;

public class AccountantLoanUpdateDto
{
    public LoanType LoanType { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = null!;
    public int LoanPeriod { get; set; }
    public LoanStatus Status { get; set; }  // Accountant-ს შეუძლია status-ის შეცვლა
}