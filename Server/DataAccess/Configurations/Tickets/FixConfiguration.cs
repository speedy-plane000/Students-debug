using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccess.Configurations;

public class FixConfiguration : IEntityTypeConfiguration<FixEntity>
{
    public void Configure(EntityTypeBuilder<FixEntity> builder)
    {
        builder.HasKey(u => u.Id);
        
        builder.HasOne(t => t.Author)
            .WithMany()
            .HasForeignKey(t => t.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
