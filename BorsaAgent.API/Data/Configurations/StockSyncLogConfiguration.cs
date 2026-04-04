using BorsaAgent.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BorsaAgent.API.Data.Configurations;

public class StockSyncLogConfiguration : IEntityTypeConfiguration<StockSyncLog>
{
    public void Configure(EntityTypeBuilder<StockSyncLog> entity)
    {
        entity.ToTable("StockSyncLog");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.RequestUrl).HasColumnType("text").IsRequired();
        entity.Property(x => x.ErrorMessage).HasColumnType("text").IsRequired();
        entity.Property(x => x.Period1Utc).IsRequired();
        entity.Property(x => x.Period2Utc).IsRequired();
        entity.Property(x => x.CreatedAtUtc).HasDefaultValueSql("CURRENT_TIMESTAMP");

        entity.HasOne(x => x.Stock)
            .WithMany()
            .HasForeignKey(x => x.StockId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasIndex(x => x.StockId);
        entity.HasIndex(x => x.CreatedAtUtc);
    }
}