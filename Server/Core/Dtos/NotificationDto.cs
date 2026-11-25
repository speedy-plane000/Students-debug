public record NotificationResponse(
    Guid Id,
    string Text,
    DateTime CreatedAt,
    bool IsRead
);
