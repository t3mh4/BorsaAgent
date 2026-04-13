using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BorsaAgent.API.Models;

namespace BorsaAgent.API.Data.Configurations;

public class StockFeatureConfiguration : IEntityTypeConfiguration<StockFeature>
{
    public void Configure(EntityTypeBuilder<StockFeature> entity)
    {
        entity.ToView("v_stock_features");
        entity.HasNoKey();
        entity.Property(x => x.PriceToSMA5).HasColumnType("real");
        entity.Property(x => x.PriceToSMA20).HasColumnType("real");
        entity.Property(x => x.SMA5ToSMA20).HasColumnType("real");
        entity.Property(x => x.VolumeChange).HasColumnType("real");
        entity.Property(x => x.DailyReturn).HasColumnType("real");
        entity.Property(x => x.NextDayReturn).HasColumnType("real");
        entity.Property(x => x.ClosePrice).HasColumnType("real");
        entity.Property(x => x.Volume).HasColumnType("real");
        entity.Property(x => x.SMA5).HasColumnType("real");
        entity.Property(x => x.SMA20).HasColumnType("real");
    }
}