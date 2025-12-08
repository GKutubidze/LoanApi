using LoanApi.Data;
using LoanApi.Enums;
using LoanApi.Interfaces;
using LoanApi.Models;
using LoanApi.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using Serilog; // დაგჭირდება Serilog-ის ბიბლიოთეკა ტესტებშიც
using Xunit;

namespace LoanApi.Tests.Services;

public class UserServiceTests
{
    // კონსტრუქტორი სერილოგის "ჩუმი" ლოგერის შესაქმნელად
    public UserServiceTests()
    {
        // ეს აუცილებელია, რადგან შენ იყენებ static Log კლასს სერვისში.
        // თუ არ გავწერთ, ტესტებმა შეიძლება error დაარტყან.
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo.Console() // ან .WriteTo.Sink(new XunitSink()) თუ გინდა ტესტის აუთფუთში გამოჩნდეს
            .CreateLogger();
    }

    private LoanDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<LoanDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // უნიკალური სახელი ყოველი ტესტისთვის
            .Options;

        return new LoanDbContext(options);
    }

    [Fact]
    public async Task RegisterAsync_ValidUser_CreatesUser()
    {
        // Arrange
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

        // Act
        await service.RegisterAsync(user, "password123");

        // Assert
        var savedUser = await context.Users.FirstOrDefaultAsync(u => u.UserName == "testuser");
        Assert.NotNull(savedUser);
        Assert.Equal("test@test.com", savedUser.Email);
        Assert.NotEmpty(savedUser.PasswordHash);
        // ვამოწმებთ რომ პაროლი ნამდვილად ჰეშირებულია
        Assert.NotEqual("password123", savedUser.PasswordHash); 
    }

    [Fact]
    public async Task RegisterAsync_DuplicateUsername_ThrowsException()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var mockJwt = new Mock<IJwtService>();
        var service = new UserService(context, mockJwt.Object);

        // ვამატებთ უკვე არსებულ იუზერს
        context.Users.Add(new User
        {
            FirstName = "Existing",
            LastName = "User",
            UserName = "duplicate",
            Email = "old@test.com",
            PasswordHash = "hash"
        });
        await context.SaveChangesAsync();

        var newUser = new User
        {
            FirstName = "New",
            LastName = "User",
            UserName = "duplicate", // იგივე username
            Email = "new@test.com",
            PasswordHash = ""
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => service.RegisterAsync(newUser, "pass"));
        Assert.Equal("Username უკვე არსებობს", exception.Message);
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsToken()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var mockJwt = new Mock<IJwtService>();
        // ვეუბნებით Mock-ს, რომ როცა ტოკენს მოთხოვენ, დააბრუნოს "fake-token"
        mockJwt.Setup(j => j.GenerateToken(It.IsAny<User>())).Returns("fake-jwt-token");

        var service = new UserService(context, mockJwt.Object);

        var password = "mySecretPassword";
        // ბაზაში ვინახავთ დაჰეშილ პაროლს, ზუსტად ისე როგორც რეალობაში ხდება
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

        var user = new User
        {
            UserName = "loginuser",
            Email = "login@test.com",
            PasswordHash = hashedPassword,
            IsBlocked = false,
            Role = UserRole.User
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        // Act
        var token = await service.LoginAsync("loginuser", password);

        // Assert
        Assert.Equal("fake-jwt-token", token);
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ThrowsException()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var mockJwt = new Mock<IJwtService>();
        var service = new UserService(context, mockJwt.Object);

        var user = new User
        {
            UserName = "wrongpassuser",
            Email = "wp@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("correct_pass"),
            IsBlocked = false
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(() => service.LoginAsync("wrongpassuser", "wrong_pass"));
        Assert.Equal("არასწორი username ან პაროლი", ex.Message);
    }

    [Fact]
    public async Task LoginAsync_BlockedUser_ThrowsException()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var mockJwt = new Mock<IJwtService>();
        var service = new UserService(context, mockJwt.Object);

        var user = new User
        {
            UserName = "blockeduser",
            Email = "blocked@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("pass"),
            IsBlocked = true // დაბლოკილია
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(() => service.LoginAsync("blockeduser", "pass"));
        Assert.Equal("თქვენი ანგარიში დაბლოკილია", ex.Message);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingUser_ReturnsUser()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var mockJwt = new Mock<IJwtService>();
        var service = new UserService(context, mockJwt.Object);

        var user = new User { UserName = "findme", Email = "f@t.com", PasswordHash = "h" };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetByIdAsync(user.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("findme", result.UserName);
    }

    [Fact]
    public async Task BlockUserAsync_UpdatesIsBlockedStatus()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var mockJwt = new Mock<IJwtService>();
        var service = new UserService(context, mockJwt.Object);

        var user = new User { UserName = "tobeBlocked", IsBlocked = false, PasswordHash = "h" };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        // Act
        await service.BlockUserAsync(user.Id, true);

        // Assert
        var dbUser = await context.Users.FindAsync(user.Id);
        Assert.True(dbUser.IsBlocked);
    }
    
    [Fact]
    public async Task BlockUserAsync_UserNotFound_ThrowsException()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var mockJwt = new Mock<IJwtService>();
        var service = new UserService(context, mockJwt.Object);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(() => service.BlockUserAsync(999, true));
        Assert.Equal("მომხმარებელი ვერ მოიძებნა", ex.Message);
    }
}