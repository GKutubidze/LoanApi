using LoanApi.Data;
using LoanApi.Enums;
using LoanApi.Interfaces;
using LoanApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LoanApi.Services;

public class LoanService : ILoanService
{
    private readonly LoanDbContext _db;

    public LoanService(LoanDbContext db)
    {
        _db = db;
    }

    public async Task<Loan> CreateLoanAsync(int userId, Loan loan)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user == null) throw new Exception("User not found");
        if (user.IsBlocked) throw new Exception("User is blocked and cannot request loans");

        loan.UserId = userId;
        loan.Status = LoanStatus.Processing;

        _db.Loans.Add(loan);
        await _db.SaveChangesAsync();
        return loan;
    }

    public async Task<List<Loan>> GetUserLoansAsync(int userId)
    {
        return await _db.Loans
            .Where(x => x.UserId == userId)
            .ToListAsync();
    }

    public async Task<Loan> GetLoanByIdAsync(int id)
    {
        var loan = await _db.Loans.FindAsync(id);
        if (loan == null) throw new Exception("Loan not found");

        return loan;
    }

    public async Task<List<Loan>> GetAllLoansAsync()
    {
        return await _db.Loans
            .Include(x => x.User)
            .ToListAsync();
    }

    public async Task<Loan> UpdateLoanAsync(int userId, int loanId, Loan updatedLoan)
    {
        var loan = await _db.Loans.FindAsync(loanId);
        if (loan == null) throw new Exception("Loan not found");

        if (loan.UserId != userId)
            throw new Exception("Access denied");

        if (loan.Status != LoanStatus.Processing)
            throw new Exception("Only loans in processing status can be updated");

        loan.Amount = updatedLoan.Amount;
        loan.Currency = updatedLoan.Currency;
        loan.LoanPeriod = updatedLoan.LoanPeriod;

        await _db.SaveChangesAsync();
        return loan;
    }

    public async Task DeleteLoanAsync(int userId, int loanId)
    {
        var loan = await _db.Loans.FindAsync(loanId);
        if (loan == null) throw new Exception("Loan not found");

        if (loan.UserId != userId)
            throw new Exception("Access denied");

        if (loan.Status != LoanStatus.Processing)
            throw new Exception("Only processing loans can be deleted");

        _db.Loans.Remove(loan);
        await _db.SaveChangesAsync();
    }
    
    // Accountant: Delete any loan
    public async Task DeleteLoanByAccountantAsync(int loanId)
    {
        var loan = await _db.Loans.FindAsync(loanId);
        if (loan == null)
            throw new Exception("Loan not found");

        _db.Loans.Remove(loan);
        await _db.SaveChangesAsync();
    }
    
    
    // Accountant: Update any loan (any status)
    public async Task<Loan> UpdateLoanByAccountantAsync(int loanId, Loan updatedLoan)
    {
        var loan = await _db.Loans.FindAsync(loanId);
        if (loan == null)
            throw new Exception("Loan not found");

        // Accountant can update any loan regardless of status
        loan.Amount = updatedLoan.Amount;
        loan.Currency = updatedLoan.Currency;
        loan.LoanPeriod = updatedLoan.LoanPeriod;

        await _db.SaveChangesAsync();
        return loan;
    }


}
