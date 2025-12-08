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

    // USER: Create loan
    [Authorize(Roles = "User")]
    [HttpPost]
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

    // USER: Get own loans
    [Authorize(Roles = "User")]
    [HttpGet("my")]
    public async Task<IActionResult> GetMyLoans()
    {
        var loans = await _loanService.GetUserLoansAsync(GetUserId());
        return Ok(loans);
    }

    // Accountant: Get all loans
    [Authorize(Roles = "Accountant")]
    [HttpGet("all")]
    public async Task<IActionResult> GetAllLoans()
    {
        return Ok(await _loanService.GetAllLoansAsync());
    }

    // User update loan
    [Authorize(Roles = "User")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateLoan(int id, LoanUpdateDto dto)
    {
        var updated = new Loan
        {
            Amount = dto.Amount,
            Currency = dto.Currency,
            LoanPeriod = dto.LoanPeriod
        };

        return Ok(await _loanService.UpdateLoanAsync(GetUserId(), id, updated));
    }

    [Authorize(Roles = "User")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteLoan(int id)
    {
        await _loanService.DeleteLoanAsync(GetUserId(), id);
        return Ok("Loan deleted");
    }
}
