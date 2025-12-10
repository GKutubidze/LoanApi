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

    // დამხმარე მეთოდი იუზერის შესაქმნელად (Required Fields რომ შევსებული იყოს)
    private User CreateUser(int id, string username, bool isBlocked = false)
    {
        return new User
        {
            Id = id,
            UserName = username,
            IsBlocked = isBlocked,
            FirstName = "Test",
            LastName = "User",
            Email = $"{username}@test.com",
            PasswordHash = "dummyhash123",
            Age = 25,
            MonthlyIncome = 1000,
            Role = UserRole.User
        };
    }

    // დამხმარე მეთოდი სესხის შესაქმნელად (Currency და სხვა ველები რომ შევსებული იყოს)
    private Loan CreateLoan(int id, int userId, decimal amount, LoanStatus status = LoanStatus.Processing)
    {
        return new Loan
        {
            Id = id,
            UserId = userId,
            Amount = amount,
            Status = status,
            Currency = "GEL",
            LoanPeriod = 12
        };
    }

    [Fact]
    public async Task CreateLoanAsync_ValidUser_CreatesLoan()
    {
        var context = GetInMemoryDbContext();
        var service = new LoanService(context);

        var user = CreateUser(1, "test");
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

        var loan = new Loan { Amount = 1000, Currency = "GEL", LoanPeriod = 12 };

        var ex = await Assert.ThrowsAsync<Exception>(() =>
            service.CreateLoanAsync(99, loan));

        Assert.Equal("User not found", ex.Message);
    }

    [Fact]
    public async Task CreateLoanAsync_BlockedUser_ThrowsException()
    {
        var context = GetInMemoryDbContext();
        var service = new LoanService(context);

        var user = CreateUser(1, "blocked", isBlocked: true);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var loan = new Loan { Amount = 1000, Currency = "GEL", LoanPeriod = 12 };

        var ex = await Assert.ThrowsAsync<Exception>(() =>
            service.CreateLoanAsync(1, loan));

        Assert.Equal("User is blocked and cannot request loans", ex.Message);
    }

    [Fact]
    public async Task GetUserLoansAsync_ReturnsOnlyUserLoans()
    {
        var context = GetInMemoryDbContext();
        var service = new LoanService(context);

        context.Users.Add(CreateUser(1, "user1"));
        context.Users.Add(CreateUser(2, "user2"));
        
        context.Loans.AddRange(
            CreateLoan(1, 1, 100),
            CreateLoan(2, 1, 200),
            CreateLoan(3, 2, 300)
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

        context.Users.Add(CreateUser(1, "user1"));
        context.Loans.Add(CreateLoan(1, 1, 100));
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

        context.Users.Add(CreateUser(1, "user1"));
        context.Loans.Add(CreateLoan(1, 1, 100));
        await context.SaveChangesAsync();

        var updateLoan = new Loan { Currency = "GEL", LoanPeriod = 12 };

        var ex = await Assert.ThrowsAsync<Exception>(() =>
            service.UpdateLoanAsync(2, 1, updateLoan));

        Assert.Equal("Access denied", ex.Message);
    }

    [Fact]
    public async Task UpdateLoanAsync_WrongStatus_ThrowsException()
    {
        var context = GetInMemoryDbContext();
        var service = new LoanService(context);

        context.Users.Add(CreateUser(1, "user1"));
        context.Loans.Add(CreateLoan(1, 1, 100, LoanStatus.Approved)); // სტატუსი Approved
        await context.SaveChangesAsync();
        
        var updateLoan = new Loan { Currency = "GEL", LoanPeriod = 12 };

        var ex = await Assert.ThrowsAsync<Exception>(() =>
            service.UpdateLoanAsync(1, 1, updateLoan));

        Assert.Equal("Only loans in processing status can be updated", ex.Message);
    }

    [Fact]
    public async Task DeleteLoanAsync_ValidLoan_DeletesLoan()
    {
        var context = GetInMemoryDbContext();
        var service = new LoanService(context);

        context.Users.Add(CreateUser(1, "user1"));
        context.Loans.Add(CreateLoan(1, 1, 100));
        await context.SaveChangesAsync();

        await service.DeleteLoanAsync(1, 1);

        Assert.Null(await context.Loans.FindAsync(1));
    }

    [Fact]
    public async Task DeleteLoanAsync_WrongStatus_ThrowsException()
    {
        var context = GetInMemoryDbContext();
        var service = new LoanService(context);

        context.Users.Add(CreateUser(1, "user1"));
        // სტატუსს ვაყენებთ Rejected-ზე, რომ exception გამოიწვიოს
        context.Loans.Add(CreateLoan(1, 1, 100, LoanStatus.Rejected)); 
        await context.SaveChangesAsync();

        var ex = await Assert.ThrowsAsync<Exception>(() =>
            service.DeleteLoanAsync(1, 1));

        // შესწორებულია LoanService.cs-ის მიხედვით:
        Assert.Equal("Only processing loans can be deleted", ex.Message);
    }

    [Fact]
    public async Task GetAllLoansAsync_ReturnsAllLoans()
    {
        var context = GetInMemoryDbContext();
        var service = new LoanService(context);

        context.Users.Add(CreateUser(1, "user1"));

        context.Loans.AddRange(
            CreateLoan(1, 1, 100),
            CreateLoan(2, 1, 200)
        );

        await context.SaveChangesAsync();

        var result = await service.GetAllLoansAsync();

        Assert.Equal(2, result.Count);
    }
}