using DocAutomation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DocAutomation.Infrastructure.Persistence.Configurations;

public class DeploymentConfiguration : IEntityTypeConfiguration<Deployment>
{
    public void Configure(EntityTypeBuilder<Deployment> builder)
    {
        builder.ToTable("Deployments");

        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).HasDefaultValueSql("NEWSEQUENTIALID()");

        builder.Property(d => d.TemplateSlug).IsRequired().HasMaxLength(100);
        builder.Property(d => d.TemplateName).IsRequired().HasMaxLength(200);
        builder.Property(d => d.TemplateVersion).IsRequired();
        builder.Property(d => d.InputValuesJson).IsRequired().HasColumnType("nvarchar(max)");
        builder.Property(d => d.RenderedStepsJson).IsRequired().HasColumnType("nvarchar(max)");
        builder.Property(d => d.Status).IsRequired().HasConversion<string>().HasMaxLength(50);
        builder.Property(d => d.JiraTicketKey).HasMaxLength(50);
        builder.Property(d => d.JiraTicketUrl).HasMaxLength(500);
        builder.Property(d => d.StartedAt).IsRequired().HasDefaultValueSql("GETUTCDATE()");
        builder.Property(d => d.CompletedBy).IsRequired().HasMaxLength(100);
        builder.Property(d => d.Notes).HasMaxLength(2000);

        builder.HasIndex(d => d.TemplateId);
        builder.HasIndex(d => d.Status);

        builder
            .HasOne(d => d.Template)
            .WithMany()
            .HasForeignKey(d => d.TemplateId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasMany(d => d.Steps)
            .WithOne(s => s.Deployment)
            .HasForeignKey(s => s.DeploymentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
