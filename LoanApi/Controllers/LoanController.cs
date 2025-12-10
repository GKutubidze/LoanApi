using LoanApi.DTOs;
using LoanApi.Enums;
using LoanApi.Interfaces;
using LoanApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LoanApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LoanController : ControllerBase
{
    private readonly ILoanService _loanService;

    public LoanController(ILoanService loanService)
    {
        _loanService = loanService;
    }

    private int GetUserId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

    // ========================
    // USER Endpoints
    // ========================

    [Authorize(Roles = "User")]
    [HttpPost("user")]
    public async Task<IActionResult> CreateLoan(LoanCreateDto dto)
    {
        var loan = new Loan
        {
            Amount = dto.Amount,
            Currency = dto.Currency,
            LoanPeriod = dto.LoanPeriod,
            LoanType = dto.LoanType
        };

        var result = await _loanService.CreateLoanAsync(GetUserId(), loan);
        return Ok(result);
    }

    [Authorize(Roles = "User")]
    [HttpGet("user/my")]
    public async Task<IActionResult> GetMyLoans()
    {
        var loans = await _loanService.GetUserLoansAsync(GetUserId());
        return Ok(loans);
    }

    [Authorize(Roles = "User")]
    [HttpPut("user/{id}")]
    public async Task<IActionResult> UpdateLoan(int id, LoanUpdateDto dto)
    {
        var updated = new Loan
        {
            Amount = dto.Amount,
            Currency = dto.Currency,
            LoanPeriod = dto.LoanPeriod
        };

        var loan = await _loanService.UpdateLoanAsync(GetUserId(), id, updated);
        return Ok(loan);
    }

    [Authorize(Roles = "User")]
    [HttpDelete("user/{id}")]
    public async Task<IActionResult> DeleteLoan(int id)
    {
        await _loanService.DeleteLoanAsync(GetUserId(), id);
        return Ok(new { message = "Loan deleted" });
    }

    // ========================
    // ACCOUNTANT Endpoints
    // ========================

    [Authorize(Roles = "Accountant")]
    [HttpGet("accountant/all")]
    public async Task<IActionResult> GetAllLoans()
    {
        var loans = await _loanService.GetAllLoansAsync();
        return Ok(loans);
    }

    [Authorize(Roles = "Accountant")]
    [HttpPut("accountant/{id}")]
    public async Task<IActionResult> UpdateAnyLoan(int id, LoanUpdateDto dto)
    {
        var updated = new Loan
        {
            Amount = dto.Amount,
            Currency = dto.Currency,
            LoanPeriod = dto.LoanPeriod
        };

        var loan = await _loanService.UpdateLoanByAccountantAsync(id, updated);
        return Ok(loan);
    }

    [Authorize(Roles = "Accountant")]
    [HttpDelete("accountant/{id}")]
    public async Task<IActionResult> DeleteAnyLoan(int id)
    {
        await _loanService.DeleteLoanByAccountantAsync(id);
        return Ok(new { message = "Loan deleted by accountant" });
    }
}
