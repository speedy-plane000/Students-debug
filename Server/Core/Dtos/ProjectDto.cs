using Core.Enums;

public record CreateProjectRequest
(
    string Code,
    string Name,
    string? Description,
    string RepositoryUrl
);

public record UpdateProjectRequest
(
    string? Code,
    string? Name,
    string? Description,
    string? RepositoryUrl
);

public record InviteMemberRequest
(
    string Email
);

public record JoinProjectRequest
(
    string Code
);

public record SetRoleRequest
(
    ProjectRole Role
);

public record DeleteProjectRequest
(
    string ConfirmName
);

public record ProjectResponse
(
    Guid Id,
    string Code,
    string Name,
    string? Description,
    string RepositoryUrl,
    Guid OwnerId,
    string OwnerName,
    ProjectRole? Role,
    bool IsOwner,
    int MembersCount,
    int OpenTicketsCount
);

public record ProjectMemberResponse
(
    Guid Id,
    Guid UserId,
    string FullName,
    string Email,
    ProjectRole? Role,
    MemberStatus Status,
    bool IsOwner
);

public record InviteMemberResponse
(
    Guid MemberId,
    string Email,
    string InviteCode
);
