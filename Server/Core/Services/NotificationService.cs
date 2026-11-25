using Microsoft.EntityFrameworkCore;

public class NotificationService : INotificationService
{
    private readonly IDebugDbContext context;

    public NotificationService(IDebugDbContext _context)
    {
        context = _context;
    }

    public async Task<Result<List<NotificationResponse>>> GetUserNotifications(Guid userId)
    {
        var notifications = await context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Select(n => new NotificationResponse(
                n.Id,
                n.Text,
                n.CreatedAt,
                n.isRead
            ))
            .ToListAsync();

        return new Result<List<NotificationResponse>>(notifications);
    }

    public async Task<Result<int>> MarkRead(Guid notificationId, Guid userId)
    {
        var notification = await context.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);
        if (notification == null)
        {
            return new Result<int>(ErrorType.NotFound, "Notification not found");
        }

        notification.isRead = true;
        await context.SaveChangesAsync();

        return new Result<int>(1);
    }

    public async Task<Result<int>> MarkAllRead(Guid userId)
    {
        var notifications = await context.Notifications
            .Where(n => n.UserId == userId && !n.isRead)
            .ToListAsync();

        foreach (var n in notifications)
        {
            n.isRead = true;
        }

        await context.SaveChangesAsync();

        return new Result<int>(1);
    }
}
