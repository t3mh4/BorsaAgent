using BorsaAgent.API.Infrastructure.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BorsaAgent.API.Infrastructure.Database.Configuration
{
    public class StockConfiguration : IEntityTypeConfiguration<Stock>
    {
        public void Configure(EntityTypeBuilder<Stock> builder)
        {
            builder.ToTable("stocks");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).HasColumnName("id");
            builder.Property(e => e.Symbol).HasColumnName("symbol").HasMaxLength(10).IsRequired();
            builder.Property(e => e.Name).HasColumnName("name").HasMaxLength(255).IsRequired();
            builder.Property(e => e.CreatedAt).HasColumnName("created_at");
            builder.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            builder.HasIndex(e => e.Symbol).IsUnique();
        }
    }
}
