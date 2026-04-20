using BorsaAgent.API.Infrastructure.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BorsaAgent.API.Infrastructure.Database.Configuration
{
    public class StockDailyDataConfiguration : IEntityTypeConfiguration<StockDailyData>
    {
        public void Configure(EntityTypeBuilder<StockDailyData> builder)
        {
            builder.ToTable("stock_daily_data");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).HasColumnName("id");
            builder.Property(e => e.StockId).HasColumnName("stock_id");
            builder.Property(e => e.Date).HasColumnName("date");
            builder.Property(e => e.Open).HasColumnName("open").HasPrecision(18, 6);
            builder.Property(e => e.High).HasColumnName("high").HasPrecision(18, 6);
            builder.Property(e => e.Low).HasColumnName("low").HasPrecision(18, 6);
            builder.Property(e => e.Close).HasColumnName("close").HasPrecision(18, 6);
            builder.Property(e => e.Volume).HasColumnName("volume");
            builder.Property(e => e.CreatedAt).HasColumnName("created_at");

            builder.HasIndex(e => new { e.StockId, e.Date }).IsUnique();
            builder.HasOne(e => e.Stock)
                .WithMany(s => s.DailyData)
                .HasForeignKey(e => e.StockId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
