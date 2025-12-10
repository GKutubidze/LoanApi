using LoanApi.Enums;

namespace LoanApi.DTOs;

public class LoanResponseDto
{
    // ავტომატურად გენერირებული ველები
    public int Id { get; set; } // სესხის უნიკალური იდენტიფიკატორი
    public LoanStatus Status { get; set; } // თავიდან იქნება "Processing"
    public int UserId { get; set; } // მომხმარებლის ID, რომელმაც შექმნა სესხი

    // ველები, რომლებიც მოვიდა LoanCreateDto-დან
    public LoanType LoanType { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; }
    public int LoanPeriod { get; set; }
}