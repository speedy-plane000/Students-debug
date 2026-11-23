
using Core.Enums;

public class ProjectMemberEntity
{
    public Guid Id {get;set;}
    public bool IsOwner {get;set;}
    public string InviteCode {get;set;} = null!;
    public DateTime InvitedAt {get;set;} 
    public MemberStatus Status {get;set;}
    public ProjectRole? Role {get;set;}
    public Guid ProjectId { get; set; }
    public ProjectEntity Project { get; set; } = null!;
    public Guid UserId { get; set; }
    public UserEntity User { get; set; } = null!;
}