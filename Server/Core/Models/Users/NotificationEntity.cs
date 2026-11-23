
public class NotificationEntity
{
    public Guid Id {get;set;}
    public string Text {get;set;} = null!;
    public DateTime CreatedAt {get;set;}
    public bool isRead {get;set;}
    public Guid UserId {get;set;}
    public UserEntity User {get;set;} = null!;
}