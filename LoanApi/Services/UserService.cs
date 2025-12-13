using LoanApi.Data;
using LoanApi.Interfaces;
using LoanApi.Models;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace LoanApi.Services;

public class UserService : IUserService
{
    private readonly LoanDbContext _db;
    private readonly IJwtService _jwt;

    public UserService(LoanDbContext db, IJwtService jwt)
    {
        _db = db;
        _jwt = jwt;
    }

    public async Task RegisterAsync(User user, string password)
    {
        try
        {
            Log.Information("Attempting to register user: {UserName}", user.UserName);
            
            var existingUser = await _db.Users.FirstOrDefaultAsync(x => x.UserName == user.UserName);
            if (existingUser != null)
            {
                Log.Warning("Registration failed: Username {UserName} already exists", user.UserName);
                throw new Exception("Username უკვე არსებობს");
            }

            var existingEmail = await _db.Users.FirstOrDefaultAsync(x => x.Email == user.Email);
            if (existingEmail != null)
            {
                Log.Warning("Registration failed: Email {Email} already exists", user.Email);
                throw new Exception("Email უკვე არსებობს");
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
            await _db.Users.AddAsync(user);
            await _db.SaveChangesAsync();

            Log.Information("User registered successfully: {UserName}, ID: {UserId}", user.UserName, user.Id);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error during user registration: {UserName}", user.UserName);
            throw;
        }
    }

    public async Task<string> LoginAsync(string username, string password)
    {
        try
        {
            Log.Information("Login attempt for user: {UserName}", username);

            var user = await _db.Users.FirstOrDefaultAsync(x => x.UserName == username);

            if (user == null)
            {
                Log.Warning("Login failed: User not found - {UserName}", username);
                throw new Exception("არასწორი username ან პაროლი");
            }

            // --- ცვლილება: აქედან ამოღებულია IsBlocked შემოწმება ---
            // დაბლოკილ იუზერსაც შეუძლია შესვლა
            // -----------------------------------------------------

            bool valid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);

            if (!valid)
            {
                Log.Warning("Login failed: Invalid password for user - {UserName}", username);
                throw new Exception("არასწორი username ან პაროლი");
            }

            Log.Information("User logged in successfully: {UserName}, Role: {Role}", username, user.Role);
            return _jwt.GenerateToken(user);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error during login for user: {UserName}", username);
            throw;
        }
    }

    public async Task<User> GetByIdAsync(int id)
    {
        try
        {
            Log.Information("Fetching user by ID: {UserId}", id);
            var user = await _db.Users.FindAsync(id);
            
            if (user == null)
            {
                Log.Warning("User not found: {UserId}", id);
            }
            else
            {
                Log.Information("User fetched successfully: {UserId}, {UserName}", id, user.UserName);
            }

            return user;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error fetching user by ID: {UserId}", id);
            throw;
        }
    }

    public async Task BlockUserAsync(int id, bool blockStatus)
    {
        try
        {
            Log.Information("Attempting to {Action} user: {UserId}", blockStatus ? "block" : "unblock", id);

            var user = await _db.Users.FindAsync(id);

            if (user == null)
            {
                Log.Warning("Block operation failed: User not found - {UserId}", id);
                throw new Exception("მომხმარებელი ვერ მოიძებნა");
            }

            user.IsBlocked = blockStatus;
            await _db.SaveChangesAsync();

            Log.Information("User {Action} successfully: {UserId}, {UserName}", 
                blockStatus ? "blocked" : "unblocked", id, user.UserName);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error during block operation for user: {UserId}", id);
            throw;
        }
    }
}