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
        // optionsBuilder.UseSqlServer("Server=localhost,1433;Database=ECharge;Trusted_Connection=False;User=SA;Password=87arMWD5;TrustServerCertificate=True");
        //optionsBuilder.UseSqlServer("Server=localhost,1433; Database=ECharger;Trusted_Connection=False;User=sa;Password=M0yEjlpWMulVkvc;TrustServerCertificate=True");
        optionsBuilder.UseSqlServer("Server=localhost;User Id=SA;Password=Alialiyev123_;Database=ECharge;TrustServerCertificate=true;");
        
            
    }

    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<Session> Sessions { get; set; }
}

