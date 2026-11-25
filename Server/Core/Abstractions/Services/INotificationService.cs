public interface INotificationService
{
    Task<Result<List<NotificationResponse>>> GetUserNotifications(Guid userId);
    Task<Result<int>> MarkRead(Guid notificationId, Guid userId);
    Task<Result<int>> MarkAllRead(Guid userId);
}
