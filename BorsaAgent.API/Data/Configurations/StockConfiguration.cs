using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BorsaAgent.API.Models;

namespace BorsaAgent.API.Data.Configurations;

public class StockConfiguration : IEntityTypeConfiguration<Stock>
{
    public void Configure(EntityTypeBuilder<Stock> entity)
    {
        entity.ToTable("Stock");
        entity.HasKey(e => e.Id);

        entity.Property(e => e.Code)
            .IsRequired()
            .HasMaxLength(20)
            .HasColumnType("varchar(20)");

        entity.Property(e => e.ShortCode)
            .IsRequired()
            .HasMaxLength(10)
            .HasColumnType("varchar(10)");

        entity.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(255)
            .HasColumnType("varchar(255)");

        entity.Property(e => e.IsActive)
            .HasDefaultValue(true);

        entity.Property(e => e.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        entity.Property(e => e.UpdatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        entity.HasIndex(e => e.Code).IsUnique();
        entity.HasIndex(e => e.ShortCode).IsUnique();
    }
}