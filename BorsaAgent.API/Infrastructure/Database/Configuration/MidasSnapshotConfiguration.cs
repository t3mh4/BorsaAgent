using BorsaAgent.API.Infrastructure.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BorsaAgent.API.Infrastructure.Database.Configuration
{
    public class MidasSnapshotConfiguration : IEntityTypeConfiguration<MidasSnapshot>
    {
        public void Configure(EntityTypeBuilder<MidasSnapshot> builder)
        {
            builder.ToTable("midas_snapshot");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).HasColumnName("id");
            builder.Property(e => e.StockId).HasColumnName("stock_id");
            builder.Property(e => e.Date).HasColumnName("date");
            builder.Property(e => e.Open).HasColumnName("open").HasPrecision(18, 6);
            builder.Property(e => e.High).HasColumnName("high").HasPrecision(18, 6);
            builder.Property(e => e.Low).HasColumnName("low").HasPrecision(18, 6);
            builder.Property(e => e.Last).HasColumnName("last").HasPrecision(18, 6);
            builder.Property(e => e.Close).HasColumnName("close").HasPrecision(18, 6);
            builder.Property(e => e.PreviousClose).HasColumnName("previous_close").HasPrecision(18, 6);
            builder.Property(e => e.DailyChange).HasColumnName("daily_change").HasPrecision(18, 6);
            builder.Property(e => e.DailyChangePercent).HasColumnName("daily_change_percent").HasPrecision(10, 4);
            builder.Property(e => e.TotalVolume).HasColumnName("total_volume");
            builder.Property(e => e.TotalTurnover).HasColumnName("total_turnover").HasPrecision(18, 2);
            builder.Property(e => e.Ask).HasColumnName("ask").HasPrecision(18, 6);
            builder.Property(e => e.Bid).HasColumnName("bid").HasPrecision(18, 6);
            builder.Property(e => e.Vwap).HasColumnName("vwap").HasPrecision(18, 6);
            builder.Property(e => e.YearlyChange).HasColumnName("yearly_change").HasPrecision(18, 6);
            builder.Property(e => e.LowerLimit).HasColumnName("lower_limit").HasPrecision(18, 6);
            builder.Property(e => e.UpperLimit).HasColumnName("upper_limit").HasPrecision(18, 6);
            builder.Property(e => e.WeeklyChange).HasColumnName("weekly_change").HasPrecision(18, 6);
            builder.Property(e => e.WeeklyChangePercent).HasColumnName("weekly_change_percent").HasPrecision(10, 4);
            builder.Property(e => e.MonthlyChange).HasColumnName("monthly_change").HasPrecision(18, 6);
            builder.Property(e => e.MonthlyChangePercent).HasColumnName("monthly_change_percent").HasPrecision(10, 4);
            builder.Property(e => e.YearlyChangePercent).HasColumnName("yearly_change_percent").HasPrecision(10, 4);
            builder.Property(e => e.Volatility).HasColumnName("volatility").HasPrecision(10, 4);
            builder.Property(e => e.PriceEarning).HasColumnName("price_earning").HasPrecision(10, 4);
            builder.Property(e => e.PriceBookValue).HasColumnName("price_book_value").HasPrecision(10, 4);
            builder.Property(e => e.ReturnOnEquity).HasColumnName("return_on_equity").HasPrecision(10, 4);
            builder.Property(e => e.MarketValue).HasColumnName("market_value");
            builder.Property(e => e.CreatedAt).HasColumnName("created_at");

            builder.HasIndex(e => new { e.StockId, e.Date }).IsUnique();
            builder.HasOne(e => e.Stock)
                .WithMany(s => s.MidasSnapshots)
                .HasForeignKey(e => e.StockId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
