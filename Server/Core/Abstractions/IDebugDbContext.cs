using Microsoft.EntityFrameworkCore;
public interface IDebugDbContext
{
    DbSet<UserEntity> Users {get;set;}
    DbSet<ProjectEntity> Projects {get;set;}
    DbSet<ProjectMemberEntity> ProjectMembers {get;set;}
    DbSet<ReleaseEntity> Releases {get;set;}
    DbSet<TicketEntity> Tickets {get;set;}
    DbSet<ReservationEntity> Reservations {get;set;}
    DbSet<FixEntity> Fixes {get;set;}
    DbSet<DeadlineChangeRequestEntity> DeadlineChangeRequests {get;set;}
    DbSet<CommentEntity> Comments {get;set;}
    DbSet<AttachmentEntity> Attachments {get;set;}
    DbSet<HistoryEntryEntity> HistoryEntries {get;set;}
    DbSet<NotificationEntity> Notifications {get;set;}
    Task<int> SaveChangesAsync(CancellationToken token = default);
}