using ECharge.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECharge.Infrastructure.Services.DatabaseContext;

public class DataContext : DbContext
{
    //private readonly string _connectionString;

    //public DataContext(string connectionString)
    //{
    //    _connectionString = connectionString;
    //}

    public DataContext()
    {

    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        //optionsBuilder.UseSqlServer(_connectionString);
        // optionsBuilder.UseSqlServer("Server=localhost,1433;Database=EChargeDemo;Trusted_Connection=False;User=SA;Password=87arMWD5;TrustServerCertificate=True");
        optionsBuilder.UseSqlServer("Data Source=.\\DEV,8082;Initial Catalog=EChargeDemo;User ID=sa;Password=87ARmwd5;Pooling=True;TrustServerCertificate=True;Trusted_Connection=False");
    }

    public DbSet<LogEntry> LogEntries { get; set; }
    public DbSet<CableStateHook> CableStateHooks { get; set; }
    public DbSet<OrderStatusChangedHook> OrderStatusChangedHooks { get; set; }
    public DbSet<Session> Sessions { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<Notification> Notifications { get; set; }

}

