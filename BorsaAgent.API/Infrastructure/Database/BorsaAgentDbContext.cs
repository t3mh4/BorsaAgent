using BorsaAgent.API.Infrastructure.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace BorsaAgent.API.Infrastructure.Database;

public class BorsaAgentDbContext : DbContext
{
    public DbSet<Stock> Stocks { get; set; }
    public DbSet<StockDailyData> StockDailyData { get; set; }
    public DbSet<MidasSnapshot> MidasSnapshots { get; set; }

    public BorsaAgentDbContext(DbContextOptions<BorsaAgentDbContext> options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseNpgsql(DatabaseConfiguration.GetConnectionString());
        }
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BorsaAgentDbContext).Assembly);
    }
}