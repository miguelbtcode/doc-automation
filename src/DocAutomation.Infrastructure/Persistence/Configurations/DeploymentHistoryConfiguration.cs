using DocAutomation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DocAutomation.Infrastructure.Persistence.Configurations;

public class DeploymentHistoryConfiguration : IEntityTypeConfiguration<DeploymentHistory>
{
    public void Configure(EntityTypeBuilder<DeploymentHistory> builder)
    {
        builder.ToTable("DeploymentHistory");

        builder.HasKey(h => h.Id);
        builder.Property(h => h.Id).HasDefaultValueSql("NEWSEQUENTIALID()");

        builder.Property(h => h.TemplateSlug).IsRequired().HasMaxLength(100);
        builder.Property(h => h.Outcome).IsRequired().HasMaxLength(50);
        builder.Property(h => h.LessonsLearned).HasMaxLength(4000);
        builder.Property(h => h.RecordedAt).IsRequired().HasDefaultValueSql("GETUTCDATE()");

        builder.HasIndex(h => h.TemplateSlug);
        builder.HasIndex(h => h.RecordedAt);

        builder
            .HasOne(h => h.Deployment)
            .WithMany()
            .HasForeignKey(h => h.DeploymentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
