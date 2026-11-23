using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccess.Configurations;

public class DeadlineChangeRequestConfiguration : IEntityTypeConfiguration<DeadlineChangeRequestEntity>
{
    public void Configure(EntityTypeBuilder<DeadlineChangeRequestEntity> builder)
    {
        builder.HasKey(u => u.Id);

        builder.HasOne(t => t.Author)
            .WithMany()
            .HasForeignKey(t => t.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.ResolvedBy)
            .WithMany()
            .HasForeignKey(t => t.ResolvedById)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
