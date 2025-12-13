using LoanApi.Data;
using LoanApi.Enums;
using LoanApi.Interfaces;
using LoanApi.Models;
using Microsoft.EntityFrameworkCore;
using Serilog;

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
        try
        {
            Log.Information("Attempting to create loan for User ID: {UserId}", userId);

            var user = await _db.Users.FindAsync(userId);
            if (user == null)
            {
                Log.Warning("CreateLoan failed: User not found - {UserId}", userId);
                throw new Exception("User not found");
            }

            // --- ცვლილება: დაბლოკილი იუზერი სესხს ვერ იღებს ---
            if (user.IsBlocked)
            {
                Log.Warning("CreateLoan failed: User is blocked - {UserId}", userId);
                throw new Exception("User is blocked and cannot request loans");
            }
            // --------------------------------------------------

            loan.UserId = userId;
            loan.Status = LoanStatus.Processing;

            _db.Loans.Add(loan);
            await _db.SaveChangesAsync();

            Log.Information("Loan created successfully. Loan ID: {LoanId}, User ID: {UserId}", loan.Id, userId);
            return loan;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error creating loan for User ID: {UserId}", userId);
            throw;
        }
    }

    public async Task<List<Loan>> GetUserLoansAsync(int userId)
    {
        try
        {
            Log.Information("Fetching loans for User ID: {UserId}", userId);
            
            var loans = await _db.Loans
                .Where(x => x.UserId == userId)
                .ToListAsync();
            
            Log.Information("Fetched {Count} loans for User ID: {UserId}", loans.Count, userId);
            return loans;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error fetching loans for User ID: {UserId}", userId);
            throw;
        }
    }

    public async Task<Loan> GetLoanByIdAsync(int id)
    {
        try
        {
            Log.Information("Fetching loan by ID: {LoanId}", id);

            var loan = await _db.Loans.FindAsync(id);
            if (loan == null)
            {
                Log.Warning("Loan not found: {LoanId}", id);
                throw new Exception("Loan not found");
            }

            return loan;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error fetching loan by ID: {LoanId}", id);
            throw;
        }
    }

    public async Task<List<Loan>> GetAllLoansAsync()
    {
        try
        {
            Log.Information("Fetching all loans from database");

            var loans = await _db.Loans
                .Include(x => x.User)
                .ToListAsync();

            Log.Information("Fetched total {Count} loans", loans.Count);
            return loans;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error fetching all loans");
            throw;
        }
    }

    public async Task<Loan> UpdateLoanAsync(int userId, int loanId, Loan updatedLoan)
    {
        try
        {
            Log.Information("User {UserId} attempting to update Loan {LoanId}", userId, loanId);

            var loan = await _db.Loans.FindAsync(loanId);
            if (loan == null)
            {
                Log.Warning("Update failed: Loan not found - {LoanId}", loanId);
                throw new Exception("Loan not found");
            }

            if (loan.UserId != userId)
            {
                Log.Warning("Update failed: Access denied for User {UserId} on Loan {LoanId}", userId, loanId);
                throw new Exception("Access denied");
            }

            if (loan.Status != LoanStatus.Processing)
            {
                Log.Warning("Update failed: Loan status is {Status}, expected Processing - Loan {LoanId}", loan.Status, loanId);
                throw new Exception("Only loans in processing status can be updated");
            }

            loan.Amount = updatedLoan.Amount;
            loan.Currency = updatedLoan.Currency;
            loan.LoanPeriod = updatedLoan.LoanPeriod;

            await _db.SaveChangesAsync();
            
            Log.Information("Loan {LoanId} updated successfully by User {UserId}", loanId, userId);
            return loan;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error updating Loan {LoanId} for User {UserId}", loanId, userId);
            throw;
        }
    }

    public async Task DeleteLoanAsync(int userId, int loanId)
    {
        try
        {
            Log.Information("User {UserId} attempting to delete Loan {LoanId}", userId, loanId);

            var loan = await _db.Loans.FindAsync(loanId);
            if (loan == null)
            {
                Log.Warning("Delete failed: Loan not found - {LoanId}", loanId);
                throw new Exception("Loan not found");
            }

            if (loan.UserId != userId)
            {
                Log.Warning("Delete failed: Access denied for User {UserId} on Loan {LoanId}", userId, loanId);
                throw new Exception("Access denied");
            }

            if (loan.Status != LoanStatus.Processing)
            {
                Log.Warning("Delete failed: Loan status is {Status}, expected Processing - Loan {LoanId}", loan.Status, loanId);
                throw new Exception("Only processing loans can be deleted");
            }

            _db.Loans.Remove(loan);
            await _db.SaveChangesAsync();

            Log.Information("Loan {LoanId} deleted successfully by User {UserId}", loanId, userId);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error deleting Loan {LoanId} for User {UserId}", loanId, userId);
            throw;
        }
    }

    public async Task DeleteLoanByAccountantAsync(int loanId)
    {
        try
        {
            Log.Information("Accountant attempting to delete Loan {LoanId}", loanId);

            var loan = await _db.Loans.FindAsync(loanId);
            if (loan == null)
            {
                Log.Warning("Accountant delete failed: Loan not found - {LoanId}", loanId);
                throw new Exception("Loan not found");
            }

            _db.Loans.Remove(loan);
            await _db.SaveChangesAsync();

            Log.Information("Loan {LoanId} deleted successfully by Accountant", loanId);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error executing Accountant delete for Loan {LoanId}", loanId);
            throw;
        }
    }

    public async Task<Loan> UpdateLoanByAccountantAsync(int loanId, Loan updatedLoan)
    {
        try
        {
            Log.Information("Accountant attempting to update Loan {LoanId}", loanId);

            var loan = await _db.Loans.FindAsync(loanId);
            if (loan == null)
            {
                Log.Warning("Accountant update failed: Loan not found - {LoanId}", loanId);
                throw new Exception("Loan not found");
            }

            loan.Amount = updatedLoan.Amount;
            loan.Currency = updatedLoan.Currency;
            loan.LoanPeriod = updatedLoan.LoanPeriod;

            await _db.SaveChangesAsync();

            Log.Information("Loan {LoanId} updated successfully by Accountant", loanId);
            return loan;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error executing Accountant update for Loan {LoanId}", loanId);
            throw;
        }
    }
}