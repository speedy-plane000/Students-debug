
using Core.Enums;

public class AttachmentEntity
{
    public Guid Id {get;set;}
    public string FilePath {get;set;} = null!;
    public long Size {get;set;}
    public AttachmentType Type {get;set;}
    public Guid TicketId {get;set;}
    public TicketEntity Ticket {get;set;} = null!;
    
    public Guid UploadedById {get;set;}
    public ProjectMemberEntity UploadedBy {get;set;} = null!;
}