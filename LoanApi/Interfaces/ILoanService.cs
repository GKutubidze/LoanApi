using LoanApi.Models;

namespace LoanApi.Interfaces;

public interface ILoanService
{
    Task<Loan> CreateLoanAsync(int userId, Loan loan);
    Task<List<Loan>> GetUserLoansAsync(int userId);
    Task<Loan> UpdateLoanAsync(int userId, int loanId, Loan updatedLoan);
    Task DeleteLoanAsync(int userId, int loanId);
    Task<List<Loan>> GetAllLoansAsync(); // Accountant
    Task<Loan> GetLoanByIdAsync(int id);
}