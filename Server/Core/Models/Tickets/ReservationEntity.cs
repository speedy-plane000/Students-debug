
public class ReservationEntity
{
    public Guid Id {get;set;}
    public Guid DeveloperId {get;set;} 
    public ProjectMemberEntity Developer {get;set;} = null!; // не нулл, т.к. либо сам рук либо раз может взять тикет

    public Guid? AssignedById {get;set;} 
    public ProjectMemberEntity? AssignedBy {get;set;} // может быть null => рук сам себя назначил

    public DateTime TakenAt {get;set;}
    public Guid TicketId {get;set;}
    public TicketEntity Ticket {get;set;} = null!;
}