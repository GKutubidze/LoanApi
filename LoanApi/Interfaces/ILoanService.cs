using LoanApi.Models;

namespace LoanApi.Interfaces;

public interface ILoanService
{
    // USER
    Task<Loan> CreateLoanAsync(int userId, Loan loan);
    Task<List<Loan>> GetUserLoansAsync(int userId);
    Task<Loan> UpdateLoanAsync(int userId, int loanId, Loan updatedLoan);
    Task DeleteLoanAsync(int userId, int loanId);

    // ACCOUNTANT
    Task<List<Loan>> GetAllLoansAsync();
    Task<Loan> GetLoanByIdAsync(int id);
    Task<Loan> UpdateLoanByAccountantAsync(int loanId, Loan updatedLoan);
    Task DeleteLoanByAccountantAsync(int loanId);
}