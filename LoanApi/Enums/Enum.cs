namespace LoanApi.Enums;

  
public enum LoanType
{
    FastLoan,
    AutoLoan,
    Installment
}

public enum LoanStatus
{
    Processing,
    Approved,
    Rejected
}
 
public enum UserRole
{
    User,
    Accountant
}