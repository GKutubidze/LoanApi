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
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
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
    }

    [Fact]
    public async Task CreateLoanAsync_UserNotFound_ThrowsException()
    {
        var context = GetInMemoryDbContext();
        var service = new LoanService(context);

        var loan = new Loan { Amount = 1000 };

        var ex = await Assert.ThrowsAsync<Exception>(() =>
            service.CreateLoanAsync(99, loan));

        Assert.Equal("User not found", ex.Message);
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

        var ex = await Assert.ThrowsAsync<Exception>(() =>
            service.CreateLoanAsync(1, loan));

        Assert.Equal("User is blocked and cannot request loans", ex.Message);
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

        var loan = new Loan
        {
            Id = 1,
            UserId = 1,
            Status = LoanStatus.Processing,
            Amount = 100
        };

        context.Loans.Add(loan);
        await context.SaveChangesAsync();

        var update = new Loan
        {
            Amount = 500,
            Currency = "USD",
            LoanPeriod = 24
        };

        var result = await service.UpdateLoanAsync(1, 1, update);

        Assert.Equal(500, result.Amount);
        Assert.Equal("USD", result.Currency);
    }

    [Fact]
    public async Task UpdateLoanAsync_WrongUser_ThrowsException()
    {
        var context = GetInMemoryDbContext();
        var service = new LoanService(context);

        context.Loans.Add(new Loan
        {
            Id = 1,
            UserId = 1,
            Status = LoanStatus.Processing
        });

        await context.SaveChangesAsync();

        var ex = await Assert.ThrowsAsync<Exception>(() =>
            service.UpdateLoanAsync(2, 1, new Loan()));

        Assert.Equal("Access denied", ex.Message);
    }

    [Fact]
    public async Task UpdateLoanAsync_WrongStatus_ThrowsException()
    {
        var context = GetInMemoryDbContext();
        var service = new LoanService(context);

        context.Loans.Add(new Loan
        {
            Id = 1,
            UserId = 1,
            Status = LoanStatus.Approved
        });

        await context.SaveChangesAsync();

        var ex = await Assert.ThrowsAsync<Exception>(() =>
            service.UpdateLoanAsync(1, 1, new Loan()));

        Assert.Equal("Only loans in processing status can be updated", ex.Message);
    }

    [Fact]
    public async Task DeleteLoanAsync_ValidLoan_DeletesLoan()
    {
        var context = GetInMemoryDbContext();
        var service = new LoanService(context);

        context.Loans.Add(new Loan
        {
            Id = 1,
            UserId = 1,
            Status = LoanStatus.Processing
        });

        await context.SaveChangesAsync();

        await service.DeleteLoanAsync(1, 1);

        Assert.Null(await context.Loans.FindAsync(1));
    }

    [Fact]
    public async Task DeleteLoanAsync_WrongStatus_ThrowsException()
    {
        var context = GetInMemoryDbContext();
        var service = new LoanService(context);

        context.Loans.Add(new Loan
        {
            Id = 1,
            UserId = 1,
            Status = LoanStatus.Rejected
        });

        await context.SaveChangesAsync();

        var ex = await Assert.ThrowsAsync<Exception>(() =>
            service.DeleteLoanAsync(1, 1));

        Assert.Equal("Only loans in processing status can be deleted", ex.Message);
    }

    [Fact]
    public async Task GetAllLoansAsync_ReturnsAllLoans()
    {
        var context = GetInMemoryDbContext();
        var service = new LoanService(context);

        context.Users.Add(new User { Id = 1, UserName = "user1" });

        context.Loans.AddRange(
            new Loan { Id = 1, UserId = 1, Amount = 100 },
            new Loan { Id = 2, UserId = 1, Amount = 200 }
        );

        await context.SaveChangesAsync();

        var result = await service.GetAllLoansAsync();

        Assert.Equal(2, result.Count);
    }
}
