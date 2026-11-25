using Core.Enums;

public record CreateTicketRequest
(
    string Title,
    string Description,
    string Steps,
    string? ExpectedResult,
    string? ActualResult,
    Priority Priority,
    DateTime PlannedStart,
    DateTime Deadline,
    Guid ProjectId
);

public record UpdateTicketRequest
(
    string? Title,
    string? Description,
    string? Steps,
    string? ExpectedResult,
    string? ActualResult,
    Priority? Priority,
    DateTime? PlannedStart,
    DateTime? Deadline,
    Guid? ProjectId
);

public record AssignTicketRequest
(
    Guid DeveloperId
);

public record SubmitFixRequest
(
    string Result,
    List<string> CommitLinks,
    string ReleaseVersion
);

public record AcceptFixRequest
(
    string? Comment
);

public record ReturnFixRequest
(
    string Comment
);

public record RequestDeadlineRequest
(
    DateTime NewDate,
    string Reason
);

public record ResolveDeadlineRequest
(
    DeadlineDecision Decision,
    DateTime? CounterDate
);

public record AddCommentRequest
(
    string Text
);

public record AddAttachmentRequest
(
    string FilePath,
    long Size,
    AttachmentType Type
);

public record TicketShortResponse
(
    Guid Id,
    int Number,
    string FormattedNumber,
    string Title,
    TicketStatus Status,
    Priority Priority,
    DateTime PlannedStart,
    DateTime Deadline,
    DateTime CreatedAt,
    Guid ProjectId,
    string ProjectCode,
    Guid? AssigneeId,
    string? AssigneeName,
    Guid AuthorId,
    string AuthorName,
    Guid? VerifierId,
    string? VerifierName,
    int CommentsCount,
    int AttachmentsCount
);

public record TicketDetailsResponse
(
    Guid Id,
    int Number,
    string FormattedNumber,
    string Title,
    string Description,
    string Steps,
    string? ExpectedResult,
    string? ActualResult,
    Priority Priority,
    TicketStatus Status,
    DateTime PlannedStart,
    DateTime Deadline,
    DateTime CreatedAt,
    Guid ProjectId,
    string ProjectCode,
    string ProjectName,
    Guid? ReleaseId,
    string? ReleaseVersion,
    Guid AuthorId,
    string AuthorName,
    Guid? VerifierId,
    string? VerifierName,
    Guid? AssigneeId,
    string? AssigneeName,
    DateTime? TakenAt,
    List<CommentResponse> Comments,
    List<HistoryEntryResponse> HistoryEntries,
    List<FixResponse> Fixes,
    List<DeadlineChangeRequestResponse> DeadlineChangeRequests,
    List<AttachmentResponse> Attachments
);

public record CommentResponse
(
    Guid Id,
    string Text,
    DateTime CreatedAt,
    Guid AuthorId,
    string AuthorName
);

public record HistoryEntryResponse
(
    Guid Id,
    string Text,
    DateTime CreatedAt,
    Guid AuthorId,
    string AuthorName
);

public record FixResponse
(
    Guid Id,
    string Result,
    List<string> CommitLinks,
    string ReleaseVersion,
    DateTime CreatedAt,
    string? CheckComment,
    FixDecision Decision,
    Guid AuthorId,
    string AuthorName
);

public record DeadlineChangeRequestResponse
(
    Guid Id,
    DateTime NewDate,
    string Reason,
    DateTime? CounterDate,
    DateTime CreatedAt,
    DeadlineDecision Decision,
    Guid AuthorId,
    string AuthorName,
    Guid? ResolvedById,
    string? ResolvedByName
);

public record AttachmentResponse
(
    Guid Id,
    string FilePath,
    long Size,
    AttachmentType Type,
    Guid UploadedById,
    string UploadedByName
);
