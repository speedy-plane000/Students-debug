using Core.Enums;

public class DeadlineChangeRequestEntity
{
    public Guid Id {get;set;}
    public DateTime NewDate {get;set;}
    public string Reason {get;set;} = null!;
    public DateTime? CounterDate {get;set;}
    public DateTime CreatedAt {get;set;}
    public DeadlineDecision Decision {get;set;}

    public Guid TicketId {get;set;}
    public TicketEntity Ticket {get;set;} = null!;

    public Guid AuthorId {get;set;}
    public ProjectMemberEntity Author {get;set;} = null!;

    public Guid? ResolvedById {get;set;} 
    public ProjectMemberEntity? ResolvedBy {get;set;}
}
