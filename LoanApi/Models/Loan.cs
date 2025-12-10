using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LoanApi.Enums;

namespace LoanApi.Models;

public class Loan
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    public int UserId { get; set; } // Foreign Key
    public User User { get; set; } // Navigation Property
    
    public LoanType LoanType { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; }
    public int LoanPeriod { get; set; }
    
    public LoanStatus Status { get; set; } = LoanStatus.Processing;
}