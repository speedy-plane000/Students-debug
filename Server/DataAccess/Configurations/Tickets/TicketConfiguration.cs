using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccess.Configurations;

public class TicketConfiguration : IEntityTypeConfiguration<TicketEntity>
{
    public void Configure(EntityTypeBuilder<TicketEntity> builder)
    {
        builder.HasKey(u => u.Id);
        builder.HasIndex(t => new { t.ProjectId, t.Number }).IsUnique();
        
        builder.HasOne(t => t.Author)
            .WithMany()
            .HasForeignKey(t => t.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.Verifier)
            .WithMany()
            .HasForeignKey(t => t.VerifierId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(t => t.Comments)
            .WithOne(c => c.Ticket)
            .HasForeignKey(c => c.TicketId);

        builder.HasMany(t => t.HistoryEntries)
            .WithOne(c => c.Ticket)
            .HasForeignKey(c => c.TicketId);

        builder.HasMany(t => t.Attachments)
            .WithOne(c => c.Ticket)
            .HasForeignKey(c => c.TicketId);

        builder.HasMany(t => t.Fixes)
            .WithOne(c => c.Ticket)
            .HasForeignKey(c => c.TicketId);

        builder.HasMany(t => t.DeadlineChangeRequests)
            .WithOne(c => c.Ticket)
            .HasForeignKey(c => c.TicketId);
    }
}
