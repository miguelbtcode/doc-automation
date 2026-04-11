using DocAutomation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DocAutomation.Infrastructure.Persistence.Configurations;

public class TemplateConfiguration : IEntityTypeConfiguration<Template>
{
    public void Configure(EntityTypeBuilder<Template> builder)
    {
        builder.ToTable("Templates");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasDefaultValueSql("NEWSEQUENTIALID()");

        builder.Property(t => t.Slug).IsRequired().HasMaxLength(100);
        builder.HasIndex(t => t.Slug).IsUnique();

        builder.Property(t => t.Name).IsRequired().HasMaxLength(200);
        builder.Property(t => t.Description).HasMaxLength(1000);
        builder.Property(t => t.Version).IsRequired().HasDefaultValue(1);
        builder.Property(t => t.IsActive).IsRequired().HasDefaultValue(true);
        builder.Property(t => t.StepsJson).IsRequired().HasColumnType("nvarchar(max)");
        builder.Property(t => t.CreatedAt).IsRequired().HasDefaultValueSql("GETUTCDATE()");
        builder.Property(t => t.UpdatedAt).IsRequired().HasDefaultValueSql("GETUTCDATE()");
        builder.Property(t => t.CreatedBy).IsRequired().HasMaxLength(100);

        builder.HasQueryFilter(t => t.IsActive);

        builder
            .HasMany(t => t.Inputs)
            .WithOne(i => i.Template)
            .HasForeignKey(i => i.TemplateId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
