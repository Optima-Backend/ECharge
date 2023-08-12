using ECharge.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECharge.Infrastructure.Services.DatabaseContext;

public class DataContext : DbContext
{

    public DataContext()
    {
            
    }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer("Server=localhost;User Id=SA;Password=Alialiyev123_;Database=ECharge;TrustServerCertificate=true;");
    }
    
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<Session> Sessions { get; set; }
}

