using Microsoft.EntityFrameworkCore;

namespace DataAccess;

public class DebugDbContext(DbContextOptions<DebugDbContext> options) : DbContext(options),IDebugDbContext
{
    public DbSet<UserEntity> Users {get;set;}
    public DbSet<ProjectEntity> Projects {get;set;}
    public DbSet<ProjectMemberEntity> ProjectMembers {get;set;}
    public DbSet<ReleaseEntity> Releases {get;set;}
    public DbSet<TicketEntity> Tickets {get;set;}
    public DbSet<ReservationEntity> Reservations {get;set;}
    public DbSet<FixEntity> Fixes {get;set;}
    public DbSet<DeadlineChangeRequestEntity> DeadlineChangeRequests {get;set;}
    public DbSet<CommentEntity> Comments {get;set;}
    public DbSet<AttachmentEntity> Attachments {get;set;}
    public DbSet<HistoryEntryEntity> HistoryEntries {get;set;}
    public DbSet<NotificationEntity> Notifications {get;set;}

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DebugDbContext).Assembly);
    }
}
