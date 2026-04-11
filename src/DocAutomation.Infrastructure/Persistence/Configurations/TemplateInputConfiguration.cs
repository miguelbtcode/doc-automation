using DocAutomation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DocAutomation.Infrastructure.Persistence.Configurations;

public class TemplateInputConfiguration : IEntityTypeConfiguration<TemplateInput>
{
    public void Configure(EntityTypeBuilder<TemplateInput> builder)
    {
        builder.ToTable("TemplateInputs");

        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).HasDefaultValueSql("NEWSEQUENTIALID()");

        builder.Property(i => i.Key).IsRequired().HasMaxLength(100);
        builder.Property(i => i.Label).IsRequired().HasMaxLength(200);
        builder.Property(i => i.InputType).IsRequired().HasConversion<string>().HasMaxLength(50);
        builder.Property(i => i.IsRequired).IsRequired().HasDefaultValue(true);
        builder.Property(i => i.DefaultValue).HasMaxLength(500);
        builder.Property(i => i.Options).HasColumnType("nvarchar(max)");
        builder.Property(i => i.DisplayOrder).IsRequired().HasDefaultValue(0);
        builder.Property(i => i.HelpText).HasMaxLength(500);

        builder.HasIndex(i => new { i.TemplateId, i.Key }).IsUnique();
    }
}
