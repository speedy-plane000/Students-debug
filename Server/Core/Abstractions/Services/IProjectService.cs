using Core.Enums;

public interface IProjectService
{
    Task<Result<Guid>> Create(Guid userId, CreateProjectRequest request);
    Task<Result<int>> Edit(Guid projectId, UpdateProjectRequest request, Guid userId);
    Task<Result<int>> Delete(Guid projectId, DeleteProjectRequest request, Guid userId);
    Task<Result<InviteMemberResponse>> Invite(Guid projectId, InviteMemberRequest request, Guid userId);
    Task<Result<Guid>> JoinByCode(JoinProjectRequest request, Guid userId);
    Task<Result<int>> SetRole(Guid projectId, Guid memberId, SetRoleRequest request, Guid userId);
    Task<Result<int>> RemoveMember(Guid projectId, Guid memberId, Guid userId);
    Task<Result<ProjectResponse>> GetById(Guid projectId, Guid userId);
    Task<Result<List<ProjectResponse>>> GetUserProjects(Guid userId);
    Task<Result<List<ProjectMemberResponse>>> GetMembers(Guid projectId, Guid userId);
    Task<Result<ReleaseEntity>> GetOrAddRelease(Guid projectId, string version, Guid userId);
}
