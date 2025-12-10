using LoanApi.Data;
using LoanApi.Enums;
using LoanApi.Interfaces;
using LoanApi.Models;
using LoanApi.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using Serilog;
using Xunit;

namespace LoanApi.Tests.Services;

public class UserServiceTests
{
    public UserServiceTests()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo.Console()
            .CreateLogger();
    }

    private LoanDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<LoanDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new LoanDbContext(options);
    }

    [Fact]
    public async Task RegisterAsync_ValidUser_CreatesUser()
    {
        var context = GetInMemoryDbContext();
        var mockJwt = new Mock<IJwtService>();
        var service = new UserService(context, mockJwt.Object);

        var user = new User
        {
            FirstName = "Test",
            LastName = "User",
            UserName = "testuser",
            Email = "test@test.com",
            Age = 25,
            MonthlyIncome = 1000,
            Role = UserRole.User
        };

        await service.RegisterAsync(user, "password123");

        var savedUser = await context.Users.FirstOrDefaultAsync(u => u.UserName == "testuser");

        Assert.NotNull(savedUser);
        Assert.Equal("test@test.com", savedUser.Email);
        Assert.NotEmpty(savedUser.PasswordHash);
        Assert.NotEqual("password123", savedUser.PasswordHash);
    }

    [Fact]
    public async Task RegisterAsync_DuplicateUsername_ThrowsException()
    {
        var context = GetInMemoryDbContext();
        var mockJwt = new Mock<IJwtService>();
        var service = new UserService(context, mockJwt.Object);

        context.Users.Add(new User
        {
            FirstName = "Existing",
            LastName = "User",
            UserName = "duplicate",
            Email = "old@test.com",
            PasswordHash = "hash",
            Age = 25,
            MonthlyIncome = 1000
        });
        await context.SaveChangesAsync();

        var newUser = new User
        {
            FirstName = "New",
            LastName = "User",
            UserName = "duplicate",
            Email = "new@test.com",
            Age = 25,
            MonthlyIncome = 1000
        };

        var ex = await Assert.ThrowsAsync<Exception>(() => service.RegisterAsync(newUser, "pass"));
        Assert.Equal("Username უკვე არსებობს", ex.Message);
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsToken()
    {
        var context = GetInMemoryDbContext();
        var mockJwt = new Mock<IJwtService>();
        mockJwt.Setup(j => j.GenerateToken(It.IsAny<User>())).Returns("fake-jwt-token");

        var service = new UserService(context, mockJwt.Object);

        var password = "mySecretPassword";
        var hashed = BCrypt.Net.BCrypt.HashPassword(password);

        context.Users.Add(new User
        {
            UserName = "loginuser",
            Email = "login@test.com",
            PasswordHash = hashed,
            IsBlocked = false,
            Role = UserRole.User,
            FirstName = "Test",
            LastName = "User",
            Age = 25,
            MonthlyIncome = 1000
        });

        await context.SaveChangesAsync();

        var token = await service.LoginAsync("loginuser", password);

        Assert.Equal("fake-jwt-token", token);
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ThrowsException()
    {
        var context = GetInMemoryDbContext();
        var mockJwt = new Mock<IJwtService>();
        var service = new UserService(context, mockJwt.Object);

        context.Users.Add(new User
        {
            UserName = "wrongpassuser",
            Email = "wp@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("correct_pass"),
            IsBlocked = false,
            FirstName = "Test",
            LastName = "User",
            Age = 25,
            MonthlyIncome = 1000
        });

        await context.SaveChangesAsync();

        var ex = await Assert.ThrowsAsync<Exception>(() =>
            service.LoginAsync("wrongpassuser", "wrong_pass"));

        Assert.Equal("არასწორი username ან პაროლი", ex.Message);
    }

    [Fact]
    public async Task LoginAsync_BlockedUser_ThrowsException()
    {
        var context = GetInMemoryDbContext();
        var mockJwt = new Mock<IJwtService>();
        var service = new UserService(context, mockJwt.Object);

        context.Users.Add(new User
        {
            UserName = "blockeduser",
            Email = "blocked@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("pass"),
            IsBlocked = true,
            FirstName = "Test",
            LastName = "User",
            Age = 25,
            MonthlyIncome = 1000
        });

        await context.SaveChangesAsync();

        var ex = await Assert.ThrowsAsync<Exception>(() =>
            service.LoginAsync("blockeduser", "pass"));

        Assert.Equal("თქვენი ანგარიში დაბლოკილია", ex.Message);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingUser_ReturnsUser()
    {
        var context = GetInMemoryDbContext();
        var service = new UserService(context, new Mock<IJwtService>().Object);

        var user = new User
        {
            UserName = "findme",
            Email = "f@t.com",
            PasswordHash = "h",
            FirstName = "Test",
            LastName = "User",
            Age = 25,
            MonthlyIncome = 1000
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        // Detach to simulate fresh query
        context.Entry(user).State = EntityState.Detached;

        var result = await service.GetByIdAsync(user.Id);

        Assert.NotNull(result);
        Assert.Equal("findme", result.UserName);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingUser_ReturnsNull()
    {
        var context = GetInMemoryDbContext();
        var service = new UserService(context, new Mock<IJwtService>().Object);

        var result = await service.GetByIdAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task BlockUserAsync_UpdatesIsBlockedStatus()
    {
        var context = GetInMemoryDbContext();
        var service = new UserService(context, new Mock<IJwtService>().Object);

        var user = new User
        {
            UserName = "tobeBlocked",
            IsBlocked = false,
            PasswordHash = "h",
            FirstName = "Test",
            LastName = "User",
            Email = "test@test.com",
            Age = 25,
            MonthlyIncome = 1000
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        await service.BlockUserAsync(user.Id, true);

        var dbUser = await context.Users.FindAsync(user.Id);
        Assert.True(dbUser.IsBlocked);
    }

    [Fact]
    public async Task BlockUserAsync_UserNotFound_ThrowsException()
    {
        var context = GetInMemoryDbContext();
        var service = new UserService(context, new Mock<IJwtService>().Object);

        var ex = await Assert.ThrowsAsync<Exception>(() => service.BlockUserAsync(999, true));

        Assert.Equal("მომხმარებელი ვერ მოიძებნა", ex.Message);
    }
}