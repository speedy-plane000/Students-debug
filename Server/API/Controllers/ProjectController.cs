using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
[ApiController]
[Route("projects")]
public class ProjectController : ControllerBase
{
    private readonly IProjectService projectService;
    public ProjectController(IProjectService _projectService)
    {
        projectService = _projectService;
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Create([FromBody] CreateProjectRequest request)
    {
        var userId = GetUserId();
        var result = await projectService.Create(userId, request);
        if (!result.IsSuccess)
        {
            return HandleError(result);
        }
        return Ok(result.Value);
    }

    [HttpGet]
    public async Task<ActionResult<List<ProjectResponse>>> GetUserProjects()
    {
        var userId = GetUserId();
        var result = await projectService.GetUserProjects(userId);
        if (!result.IsSuccess)
        {
            return HandleError(result);
        }
        return Ok(result.Value);
    }

    [HttpGet("{projectId}")]
    public async Task<ActionResult<ProjectResponse>> GetById([FromRoute] Guid projectId)
    {
        var userId = GetUserId();
        var result = await projectService.GetById(projectId, userId);
        if (!result.IsSuccess)
        {
            return HandleError(result);
        }
        return Ok(result.Value);
    }

    [HttpPut("{projectId}")]
    public async Task<IActionResult> Edit(
        [FromRoute] Guid projectId,
        [FromBody] UpdateProjectRequest request)
    {
        var userId = GetUserId();
        var result = await projectService.Edit(projectId, request, userId);
        if (!result.IsSuccess)
        {
            return HandleError(result);
        }
        return Ok();
    }

    [HttpDelete("{projectId}")]
    public async Task<IActionResult> Delete(
        [FromRoute] Guid projectId,
        [FromBody] DeleteProjectRequest request)
    {
        var userId = GetUserId();
        var result = await projectService.Delete(projectId, request, userId);
        if (!result.IsSuccess)
        {
            return HandleError(result);
        }
        return Ok();
    }

    [HttpPost("{projectId}/invite")]
    public async Task<ActionResult<InviteMemberResponse>> Invite(
        [FromRoute] Guid projectId,
        [FromBody] InviteMemberRequest request)
    {
        var userId = GetUserId();
        var result = await projectService.Invite(projectId, request, userId);
        if (!result.IsSuccess)
        {
            return HandleError(result);
        }
        return Ok(result.Value);
    }

    [HttpPost("join")]
    public async Task<ActionResult<Guid>> JoinByCode([FromBody] JoinProjectRequest request)
    {
        var userId = GetUserId();
        var result = await projectService.JoinByCode(request, userId);
        if (!result.IsSuccess)
        {
            return HandleError(result);
        }
        return Ok(result.Value);
    }

    [HttpGet("{projectId}/members")]
    public async Task<ActionResult<List<ProjectMemberResponse>>> GetMembers([FromRoute] Guid projectId)
    {
        var userId = GetUserId();
        var result = await projectService.GetMembers(projectId, userId);
        if (!result.IsSuccess)
        {
            return HandleError(result);
        }
        return Ok(result.Value);
    }

    [HttpPut("{projectId}/members/{memberId}/role")]
    public async Task<IActionResult> SetRole(
        [FromRoute] Guid projectId,
        [FromRoute] Guid memberId,
        [FromBody] SetRoleRequest request)
    {
        var userId = GetUserId();
        var result = await projectService.SetRole(projectId, memberId, request, userId);
        if (!result.IsSuccess)
        {
            return HandleError(result);
        }
        return Ok();
    }

    [HttpDelete("{projectId}/members/{memberId}")]
    public async Task<IActionResult> RemoveMember(
        [FromRoute] Guid projectId,
        [FromRoute] Guid memberId)
    {
        var userId = GetUserId();
        var result = await projectService.RemoveMember(projectId, memberId, userId);
        if (!result.IsSuccess)
        {
            return HandleError(result);
        }
        return Ok();
    }

    private Guid GetUserId()
    {
        var claim = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
        return Guid.TryParse(claim, out var userId) ? userId : Guid.Empty;
    }

    private ActionResult HandleError<T>(Result<T> result)
    {
        return result.ErrorType switch
        {
            ErrorType.NotFound => NotFound(result.TextError),
            ErrorType.Forbidden => StatusCode(403, result.TextError),
            ErrorType.Conflict => StatusCode(409, result.TextError),
            _ => BadRequest(result.TextError)
        };
    }
}
