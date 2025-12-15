using LoanApi.DTOs;
using LoanApi.Interfaces;
using LoanApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LoanApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(UserRegisterDto dto)
    {
        var user = new User
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            UserName = dto.UserName,
            Email = dto.Email,
            Age = dto.Age,
            MonthlyIncome = dto.MonthlyIncome
        };

        await _userService.RegisterAsync(user, dto.Password);
        return Ok(new { message = "User registered successfully" });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(UserLoginDto dto)
    {
        string token = await _userService.LoginAsync(dto.UserName, dto.Password);
        return Ok(new { token });
    }

    [Authorize]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(int id)
    {
        var user = await _userService.GetByIdAsync(id);
        if (user == null)
            return NotFound(new { message = "User not found" });

        var response = new UserResponseDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            UserName = user.UserName,
            Age = user.Age,
            Email = user.Email,
            MonthlyIncome = user.MonthlyIncome,
            IsBlocked = user.IsBlocked,
            IsAccountant = user.Role == Enums.UserRole.Accountant
        };

        return Ok(response);
    }

    
    [Authorize(Roles = "Accountant")]
    [HttpPatch("accountant/{id}/block")]
    public async Task<IActionResult> BlockUser(int id, [FromBody] BlockUserDto dto)
    {
        await _userService.BlockUserAsync(id, dto.IsBlocked);
        return Ok(new { message = $"User {(dto.IsBlocked ? "blocked" : "unblocked")} successfully" });
    }
}
