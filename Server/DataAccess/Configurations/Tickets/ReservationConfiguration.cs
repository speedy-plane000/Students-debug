using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccess.Configurations;

public class ReservationConfiguration : IEntityTypeConfiguration<ReservationEntity>
{
    public void Configure(EntityTypeBuilder<ReservationEntity> builder)
    {
        builder.HasKey(u => u.Id);

        builder.HasOne(t => t.Developer)
            .WithMany()
            .HasForeignKey(t => t.DeveloperId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.AssignedBy)
            .WithMany()
            .HasForeignKey(t => t.AssignedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.Ticket)
            .WithOne(r => r.Reservation)
            .HasForeignKey<ReservationEntity>(t => t.TicketId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
