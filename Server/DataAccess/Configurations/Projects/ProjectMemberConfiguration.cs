using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccess.Configurations;

public class ProjectMemberConfiguration : IEntityTypeConfiguration<ProjectMemberEntity>
{
    public void Configure(EntityTypeBuilder<ProjectMemberEntity> builder)
    {
        builder.HasKey(u => u.Id);

        builder.HasIndex(pm => pm.inviteCode).IsUnique();
        builder.HasIndex(pm => new {pm.ProjectId,pm.UserId}).IsUnique();
    }
}
