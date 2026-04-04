using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BorsaAgent.API.Models;

namespace BorsaAgent.API.Data.Configurations;

public class StockSummaryConfiguration : IEntityTypeConfiguration<StockSummary>
{
    public void Configure(EntityTypeBuilder<StockSummary> entity)
    {
        entity.ToTable("StockSummary");
        entity.HasKey(e => e.Id);

        entity.Property(e => e.SummaryDate)
            .IsRequired();

        entity.Property(e => e.SummaryText)
            .IsRequired()
            .HasColumnType("text");

        entity.Property(e => e.QdrantPointId)
            .HasMaxLength(50)
            .HasColumnType("varchar(50)");

        entity.Property(e => e.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        entity.HasOne(s => s.Stock)
            .WithMany(p => p.Summaries)
            .HasForeignKey(s => s.StockId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasIndex(e => new { e.StockId, e.SummaryDate }).IsUnique();
    }
}