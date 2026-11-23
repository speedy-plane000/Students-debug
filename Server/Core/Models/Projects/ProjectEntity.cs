
public class ProjectEntity
{
    public Guid Id {get;set;}
    public string Code{get;set;} = null!;
    public string Name{get;set;} = null!;
    public string RepositoryUrl {get;set;} = null!;
    public string? Description{get;set;}
    public List<ProjectMemberEntity> ProjectMembers {get;set;} = new();
    public List<ReleaseEntity> Releases {get;set;} = new();
    public List<TicketEntity> Tickets{get;set;} = new();
}