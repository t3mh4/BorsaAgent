using BorsaAgent.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BorsaAgent.API.Data.Configurations;

public class MlModelMetricConfiguration : IEntityTypeConfiguration<MlModelMetric>
{
    public void Configure(EntityTypeBuilder<MlModelMetric> entity)
    {
        entity.ToTable("MlModelMetrics");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.ModelPath).HasColumnType("text").IsRequired();
        entity.Property(x => x.Notes).HasColumnType("text");
        entity.Property(x => x.CreatedAtUtc).HasDefaultValueSql("CURRENT_TIMESTAMP");

        entity.HasIndex(x => x.CreatedAtUtc);
    }
}