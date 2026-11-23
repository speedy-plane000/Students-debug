using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccess.Configurations;

public class AttachmentConfiguration : IEntityTypeConfiguration<AttachmentEntity>
{
    public void Configure(EntityTypeBuilder<AttachmentEntity> builder)
    {
        builder.HasKey(u => u.Id);

        builder.HasOne(t => t.UploadedBy)
            .WithMany()
            .HasForeignKey(t => t.UploadedById)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
