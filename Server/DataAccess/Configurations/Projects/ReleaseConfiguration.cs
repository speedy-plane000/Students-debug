using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccess.Configurations;

public class ReleaseConfiguration : IEntityTypeConfiguration<ReleaseEntity>
{
    public void Configure(EntityTypeBuilder<ReleaseEntity> builder)
    {
        builder.HasKey(u => u.Id);
        
        builder.HasMany(r => r.Tickets)
            .WithOne(t => t.Release)
            .HasForeignKey(t => t.ReleaseId);

        builder.HasIndex(r => new { r.ProjectId, r.Version }).IsUnique();
    }
}
