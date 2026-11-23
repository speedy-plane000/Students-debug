using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccess.Configurations;

public class HistoryEntryConfiguration : IEntityTypeConfiguration<HistoryEntryEntity>
{
    public void Configure(EntityTypeBuilder<HistoryEntryEntity> builder)
    {
        builder.HasKey(u => u.Id);

        builder.HasOne(t => t.Author)
            .WithMany()
            .HasForeignKey(t => t.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
