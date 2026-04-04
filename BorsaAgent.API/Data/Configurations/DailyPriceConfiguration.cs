using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BorsaAgent.API.Models;

namespace BorsaAgent.API.Data.Configurations;

public class DailyPriceConfiguration : IEntityTypeConfiguration<DailyPrice>
{
    public void Configure(EntityTypeBuilder<DailyPrice> entity)
    {
        entity.ToTable("DailyPrice");
        entity.HasKey(e => e.Id);

        entity.Property(e => e.TradeDate)
            .IsRequired();

        entity.Property(e => e.OpenPrice).HasPrecision(18, 4).IsRequired();
        entity.Property(e => e.HighPrice).HasPrecision(18, 4).IsRequired();
        entity.Property(e => e.LowPrice).HasPrecision(18, 4).IsRequired();
        entity.Property(e => e.ClosePrice).HasPrecision(18, 4).IsRequired();
        entity.Property(e => e.ChangePercent).HasPrecision(18, 4);

        entity.Property(e => e.Volume)
            .IsRequired();

        entity.Property(e => e.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        entity.HasOne(d => d.Stock)
            .WithMany(p => p.DailyPrices)
            .HasForeignKey(d => d.StockId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasIndex(e => new { e.StockId, e.TradeDate }).IsUnique();
    }
}