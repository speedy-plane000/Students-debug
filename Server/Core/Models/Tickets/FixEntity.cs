using Core.Enums;

public class FixEntity
{
    public Guid Id {get;set;}
    public string Result {get;set;} = null!;
    public List<string> CommitLinks {get;set;} = new();
    public string ReleaseVersion {get;set;} = null!;
    public DateTime CreatedAt {get;set;}
    public string? CheckComment {get;set;}
    public FixDecision Decision {get;set;}

    public Guid TicketId {get;set;}
    public TicketEntity Ticket {get;set;} = null!;

    public Guid AuthorId {get;set;}
    public ProjectMemberEntity Author {get;set;} = null!;
}
