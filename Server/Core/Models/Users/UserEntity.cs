
public class UserEntity
{
    public Guid Id { get; set; }
    public string FullName {get;set;} = null!;
    public string PasswordHash {get;set;} = null!;
    public string Email {get;set;} = null!;
    public List<NotificationEntity> Notifications {get;set;} = new();
    public List<ProjectMemberEntity> ProjectMembers {get;set;} = new();
}