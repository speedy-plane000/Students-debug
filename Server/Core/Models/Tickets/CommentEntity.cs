
public class CommentEntity
{
    public Guid Id {get;set;}
    public string Text {get;set;} = null!;
    public DateTime CreatedAt {get;set;}
    public Guid TicketId {get;set;}
    public TicketEntity Ticket {get;set;} = null!;
    public Guid AuthorId {get;set;}
    public ProjectMemberEntity Author {get;set;} = null!;
}