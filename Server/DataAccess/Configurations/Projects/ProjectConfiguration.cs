using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccess.Configurations;

public class ProjectConfiguration : IEntityTypeConfiguration<ProjectEntity>
{
    public void Configure(EntityTypeBuilder<ProjectEntity> builder)
    {
        builder.HasKey(u => u.Id);
        builder.HasIndex(p => p.Code).IsUnique();
        
        builder.HasMany(p => p.ProjectMembers)
            .WithOne(pm => pm.Project)
            .HasForeignKey(pm => pm.ProjectId);

        builder.HasMany(p => p.Releases)
            .WithOne(pm => pm.Project)
            .HasForeignKey(pm => pm.ProjectId);

        builder.HasMany(p => p.Tickets)
            .WithOne(pm => pm.Project)
            .HasForeignKey(pm => pm.ProjectId);
    }
}
