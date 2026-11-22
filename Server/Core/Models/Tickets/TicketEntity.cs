using Core.Enums;

public class TicketEntity
{
    public Guid Id {get;set;}
    public int Number {get;set;}
    public string Title {get;set;} = null!;
    public string Description {get;set;} = null!;
    public string Steps {get;set;} = null!;
    public string? ExpectedResult {get;set;}
    public string? ActualResult {get;set;}
    public Priority Priority {get;set;}
    public TicketStatus Status {get;set;}
    public DateTime PlannedStart {get;set;}
    public DateTime Deadline {get;set;}
    public DateTime CreatedAt {get;set;}

    public Guid ProjectId {get;set;}
    public ProjectEntity Project {get;set;} = null!;

    public Guid? ReleaseId {get;set;}
    public ReleaseEntity? Release {get;set;}

    // связи с тем кто создал тикет и кто проверяет(может быть нулл)
    public Guid AuthorId {get;set;}
    public ProjectMemberEntity Author {get;set;} = null!;

    public Guid? VerifierId {get;set;}
    public ProjectMemberEntity? Verifier {get;set;}

    //кто берет в работу 
    public Guid? ReservationId {get;set;}
    public ReservationEntity? Reservation {get;set;} 
    
    public List<CommentEntity> Comments {get;set;} = new();
    public List<HistoryEntryEntity> HistoryEntries {get;set;} = new(); // история изменений тикета и создания
    public List<AttachmentEntity> Attachments {get;set;} = new();
    public List<FixEntity> Fixes {get;set;} = new(); 
    public List<DeadlineChangeRequestEntity> DeadlineChangeRequests {get;set;} = new();
}
