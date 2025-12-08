using LoanApi.Data;
using LoanApi.Enums;
using LoanApi.Models;
using LoanApi.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace LoanApi.Tests.Services;

public class LoanServiceTests
{
    private LoanDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<LoanDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new LoanDbContext(options);
    }

    [Fact]
    public async Task CreateLoanAsync_ValidUser_CreatesLoan()
    {
        var context = GetInMemoryDbContext();
        var service = new LoanService(context);

        var user = new User { Id = 1, UserName = "test", IsBlocked = false };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var loan = new Loan { Amount = 1000, Currency = "GEL", LoanPeriod = 12 };

        var result = await service.CreateLoanAsync(1, loan);

        Assert.NotNull(result);
        Assert.Equal(LoanStatus.Processing, result.Status);
        Assert.Equal(1, result.UserId);
        
        var dbLoan = await context.Loans.FirstOrDefaultAsync();
        Assert.NotNull(dbLoan);
    }

    [Fact]
    public async Task CreateLoanAsync_UserNotFound_ThrowsException()
    {
        var context = GetInMemoryDbContext();
        var service = new LoanService(context);
        var loan = new Loan { Amount = 1000 };

        var exception = await Assert.ThrowsAsync<Exception>(() => service.CreateLoanAsync(99, loan));
        Assert.Equal("User not found", exception.Message);
    }

    [Fact]
    public async Task CreateLoanAsync_BlockedUser_ThrowsException()
    {
        var context = GetInMemoryDbContext();
        var service = new LoanService(context);

        var user = new User { Id = 1, UserName = "blocked", IsBlocked = true };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var loan = new Loan { Amount = 1000 };

        var exception = await Assert.ThrowsAsync<Exception>(() => service.CreateLoanAsync(1, loan));
        Assert.Equal("User is blocked and cannot request loans", exception.Message);
    }

    [Fact]
    public async Task GetUserLoansAsync_ReturnsOnlyUserLoans()
    {
        var context = GetInMemoryDbContext();
        var service = new LoanService(context);

        context.Loans.AddRange(
            new Loan { Id = 1, UserId = 1, Amount = 100 },
            new Loan { Id = 2, UserId = 1, Amount = 200 },
            new Loan { Id = 3, UserId = 2, Amount = 300 }
        );
        await context.SaveChangesAsync();

        var result = await service.GetUserLoansAsync(1);

        Assert.Equal(2, result.Count);
        Assert.All(result, l => Assert.Equal(1, l.UserId));
    }

    [Fact]
    public async Task UpdateLoanAsync_ValidLoan_UpdatesLoan()
    {
        var context = GetInMemoryDbContext();
        var service = new LoanService(context);

        var loan = new Loan { Id = 1, UserId = 1, Status = LoanStatus.Processing, Amount = 100 };
        context.Loans.Add(loan);
        await context.SaveChangesAsync();

        var updateDto = new Loan { Amount = 500, Currency = "USD", LoanPeriod = 24 };

        var result = await service.UpdateLoanAsync(1, 1, updateDto);

        Assert.Equal(500, result.Amount);
        Assert.Equal("USD", result.Currency);
        
        var dbLoan = await context.Loans.FindAsync(1);
        Assert.Equal(500, dbLoan.Amount);
    }

    [Fact]
    public async Task UpdateLoanAsync_WrongUser_ThrowsException()
    {
        var context = GetInMemoryDbContext();
        var service = new LoanService(context);

        var loan = new Loan { Id = 1, UserId = 1, Status = LoanStatus.Processing };
        context.Loans.Add(loan);
        await context.SaveChangesAsync();

        var exception = await Assert.ThrowsAsync<Exception>(() => service.UpdateLoanAsync(2, 1, new Loan()));
        Assert.Equal("Access denied", exception.Message);
    }

    [Fact]
    public async Task UpdateLoanAsync_WrongStatus_ThrowsException()
    {
        var context = GetInMemoryDbContext();
        var service = new LoanService(context);

        var loan = new Loan { Id = 1, UserId = 1, Status = LoanStatus.Approved };
        context.Loans.Add(loan);
        await context.SaveChangesAsync();

        var exception = await Assert.ThrowsAsync<Exception>(() => service.UpdateLoanAsync(1, 1, new Loan()));
        Assert.Equal("Only loans in processing status can be updated", exception.Message);
    }

    [Fact]
    public async Task DeleteLoanAsync_ValidLoan_DeletesLoan()
    {
        var context = GetInMemoryDbContext();
        var service = new LoanService(context);

        var loan = new Loan { Id = 1, UserId = 1, Status = LoanStatus.Processing };
        context.Loans.Add(loan);
        await context.SaveChangesAsync();

        await service.DeleteLoanAsync(1, 1);

        var dbLoan = await context.Loans.FindAsync(1);
        Assert.Null(dbLoan);
    }

    [Fact]
    public async Task DeleteLoanAsync_WrongStatus_ThrowsException()
    {
        var context = GetInMemoryDbContext();
        var service = new LoanService(context);

        var loan = new Loan { Id = 1, UserId = 1, Status = LoanStatus.Rejected };
        context.Loans.Add(loan);
        await context.SaveChangesAsync();

        var exception = await Assert.ThrowsAsync<Exception>(() => service.DeleteLoanAsync(1, 1));
        Assert.Equal("Only loans in processing status can be deleted", exception.Message);
    }
    
    [Fact]
    public async Task GetAllLoansAsync_ReturnsAllLoans()
    {
        var context = GetInMemoryDbContext();
        var service = new LoanService(context);

        var user = new User { Id = 1, UserName = "user1" };
        context.Users.Add(user);
        
        context.Loans.Add(new Loan { Id = 1, UserId = 1, Amount = 100 });
        context.Loans.Add(new Loan { Id = 2, UserId = 1, Amount = 200 });
        await context.SaveChangesAsync();

        var result = await service.GetAllLoansAsync();
        
        Assert.Equal(2, result.Count);
    }
}