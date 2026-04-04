using Microsoft.EntityFrameworkCore;
using BorsaAgent.API.Models;

namespace BorsaAgent.API.Data;

public class AppDbContext : DbContext
{
    public DbSet<Stock> Stocks { get; set; }
    public DbSet<DailyPrice> DailyPrices { get; set; }
    public DbSet<StockSummary> StockSummaries { get; set; }
    public DbSet<StockSyncLog> StockSyncLogs { get; set; }
    public DbSet<StockFeature> StockFeatures { get; set; }
    public DbSet<MlModelMetric> MlModelMetrics { get; set; }
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
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
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}