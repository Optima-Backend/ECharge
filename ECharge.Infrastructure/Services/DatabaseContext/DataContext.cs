using ECharge.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECharge.Infrastructure.Services.DatabaseContext;

public class DataContext : DbContext
{
    private readonly string _connectionString;

    public DataContext(string connectionString)
    {
        _connectionString = connectionString;
    }

    //public DataContext()
    //{

    //}

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(_connectionString);
        //optionsBuilder.UseSqlServer("Server=localhost,1433;Database=ECharge;Trusted_Connection=False;User=SA;Password=87arMWD5;TrustServerCertificate=True");
    }

    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<Session> Sessions { get; set; }
    public DbSet<Order> Orders { get; set; }

}

