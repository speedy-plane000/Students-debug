public class ReleaseEntity
{
    public Guid Id {get;set;}
    public string Version {get;set;} = null!;
    public DateTime CreatedAt {get;set;} 
    public Guid ProjectId {get;set;}
    public ProjectEntity Project {get;set;} = null!;
    public List<TicketEntity> Tickets {get;set;} = new();
}