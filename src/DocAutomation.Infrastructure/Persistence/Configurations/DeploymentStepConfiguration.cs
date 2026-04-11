using DocAutomation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DocAutomation.Infrastructure.Persistence.Configurations;

public class DeploymentStepConfiguration : IEntityTypeConfiguration<DeploymentStep>
{
    public void Configure(EntityTypeBuilder<DeploymentStep> builder)
    {
        builder.ToTable("DeploymentSteps");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasDefaultValueSql("NEWSEQUENTIALID()");

        builder.Property(s => s.StepPath).IsRequired().HasMaxLength(200);
        builder.Property(s => s.Title).IsRequired().HasMaxLength(500);
        builder.Property(s => s.StepType).IsRequired().HasConversion<string>().HasMaxLength(50);
        builder.Property(s => s.Section).IsRequired().HasMaxLength(50);
        builder.Property(s => s.Status).IsRequired().HasConversion<string>().HasMaxLength(50);
        builder.Property(s => s.Notes).HasMaxLength(1000);
        builder.Property(s => s.DisplayOrder).IsRequired();

        builder.HasIndex(s => s.DeploymentId);
        builder.HasIndex(s => new { s.DeploymentId, s.StepPath }).IsUnique();
    }
}
