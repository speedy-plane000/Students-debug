
using Core.Enums;

public class ProjectMemberEntity
{
    public Guid Id {get;set;}
    public bool isOwner {get;set;}
    public string inviteCode {get;set;} = null!;
    public DateTime invitedAt {get;set;} 
    public MemberStatus Status {get;set;}
    public ProjectRole? Role {get;set;}
    public Guid ProjectId { get; set; }
    public ProjectEntity Project { get; set; } = null!;
    public Guid UserId { get; set; }
    public UserEntity User { get; set; } = null!;
}