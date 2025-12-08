using Microsoft.EntityFrameworkCore;
using LoanApi.Models;

namespace LoanApi.Data;

public class LoanDbContext : DbContext
{
    public LoanDbContext(DbContextOptions<LoanDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Loan> Loans { get; set; }
}
