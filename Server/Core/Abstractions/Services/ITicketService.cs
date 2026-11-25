using Core.Enums;

public interface ITicketService
{
    Task<Result<Guid>> Create(Guid userId, CreateTicketRequest request);
    Task<Result<int>> Edit(Guid ticketId, UpdateTicketRequest request, Guid userId);
    Task<Result<int>> Delete(Guid ticketId, Guid userId);
    Task<Result<int>> Assign(Guid ticketId, AssignTicketRequest request, Guid userId);
    Task<Result<int>> SendToPool(Guid ticketId, Guid userId);
    Task<Result<int>> Take(Guid ticketId, Guid userId);
    Task<Result<int>> StartWork(Guid ticketId, Guid userId);
    Task<Result<int>> Pause(Guid ticketId, Guid userId);
    Task<Result<int>> SubmitFix(Guid ticketId, SubmitFixRequest request, Guid userId);
    Task<Result<int>> TakeForCheck(Guid ticketId, Guid userId);
    Task<Result<int>> Accept(Guid ticketId, AcceptFixRequest request, Guid userId);
    Task<Result<int>> Return(Guid ticketId, ReturnFixRequest request, Guid userId);
    Task<Result<int>> RequestDeadline(Guid ticketId, RequestDeadlineRequest request, Guid userId);
    Task<Result<int>> ResolveDeadline(Guid ticketId, Guid requestId, ResolveDeadlineRequest request, Guid userId);
    Task<Result<CommentResponse>> AddComment(Guid ticketId, AddCommentRequest request, Guid userId);
    Task<Result<AttachmentResponse>> AddAttachment(Guid ticketId, string filePath, long size, AttachmentType type, Guid userId);
    Task<Result<TicketDetailsResponse>> GetById(Guid ticketId, Guid userId);
    Task<Result<List<TicketShortResponse>>> GetProjectTickets(Guid projectId, TicketStatus? status, Priority? priority, Guid? assigneeId, string? search, Guid userId);
    Task<Result<List<TicketShortResponse>>> GetMyTickets(Guid userId);
}
