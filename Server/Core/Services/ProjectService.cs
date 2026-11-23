using System.Security.Cryptography;
using Core.Enums;
using Microsoft.EntityFrameworkCore;

public class ProjectService : IProjectService
{
    private readonly IDebugDbContext context;
    public ProjectService(IDebugDbContext _context)
    {
        context = _context;
    }

    public async Task<Result<Guid>> Create(Guid userId, CreateProjectRequest request)
    {
        var isExistCode = await context.Projects.AnyAsync(p => p.Code == request.Code);
        if (isExistCode)
        {
            return new Result<Guid>(ErrorType.Conflict, "Code does exist");
        }
        var project = new ProjectEntity
        {
            Id = Guid.NewGuid(),
            Code = request.Code,
            Name = request.Name,
            RepositoryUrl = request.RepositoryUrl,
            Description = request.Description == null ? "" : request.Description
        };

        var ownerMember = new ProjectMemberEntity
        {
            Id = Guid.NewGuid(),
            IsOwner = true,
            InviteCode = await GenerateUniqueInviteCode(),
            InvitedAt = DateTime.UtcNow,
            Status = MemberStatus.Active,
            Role = ProjectRole.Lead,
            ProjectId = project.Id,
            UserId = userId
        };
        project.ProjectMembers.Add(ownerMember);

        await context.Projects.AddAsync(project);
        await context.SaveChangesAsync();

        return new Result<Guid>(project.Id);
    }

    public async Task<Result<int>> Edit(Guid projectId, UpdateProjectRequest request, Guid userId)
    {
        var project = await context.Projects.FirstOrDefaultAsync(p => p.Id == projectId);
        if (project == null)
        {
            return new Result<int>(ErrorType.NotFound, "Project not found");
        }

        var caller = await context.ProjectMembers
            .FirstOrDefaultAsync(pm => pm.ProjectId == projectId && pm.UserId == userId && pm.Status == MemberStatus.Active);
        if (caller == null || caller.Role != ProjectRole.Lead)
        {
            return new Result<int>(ErrorType.Forbidden, "Only project lead can edit project");
        }

        if (!string.IsNullOrWhiteSpace(request.Code) && request.Code != project.Code)
        {
            var isExistCode = await context.Projects.AnyAsync(p => p.Code == request.Code);
            if (isExistCode)
            {
                return new Result<int>(ErrorType.Conflict, "Code does exist");
            }
            project.Code = request.Code;
        }

        if (request.Name != null) { project.Name = request.Name; }
        if (request.Description != null) { project.Description = request.Description; }
        if (request.RepositoryUrl != null) { project.RepositoryUrl = request.RepositoryUrl; }

        await context.SaveChangesAsync();
        return new Result<int>(1);
    }

    public async Task<Result<int>> Delete(Guid projectId, DeleteProjectRequest request, Guid userId)
    {
        var project = await context.Projects.FirstOrDefaultAsync(p => p.Id == projectId);
        if (project == null)
        {
            return new Result<int>(ErrorType.NotFound, "Project not found");
        }

        var caller = await context.ProjectMembers
            .FirstOrDefaultAsync(pm => pm.ProjectId == projectId && pm.UserId == userId && pm.Status == MemberStatus.Active);
        if (caller == null || caller.Role != ProjectRole.Lead)
        {
            return new Result<int>(ErrorType.Forbidden, "Only project lead can delete project");
        }

        if (project.Name != request.ConfirmName)
        {
            return new Result<int>(ErrorType.BadRequest, "Project name does not match");
        }

        context.Projects.Remove(project);
        await context.SaveChangesAsync();

        return new Result<int>(1);
    }

    public async Task<Result<InviteMemberResponse>> Invite(Guid projectId, InviteMemberRequest request, Guid userId)
    {
        var project = await context.Projects.FirstOrDefaultAsync(p => p.Id == projectId);
        if (project == null)
        {
            return new Result<InviteMemberResponse>(ErrorType.NotFound, "Project not found");
        }

        var caller = await context.ProjectMembers
            .FirstOrDefaultAsync(pm => pm.ProjectId == projectId && pm.UserId == userId && pm.Status == MemberStatus.Active);
        if (caller == null || caller.Role != ProjectRole.Lead)
        {
            return new Result<InviteMemberResponse>(ErrorType.Forbidden, "Only project lead can invite members");
        }

        var user = await context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user == null)
        {
            return new Result<InviteMemberResponse>(ErrorType.NotFound, "User not found");
        }

        var existingMember = await context.ProjectMembers
            .FirstOrDefaultAsync(pm => pm.ProjectId == projectId && pm.UserId == user.Id);
        if (existingMember != null)
        {
            if (existingMember.Status != MemberStatus.Removed)
            {
                return new Result<InviteMemberResponse>(ErrorType.Conflict, "User is already a member of this project");
            }

            var newCode = await GenerateUniqueInviteCode();
            existingMember.Status = MemberStatus.Invited;
            existingMember.Role = null;
            existingMember.InviteCode = newCode;
            existingMember.InvitedAt = DateTime.UtcNow;
            existingMember.User = user;

            await context.SaveChangesAsync();
            return new Result<InviteMemberResponse>(new InviteMemberResponse(existingMember.Id, user.Email, newCode));
        }

        var inviteCode = await GenerateUniqueInviteCode();
        var projectMember = new ProjectMemberEntity
        {
            Id = Guid.NewGuid(),
            IsOwner = false,
            InviteCode = inviteCode,
            InvitedAt = DateTime.UtcNow,
            Status = MemberStatus.Invited,
            Role = null,
            ProjectId = projectId,
            UserId = user.Id,
            User = user
        };

        await context.ProjectMembers.AddAsync(projectMember);
        await context.SaveChangesAsync();

        return new Result<InviteMemberResponse>(new InviteMemberResponse(projectMember.Id, user.Email, inviteCode));
    }

    public async Task<Result<Guid>> JoinByCode(JoinProjectRequest request, Guid userId)
    {
        var code = request.Code.Trim().ToUpper();
        var member = await context.ProjectMembers
            .Include(pm => pm.User)
            .FirstOrDefaultAsync(pm => pm.InviteCode == code);

        if (member == null)
        {
            return new Result<Guid>(ErrorType.NotFound, "Invalid invite code");
        }

        if (member.UserId != userId)
        {
            return new Result<Guid>(ErrorType.Forbidden, "This invite is for another user");
        }

        if (member.Status == MemberStatus.Active)
        {
            return new Result<Guid>(ErrorType.Conflict, "User is already active in this project");
        }

        member.Status = MemberStatus.Active;

        var leads = await context.ProjectMembers
            .Where(pm => pm.ProjectId == member.ProjectId && pm.Role == ProjectRole.Lead && pm.Status == MemberStatus.Active)
            .ToListAsync();

        foreach (var lead in leads)
        {
            await context.Notifications.AddAsync(new NotificationEntity
            {
                Id = Guid.NewGuid(),
                Text = $"{member.User.FullName} вошел в проект по коду приглашения",
                CreatedAt = DateTime.UtcNow,
                isRead = false,
                UserId = lead.UserId
            });
        }

        await context.SaveChangesAsync();

        return new Result<Guid>(member.ProjectId);
    }

    public async Task<Result<int>> SetRole(Guid projectId, Guid memberId, SetRoleRequest request, Guid userId)
    {
        var project = await context.Projects.FirstOrDefaultAsync(p => p.Id == projectId);
        if (project == null)
        {
            return new Result<int>(ErrorType.NotFound, "Project not found");
        }

        var caller = await context.ProjectMembers
            .FirstOrDefaultAsync(pm => pm.ProjectId == projectId && pm.UserId == userId && pm.Status == MemberStatus.Active);
        if (caller == null || caller.Role != ProjectRole.Lead)
        {
            return new Result<int>(ErrorType.Forbidden, "Only project lead can assign roles");
        }

        var member = await context.ProjectMembers
            .FirstOrDefaultAsync(pm => pm.ProjectId == projectId && (pm.Id == memberId || pm.UserId == memberId));

        if (member == null || member.Status == MemberStatus.Removed)
        {
            return new Result<int>(ErrorType.NotFound, "Member not found");
        }

        if (member.IsOwner)
        {
            return new Result<int>(ErrorType.Forbidden, "Cannot change role of project owner");
        }

        if (member.Role == ProjectRole.Developer && request.Role != ProjectRole.Developer)
        {
            var openTickets = await context.Tickets
                .Include(t => t.Reservation)
                .Include(t => t.DeadlineChangeRequests)
                .Where(t => t.ProjectId == projectId && t.Reservation != null && t.Reservation.DeveloperId == member.Id && t.Status != TicketStatus.Done)
                .ToListAsync();

            foreach (var ticket in openTickets)
            {
                if (ticket.Reservation != null)
                {
                    context.Reservations.Remove(ticket.Reservation);
                    ticket.Reservation = null;
                }
                ticket.Status = TicketStatus.InPool;

                foreach (var req in ticket.DeadlineChangeRequests.Where(r => r.AuthorId == member.Id && r.Decision == DeadlineDecision.Pending))
                {
                    req.Decision = DeadlineDecision.Rejected;
                }
            }
        }

        member.Role = request.Role;
        member.Status = MemberStatus.Active;

        await context.Notifications.AddAsync(new NotificationEntity
        {
            Id = Guid.NewGuid(),
            Text = $"Вам назначена роль {request.Role} в проекте",
            CreatedAt = DateTime.UtcNow,
            isRead = false,
            UserId = member.UserId
        });

        await context.SaveChangesAsync();

        return new Result<int>(1);
    }

    public async Task<Result<int>> RemoveMember(Guid projectId, Guid memberId, Guid userId)
    {
        var project = await context.Projects.FirstOrDefaultAsync(p => p.Id == projectId);
        if (project == null)
        {
            return new Result<int>(ErrorType.NotFound, "Project not found");
        }

        var caller = await context.ProjectMembers
            .FirstOrDefaultAsync(pm => pm.ProjectId == projectId && pm.UserId == userId && pm.Status == MemberStatus.Active);
        if (caller == null || caller.Role != ProjectRole.Lead)
        {
            return new Result<int>(ErrorType.Forbidden, "Only project lead can remove members");
        }

        var member = await context.ProjectMembers
            .FirstOrDefaultAsync(pm => pm.ProjectId == projectId && (pm.Id == memberId || pm.UserId == memberId));

        if (member == null || member.Status == MemberStatus.Removed)
        {
            return new Result<int>(ErrorType.NotFound, "Member not found");
        }

        if (member.IsOwner)
        {
            return new Result<int>(ErrorType.Forbidden, "Cannot remove project owner");
        }

        var tickets = await context.Tickets
            .Include(t => t.Reservation)
            .Include(t => t.DeadlineChangeRequests)
            .Where(t => t.ProjectId == projectId && t.Status != TicketStatus.Done && (
                (t.Reservation != null && t.Reservation.DeveloperId == member.Id) ||
                t.VerifierId == member.Id
            ))
            .ToListAsync();

        foreach (var ticket in tickets)
        {
            if (ticket.Reservation != null && ticket.Reservation.DeveloperId == member.Id)
            {
                context.Reservations.Remove(ticket.Reservation);
                ticket.Reservation = null;
                ticket.Status = TicketStatus.InPool;

                foreach (var req in ticket.DeadlineChangeRequests.Where(r => r.AuthorId == member.Id && r.Decision == DeadlineDecision.Pending))
                {
                    req.Decision = DeadlineDecision.Rejected;
                }
            }

            if (ticket.VerifierId == member.Id)
            {
                ticket.VerifierId = null;
            }
        }

        member.Status = MemberStatus.Removed;
        member.Role = null;

        await context.SaveChangesAsync();

        return new Result<int>(1);
    }

    public async Task<Result<ProjectResponse>> GetById(Guid projectId, Guid userId)
    {
        var project = await context.Projects
            .Include(p => p.ProjectMembers)
                .ThenInclude(pm => pm.User)
            .Include(p => p.Tickets)
            .FirstOrDefaultAsync(p => p.Id == projectId);

        if (project == null)
        {
            return new Result<ProjectResponse>(ErrorType.NotFound, "Project not found");
        }

        var currentMember = project.ProjectMembers.FirstOrDefault(pm => pm.UserId == userId && pm.Status == MemberStatus.Active);
        if (currentMember == null)
        {
            return new Result<ProjectResponse>(ErrorType.Forbidden, "Access denied");
        }

        var owner = project.ProjectMembers.FirstOrDefault(pm => pm.IsOwner);
        var ownerName = owner?.User?.FullName ?? "";
        var ownerId = owner?.UserId ?? Guid.Empty;
        var membersCount = project.ProjectMembers.Count(pm => pm.Status != MemberStatus.Removed);
        var openTicketsCount = project.Tickets.Count(t => t.Status != TicketStatus.Done);

        var response = new ProjectResponse(
            project.Id,
            project.Code,
            project.Name,
            project.Description,
            project.RepositoryUrl,
            ownerId,
            ownerName,
            currentMember.Role,
            currentMember.IsOwner,
            membersCount,
            openTicketsCount
        );

        return new Result<ProjectResponse>(response);
    }

    public async Task<Result<List<ProjectResponse>>> GetUserProjects(Guid userId)
    {
        var projects = await context.Projects
            .Include(p => p.ProjectMembers)
                .ThenInclude(pm => pm.User)
            .Include(p => p.Tickets)
            .Where(p => p.ProjectMembers.Any(pm => pm.UserId == userId && pm.Status == MemberStatus.Active))
            .ToListAsync();

        var list = new List<ProjectResponse>();
        foreach (var project in projects)
        {
            var currentMember = project.ProjectMembers.First(pm => pm.UserId == userId && pm.Status == MemberStatus.Active);
            var owner = project.ProjectMembers.FirstOrDefault(pm => pm.IsOwner);
            var ownerName = owner?.User?.FullName ?? "";
            var ownerId = owner?.UserId ?? Guid.Empty;
            var membersCount = project.ProjectMembers.Count(pm => pm.Status != MemberStatus.Removed);
            var openTicketsCount = project.Tickets.Count(t => t.Status != TicketStatus.Done);

            list.Add(new ProjectResponse(
                project.Id,
                project.Code,
                project.Name,
                project.Description,
                project.RepositoryUrl,
                ownerId,
                ownerName,
                currentMember.Role,
                currentMember.IsOwner,
                membersCount,
                openTicketsCount
            ));
        }

        return new Result<List<ProjectResponse>>(list);
    }

    public async Task<Result<List<ProjectMemberResponse>>> GetMembers(Guid projectId, Guid userId)
    {
        var isMember = await context.ProjectMembers.AnyAsync(pm => pm.ProjectId == projectId && pm.UserId == userId && pm.Status == MemberStatus.Active);
        if (!isMember)
        {
            return new Result<List<ProjectMemberResponse>>(ErrorType.Forbidden, "Access denied");
        }

        var members = await context.ProjectMembers
            .Include(pm => pm.User)
            .Where(pm => pm.ProjectId == projectId && pm.Status != MemberStatus.Removed)
            .ToListAsync();

        var list = members.Select(m => new ProjectMemberResponse(
            m.Id,
            m.UserId,
            m.User.FullName,
            m.User.Email,
            m.Role,
            m.Status,
            m.IsOwner
        )).ToList();

        return new Result<List<ProjectMemberResponse>>(list);
    }

    public async Task<Result<ReleaseEntity>> GetOrAddRelease(Guid projectId, string version, Guid userId)
    {
        var isMember = await context.ProjectMembers.AnyAsync(pm => pm.ProjectId == projectId && pm.UserId == userId && pm.Status == MemberStatus.Active);
        if (!isMember)
        {
            return new Result<ReleaseEntity>(ErrorType.Forbidden, "Access denied");
        }

        var release = await context.Releases.FirstOrDefaultAsync(r => r.ProjectId == projectId && r.Version == version);
        if (release != null)
        {
            return new Result<ReleaseEntity>(release);
        }

        release = new ReleaseEntity
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            Version = version,
            CreatedAt = DateTime.UtcNow
        };

        await context.Releases.AddAsync(release);
        await context.SaveChangesAsync();

        return new Result<ReleaseEntity>(release);
    }

    private async Task<string> GenerateUniqueInviteCode()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        while (true)
        {
            var code = $"PRJ-{RandomNumberGenerator.GetString(chars, 4)}-{RandomNumberGenerator.GetString(chars, 4)}";
            var exists = await context.ProjectMembers.AnyAsync(pm => pm.InviteCode == code);
            if (!exists)
            {
                return code;
            }
        }
    }
}
