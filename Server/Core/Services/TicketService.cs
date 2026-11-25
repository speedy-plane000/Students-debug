using Core.Enums;
using Microsoft.EntityFrameworkCore;

public class TicketService : ITicketService
{
    private readonly IDebugDbContext context;

    public TicketService(IDebugDbContext _context)
    {
        context = _context;
    }

    public async Task<Result<Guid>> Create(Guid userId, CreateTicketRequest request)
    {
        var caller = await context.ProjectMembers
            .FirstOrDefaultAsync(pm => pm.ProjectId == request.ProjectId && pm.UserId == userId && pm.Status == MemberStatus.Active);
        if (caller == null){ return new Result<Guid>(ErrorType.Forbidden, "Access denied");}

        var project = await context.Projects.FirstOrDefaultAsync(p => p.Id == request.ProjectId);
        if (project == null){ return new Result<Guid>(ErrorType.NotFound, "Project not found");}

        var maxNumber = await context.Tickets
            .Where(t => t.ProjectId == request.ProjectId)
            .MaxAsync(t => (int?)t.Number) ?? 0;
        var nextNumber = maxNumber + 1;

        var ticket = new TicketEntity
        {
            Id = Guid.NewGuid(),
            Number = nextNumber,
            Title = request.Title.Trim(),
            Description = request.Description,
            Steps = request.Steps,
            ExpectedResult = request.ExpectedResult,
            ActualResult = request.ActualResult,
            Priority = request.Priority,
            Status = TicketStatus.OnReview,
            PlannedStart = request.PlannedStart,
            Deadline = request.Deadline,
            CreatedAt = DateTime.UtcNow,
            ProjectId = request.ProjectId,
            AuthorId = caller.Id
        };

        await context.Tickets.AddAsync(ticket);

        var history = new HistoryEntryEntity
        {
            Id = Guid.NewGuid(),
            Text = "Тикет создан",
            CreatedAt = DateTime.UtcNow,
            TicketId = ticket.Id,
            AuthorId = caller.Id
        };
        await context.HistoryEntries.AddAsync(history);

        var leads = await context.ProjectMembers
            .Where(pm => pm.ProjectId == request.ProjectId && pm.Role == ProjectRole.Lead && pm.Status == MemberStatus.Active && pm.UserId != userId)
            .ToListAsync();

        foreach (var lead in leads)
        {
            await context.Notifications.AddAsync(new NotificationEntity
            {
                Id = Guid.NewGuid(),
                Text = $"Создан новый тикет #{nextNumber} '{ticket.Title}'",
                CreatedAt = DateTime.UtcNow,
                isRead = false,
                UserId = lead.UserId
            });
        }

        await context.SaveChangesAsync();

        return new Result<Guid>(ticket.Id);
    }

    public async Task<Result<int>> Edit(Guid ticketId, UpdateTicketRequest request, Guid userId)
    {
        var ticket = await context.Tickets
            .Include(t => t.Reservation)
            .FirstOrDefaultAsync(t => t.Id == ticketId);
        if (ticket == null)
        {
            return new Result<int>(ErrorType.NotFound, "Ticket not found");
        }

        var caller = await context.ProjectMembers
            .FirstOrDefaultAsync(pm => pm.ProjectId == ticket.ProjectId && pm.UserId == userId && pm.Status == MemberStatus.Active);
        if (caller == null || caller.Role != ProjectRole.Lead)
        {
            return new Result<int>(ErrorType.Forbidden, "Only project lead can edit ticket");
        }

        if (ticket.Status == TicketStatus.OnCheck || ticket.Status == TicketStatus.Done)
        {
            return new Result<int>(ErrorType.Conflict, "Cannot edit ticket in OnCheck or Done status");
        }

        if (request.ProjectId.HasValue && request.ProjectId.Value != ticket.ProjectId)
        {
            var destProject = await context.Projects.FirstOrDefaultAsync(p => p.Id == request.ProjectId.Value);
            if (destProject == null)
            {
                return new Result<int>(ErrorType.NotFound, "Target project not found");
            }

            var destLead = await context.ProjectMembers
                .FirstOrDefaultAsync(pm => pm.ProjectId == request.ProjectId.Value && pm.UserId == userId && pm.Status == MemberStatus.Active);
            if (destLead == null || destLead.Role != ProjectRole.Lead)
            {
                return new Result<int>(ErrorType.Forbidden, "Must be Lead in destination project to move ticket");
            }

            if (ticket.Reservation != null)
            {
                context.Reservations.Remove(ticket.Reservation);
                ticket.Reservation = null;
            }

            var pendingDeadlines = await context.DeadlineChangeRequests
                .Where(d => d.TicketId == ticket.Id && d.Decision == DeadlineDecision.Pending)
                .ToListAsync();
            foreach (var d in pendingDeadlines)
            {
                d.Decision = DeadlineDecision.Rejected;
            }

            var maxNumber = await context.Tickets
                .Where(t => t.ProjectId == request.ProjectId.Value)
                .MaxAsync(t => (int?)t.Number) ?? 0;

            ticket.Number = maxNumber + 1;
            ticket.ProjectId = request.ProjectId.Value;
            ticket.AuthorId = destLead.Id;
            ticket.VerifierId = null;
            ticket.Status = TicketStatus.OnReview;
        }

        var plannedStart = request.PlannedStart ?? ticket.PlannedStart;
        var deadline = request.Deadline ?? ticket.Deadline;
        if (deadline < plannedStart)
        {
            return new Result<int>(ErrorType.BadRequest, "Deadline cannot be earlier than PlannedStart");
        }

        ticket.PlannedStart = plannedStart;
        ticket.Deadline = deadline;

        if (request.Title != null){ticket.Title = request.Title.Trim();}

        if (request.Description != null) { ticket.Description = request.Description; }
        if (request.Steps != null) { ticket.Steps = request.Steps; }
        if (request.ExpectedResult != null) { ticket.ExpectedResult = request.ExpectedResult; }
        if (request.ActualResult != null) { ticket.ActualResult = request.ActualResult; }
        if (request.Priority.HasValue) { ticket.Priority = request.Priority.Value; }

        var history = new HistoryEntryEntity
        {
            Id = Guid.NewGuid(),
            Text = "Тикет отредактирован",
            CreatedAt = DateTime.UtcNow,
            TicketId = ticket.Id,
            AuthorId = caller.Id
        };
        await context.HistoryEntries.AddAsync(history);

        await context.SaveChangesAsync();

        return new Result<int>(1);
    }

    public async Task<Result<int>> Delete(Guid ticketId, Guid userId)
    {
        var ticket = await context.Tickets
            .Include(t => t.Reservation)
            .FirstOrDefaultAsync(t => t.Id == ticketId);
        if (ticket == null)
        {
            return new Result<int>(ErrorType.NotFound, "Ticket not found");
        }

        var caller = await context.ProjectMembers
            .FirstOrDefaultAsync(pm => pm.ProjectId == ticket.ProjectId && pm.UserId == userId && pm.Status == MemberStatus.Active);
        if (caller == null || caller.Role != ProjectRole.Lead)
        {
            return new Result<int>(ErrorType.Forbidden, "Only project lead can delete ticket");
        }

        if ((ticket.Status != TicketStatus.OnReview && ticket.Status != TicketStatus.InPool) || ticket.Reservation != null)
        {
            return new Result<int>(ErrorType.Conflict, "Can only delete tickets in OnReview or InPool status without reservation");
        }

        context.Tickets.Remove(ticket);
        await context.SaveChangesAsync();

        return new Result<int>(1);
    }

    public async Task<Result<int>> Assign(Guid ticketId, AssignTicketRequest request, Guid userId)
    {
        var ticket = await context.Tickets
            .Include(t => t.Reservation)
            .FirstOrDefaultAsync(t => t.Id == ticketId);
        if (ticket == null)
        {
            return new Result<int>(ErrorType.NotFound, "Ticket not found");
        }

        var caller = await context.ProjectMembers
            .FirstOrDefaultAsync(pm => pm.ProjectId == ticket.ProjectId && pm.UserId == userId && pm.Status == MemberStatus.Active);
        if (caller == null || caller.Role != ProjectRole.Lead)
        {
            return new Result<int>(ErrorType.Forbidden, "Only project lead can assign ticket");
        }

        if (ticket.Status == TicketStatus.Done)
        {
            return new Result<int>(ErrorType.Conflict, "Cannot assign finished ticket");
        }

        var devMember = await context.ProjectMembers
            .Include(pm => pm.User)
            .FirstOrDefaultAsync(pm => pm.ProjectId == ticket.ProjectId && (pm.Id == request.DeveloperId || pm.UserId == request.DeveloperId) && pm.Status == MemberStatus.Active);
        if (devMember == null)
        {
            return new Result<int>(ErrorType.NotFound, "Developer not found in project");
        }

        if (devMember.Role != ProjectRole.Developer && devMember.Role != ProjectRole.Lead)
        {
            return new Result<int>(ErrorType.BadRequest, "Member does not have Developer role");
        }

        if (ticket.Reservation != null)
        {
            ticket.Reservation.DeveloperId = devMember.Id;
            ticket.Reservation.AssignedById = caller.Id;
            ticket.Reservation.TakenAt = DateTime.UtcNow;
        }
        else
        {
            ticket.Reservation = new ReservationEntity
            {
                Id = Guid.NewGuid(),
                DeveloperId = devMember.Id,
                AssignedById = caller.Id,
                TakenAt = DateTime.UtcNow,
                TicketId = ticket.Id
            };
        }

        ticket.Status = TicketStatus.Assigned;

        var history = new HistoryEntryEntity
        {
            Id = Guid.NewGuid(),
            Text = $"Назначен разработчик: {devMember.User.FullName}",
            CreatedAt = DateTime.UtcNow,
            TicketId = ticket.Id,
            AuthorId = caller.Id
        };
        await context.HistoryEntries.AddAsync(history);

        await context.Notifications.AddAsync(new NotificationEntity
        {
            Id = Guid.NewGuid(),
            Text = $"Вам назначен тикет #{ticket.Number} '{ticket.Title}'",
            CreatedAt = DateTime.UtcNow,
            isRead = false,
            UserId = devMember.UserId
        });

        await context.SaveChangesAsync();

        return new Result<int>(1);
    }

    public async Task<Result<int>> SendToPool(Guid ticketId, Guid userId)
    {
        var ticket = await context.Tickets
            .Include(t => t.Reservation)
            .FirstOrDefaultAsync(t => t.Id == ticketId);
        if (ticket == null)
        {
            return new Result<int>(ErrorType.NotFound, "Ticket not found");
        }

        var caller = await context.ProjectMembers
            .FirstOrDefaultAsync(pm => pm.ProjectId == ticket.ProjectId && pm.UserId == userId && pm.Status == MemberStatus.Active);
        if (caller == null || caller.Role != ProjectRole.Lead)
        {
            return new Result<int>(ErrorType.Forbidden, "Only project lead can send ticket to pool");
        }

        if (ticket.Status == TicketStatus.Done)
        {
            return new Result<int>(ErrorType.Conflict, "Cannot send finished ticket to pool");
        }

        if (ticket.Reservation != null)
        {
            context.Reservations.Remove(ticket.Reservation);
            ticket.Reservation = null;
        }

        ticket.Status = TicketStatus.InPool;

        var history = new HistoryEntryEntity
        {
            Id = Guid.NewGuid(),
            Text = "Тикет отправлен в пул",
            CreatedAt = DateTime.UtcNow,
            TicketId = ticket.Id,
            AuthorId = caller.Id
        };
        await context.HistoryEntries.AddAsync(history);

        await context.SaveChangesAsync();

        return new Result<int>(1);
    }

    public async Task<Result<int>> Take(Guid ticketId, Guid userId)
    {
        var ticket = await context.Tickets
            .Include(t => t.Reservation)
            .FirstOrDefaultAsync(t => t.Id == ticketId);
        if (ticket == null)
        {
            return new Result<int>(ErrorType.NotFound, "Ticket not found");
        }

        var caller = await context.ProjectMembers
            .FirstOrDefaultAsync(pm => pm.ProjectId == ticket.ProjectId && pm.UserId == userId && pm.Status == MemberStatus.Active);
        if (caller == null || (caller.Role != ProjectRole.Developer && caller.Role != ProjectRole.Lead))
        {
            return new Result<int>(ErrorType.Forbidden, "Only developers can take tickets from pool");
        }

        if (ticket.Status != TicketStatus.InPool || ticket.Reservation != null)
        {
            return new Result<int>(ErrorType.Conflict, "Ticket is not available in pool");
        }

        ticket.Reservation = new ReservationEntity
        {
            Id = Guid.NewGuid(),
            DeveloperId = caller.Id,
            AssignedById = null,
            TakenAt = DateTime.UtcNow,
            TicketId = ticket.Id
        };
        ticket.Status = TicketStatus.InProgress;

        var history = new HistoryEntryEntity
        {
            Id = Guid.NewGuid(),
            Text = "Тикет взят в работу из пула",
            CreatedAt = DateTime.UtcNow,
            TicketId = ticket.Id,
            AuthorId = caller.Id
        };
        await context.HistoryEntries.AddAsync(history);

        await context.SaveChangesAsync();

        return new Result<int>(1);
    }

    public async Task<Result<int>> StartWork(Guid ticketId, Guid userId)
    {
        var ticket = await context.Tickets
            .Include(t => t.Reservation)
            .FirstOrDefaultAsync(t => t.Id == ticketId);
        if (ticket == null)
        {
            return new Result<int>(ErrorType.NotFound, "Ticket not found");
        }

        var caller = await context.ProjectMembers
            .FirstOrDefaultAsync(pm => pm.ProjectId == ticket.ProjectId && pm.UserId == userId && pm.Status == MemberStatus.Active);
        if (caller == null || ticket.Reservation == null || ticket.Reservation.DeveloperId != caller.Id)
        {
            return new Result<int>(ErrorType.Forbidden, "Only assigned developer can start work");
        }

        if (ticket.Status != TicketStatus.Assigned && ticket.Status != TicketStatus.Paused && ticket.Status != TicketStatus.Rework)
        {
            return new Result<int>(ErrorType.Conflict, "Cannot start work from current status");
        }

        ticket.Status = TicketStatus.InProgress;

        var history = new HistoryEntryEntity
        {
            Id = Guid.NewGuid(),
            Text = "Работа над тикетом начата",
            CreatedAt = DateTime.UtcNow,
            TicketId = ticket.Id,
            AuthorId = caller.Id
        };
        await context.HistoryEntries.AddAsync(history);

        await context.SaveChangesAsync();

        return new Result<int>(1);
    }

    public async Task<Result<int>> Pause(Guid ticketId, Guid userId)
    {
        var ticket = await context.Tickets
            .Include(t => t.Reservation)
            .FirstOrDefaultAsync(t => t.Id == ticketId);
        if (ticket == null)
        {
            return new Result<int>(ErrorType.NotFound, "Ticket not found");
        }

        var caller = await context.ProjectMembers
            .FirstOrDefaultAsync(pm => pm.ProjectId == ticket.ProjectId && pm.UserId == userId && pm.Status == MemberStatus.Active);
        if (caller == null || ticket.Reservation == null || ticket.Reservation.DeveloperId != caller.Id)
        {
            return new Result<int>(ErrorType.Forbidden, "Only assigned developer can pause work");
        }

        if (ticket.Status != TicketStatus.InProgress)
        {
            return new Result<int>(ErrorType.Conflict, "Can only pause in-progress ticket");
        }

        ticket.Status = TicketStatus.Paused;

        var history = new HistoryEntryEntity
        {
            Id = Guid.NewGuid(),
            Text = "Работа над тикетом приостановлена",
            CreatedAt = DateTime.UtcNow,
            TicketId = ticket.Id,
            AuthorId = caller.Id
        };
        await context.HistoryEntries.AddAsync(history);

        await context.SaveChangesAsync();

        return new Result<int>(1);
    }

    public async Task<Result<int>> SubmitFix(Guid ticketId, SubmitFixRequest request, Guid userId)
    {
        var ticket = await context.Tickets
            .Include(t => t.Reservation)
            .Include(t => t.Author)
            .FirstOrDefaultAsync(t => t.Id == ticketId);
        if (ticket == null)
        {
            return new Result<int>(ErrorType.NotFound, "Ticket not found");
        }

        var caller = await context.ProjectMembers
            .FirstOrDefaultAsync(pm => pm.ProjectId == ticket.ProjectId && pm.UserId == userId && pm.Status == MemberStatus.Active);
        if (caller == null || ticket.Reservation == null || ticket.Reservation.DeveloperId != caller.Id)
        {
            return new Result<int>(ErrorType.Forbidden, "Only assigned developer can submit fix");
        }

        if (ticket.Status != TicketStatus.InProgress && ticket.Status != TicketStatus.Rework)
        {
            return new Result<int>(ErrorType.Conflict, "Can only submit fix for in-progress or rework ticket");
        }

        var fix = new FixEntity
        {
            Id = Guid.NewGuid(),
            Result = request.Result.Trim(),
            CommitLinks = request.CommitLinks.Where(c => !string.IsNullOrWhiteSpace(c)).Select(c => c.Trim()).ToList(),
            ReleaseVersion = request.ReleaseVersion.Trim(),
            CreatedAt = DateTime.UtcNow,
            Decision = FixDecision.Pending,
            TicketId = ticket.Id,
            AuthorId = caller.Id
        };
        await context.Fixes.AddAsync(fix);

        ticket.Status = TicketStatus.OnCheck;
        ticket.VerifierId = null;

        var history = new HistoryEntryEntity
        {
            Id = Guid.NewGuid(),
            Text = $"Решение отправлено на проверку (версия {fix.ReleaseVersion})",
            CreatedAt = DateTime.UtcNow,
            TicketId = ticket.Id,
            AuthorId = caller.Id
        };
        await context.HistoryEntries.AddAsync(history);

        if (ticket.Author != null && ticket.Author.UserId != userId)
        {
            await context.Notifications.AddAsync(new NotificationEntity
            {
                Id = Guid.NewGuid(),
                Text = $"По тикету #{ticket.Number} отправлено решение на проверку",
                CreatedAt = DateTime.UtcNow,
                isRead = false,
                UserId = ticket.Author.UserId
            });
        }

        var testers = await context.ProjectMembers
            .Where(pm => pm.ProjectId == ticket.ProjectId && pm.Role == ProjectRole.Tester && pm.Status == MemberStatus.Active && pm.UserId != userId)
            .ToListAsync();

        foreach (var tester in testers)
        {
            await context.Notifications.AddAsync(new NotificationEntity
            {
                Id = Guid.NewGuid(),
                Text = $"Тикет #{ticket.Number} '{ticket.Title}' ожидает проверки",
                CreatedAt = DateTime.UtcNow,
                isRead = false,
                UserId = tester.UserId
            });
        }

        await context.SaveChangesAsync();

        return new Result<int>(1);
    }

    public async Task<Result<int>> TakeForCheck(Guid ticketId, Guid userId)
    {
        var ticket = await context.Tickets.FirstOrDefaultAsync(t => t.Id == ticketId);
        if (ticket == null)
        {
            return new Result<int>(ErrorType.NotFound, "Ticket not found");
        }

        var caller = await context.ProjectMembers
            .Include(pm => pm.User)
            .FirstOrDefaultAsync(pm => pm.ProjectId == ticket.ProjectId && pm.UserId == userId && pm.Status == MemberStatus.Active);
        if (caller == null || (caller.Role != ProjectRole.Tester && caller.Role != ProjectRole.Lead))
        {
            return new Result<int>(ErrorType.Forbidden, "Only testers can take tickets for check");
        }

        if (ticket.Status != TicketStatus.OnCheck)
        {
            return new Result<int>(ErrorType.Conflict, "Ticket is not ready for check");
        }

        ticket.VerifierId = caller.Id;

        var history = new HistoryEntryEntity
        {
            Id = Guid.NewGuid(),
            Text = $"Тикет взят на проверку тестировщиком: {caller.User.FullName}",
            CreatedAt = DateTime.UtcNow,
            TicketId = ticket.Id,
            AuthorId = caller.Id
        };
        await context.HistoryEntries.AddAsync(history);

        await context.SaveChangesAsync();

        return new Result<int>(1);
    }

    public async Task<Result<int>> Accept(Guid ticketId, AcceptFixRequest request, Guid userId)
    {
        var ticket = await context.Tickets
            .Include(t => t.Reservation)
            .FirstOrDefaultAsync(t => t.Id == ticketId);
        if (ticket == null)
        {
            return new Result<int>(ErrorType.NotFound, "Ticket not found");
        }

        var caller = await context.ProjectMembers
            .FirstOrDefaultAsync(pm => pm.ProjectId == ticket.ProjectId && pm.UserId == userId && pm.Status == MemberStatus.Active);
        if (caller == null || (caller.Role != ProjectRole.Tester && caller.Role != ProjectRole.Lead))
        {
            return new Result<int>(ErrorType.Forbidden, "Only testers can accept fixes");
        }

        if (ticket.VerifierId.HasValue && ticket.VerifierId.Value != caller.Id && caller.Role != ProjectRole.Lead)
        {
            return new Result<int>(ErrorType.Forbidden, "Only assigned verifier or project lead can accept fix");
        }

        if (ticket.Status != TicketStatus.OnCheck)
        {
            return new Result<int>(ErrorType.Conflict, "Ticket is not in OnCheck status");
        }

        var latestFix = await context.Fixes
            .Where(f => f.TicketId == ticket.Id)
            .OrderByDescending(f => f.CreatedAt)
            .FirstOrDefaultAsync();
        if (latestFix == null)
        {
            return new Result<int>(ErrorType.Conflict, "No fix found for ticket");
        }

        latestFix.Decision = FixDecision.Accepted;
        latestFix.CheckComment = request.Comment;

        var release = await context.Releases
            .FirstOrDefaultAsync(r => r.ProjectId == ticket.ProjectId && r.Version == latestFix.ReleaseVersion);
        if (release == null)
        {
            release = new ReleaseEntity
            {
                Id = Guid.NewGuid(),
                ProjectId = ticket.ProjectId,
                Version = latestFix.ReleaseVersion,
                CreatedAt = DateTime.UtcNow
            };
            await context.Releases.AddAsync(release);
        }

        ticket.ReleaseId = release.Id;

        var pendingDeadlines = await context.DeadlineChangeRequests
            .Where(d => d.TicketId == ticket.Id && d.Decision == DeadlineDecision.Pending)
            .ToListAsync();
        foreach (var d in pendingDeadlines)
        {
            d.Decision = DeadlineDecision.Rejected;
        }

        ticket.Status = TicketStatus.Done;

        var history = new HistoryEntryEntity
        {
            Id = Guid.NewGuid(),
            Text = $"Решение принято. Тикет закрыт (релиз {latestFix.ReleaseVersion})",
            CreatedAt = DateTime.UtcNow,
            TicketId = ticket.Id,
            AuthorId = caller.Id
        };
        await context.HistoryEntries.AddAsync(history);

        if (ticket.Reservation != null)
        {
            var dev = await context.ProjectMembers
                .FirstOrDefaultAsync(pm => pm.Id == ticket.Reservation.DeveloperId);
            if (dev != null && dev.UserId != userId)
            {
                await context.Notifications.AddAsync(new NotificationEntity
                {
                    Id = Guid.NewGuid(),
                    Text = $"Решение по тикету #{ticket.Number} принято. Тикет закрыт",
                    CreatedAt = DateTime.UtcNow,
                    isRead = false,
                    UserId = dev.UserId
                });
            }
        }

        await context.SaveChangesAsync();

        return new Result<int>(1);
    }

    public async Task<Result<int>> Return(Guid ticketId, ReturnFixRequest request, Guid userId)
    {
        var ticket = await context.Tickets
            .Include(t => t.Reservation)
            .FirstOrDefaultAsync(t => t.Id == ticketId);
        if (ticket == null)
        {
            return new Result<int>(ErrorType.NotFound, "Ticket not found");
        }

        var caller = await context.ProjectMembers
            .FirstOrDefaultAsync(pm => pm.ProjectId == ticket.ProjectId && pm.UserId == userId && pm.Status == MemberStatus.Active);
        if (caller == null || (caller.Role != ProjectRole.Tester && caller.Role != ProjectRole.Lead))
        {
            return new Result<int>(ErrorType.Forbidden, "Only testers can return fixes");
        }

        if (ticket.VerifierId.HasValue && ticket.VerifierId.Value != caller.Id && caller.Role != ProjectRole.Lead)
        {
            return new Result<int>(ErrorType.Forbidden, "Only assigned verifier or project lead can return fix");
        }

        if (ticket.Status != TicketStatus.OnCheck)
        {
            return new Result<int>(ErrorType.Conflict, "Ticket is not in OnCheck status");
        }

        var latestFix = await context.Fixes
            .Where(f => f.TicketId == ticket.Id)
            .OrderByDescending(f => f.CreatedAt)
            .FirstOrDefaultAsync();
        if (latestFix != null)
        {
            latestFix.Decision = FixDecision.Returned;
            latestFix.CheckComment = request.Comment.Trim();
        }

        var comment = new CommentEntity
        {
            Id = Guid.NewGuid(),
            Text = request.Comment.Trim(),
            CreatedAt = DateTime.UtcNow,
            TicketId = ticket.Id,
            AuthorId = caller.Id
        };
        await context.Comments.AddAsync(comment);

        ticket.Status = TicketStatus.Rework;

        var history = new HistoryEntryEntity
        {
            Id = Guid.NewGuid(),
            Text = $"Решение отклонено. Тикет возвращен на доработку: {request.Comment.Trim()}",
            CreatedAt = DateTime.UtcNow,
            TicketId = ticket.Id,
            AuthorId = caller.Id
        };
        await context.HistoryEntries.AddAsync(history);

        if (ticket.Reservation != null)
        {
            var dev = await context.ProjectMembers
                .FirstOrDefaultAsync(pm => pm.Id == ticket.Reservation.DeveloperId);
            if (dev != null && dev.UserId != userId)
            {
                await context.Notifications.AddAsync(new NotificationEntity
                {
                    Id = Guid.NewGuid(),
                    Text = $"Решение по тикету #{ticket.Number} отклонено: {request.Comment.Trim()}",
                    CreatedAt = DateTime.UtcNow,
                    isRead = false,
                    UserId = dev.UserId
                });
            }
        }

        await context.SaveChangesAsync();

        return new Result<int>(1);
    }

    public async Task<Result<int>> RequestDeadline(Guid ticketId, RequestDeadlineRequest request, Guid userId)
    {
        var ticket = await context.Tickets
            .Include(t => t.Reservation)
            .FirstOrDefaultAsync(t => t.Id == ticketId);
        if (ticket == null)
        {
            return new Result<int>(ErrorType.NotFound, "Ticket not found");
        }

        var caller = await context.ProjectMembers
            .FirstOrDefaultAsync(pm => pm.ProjectId == ticket.ProjectId && pm.UserId == userId && pm.Status == MemberStatus.Active);
        if (caller == null || ticket.Reservation == null || ticket.Reservation.DeveloperId != caller.Id)
        {
            return new Result<int>(ErrorType.Forbidden, "Only assigned developer can request deadline change");
        }

        if (ticket.Status == TicketStatus.Done || ticket.Status == TicketStatus.OnReview || ticket.Status == TicketStatus.InPool)
        {
            return new Result<int>(ErrorType.Conflict, "Cannot request deadline for ticket in current status");
        }

        if (request.NewDate <= ticket.PlannedStart)
        {
            return new Result<int>(ErrorType.BadRequest, "New deadline must be after planned start date");
        }

        var totalRequests = await context.DeadlineChangeRequests
            .CountAsync(d => d.TicketId == ticket.Id);
        if (totalRequests >= 2)
        {
            return new Result<int>(ErrorType.Conflict, "Maximum of 2 deadline change requests allowed per ticket");
        }

        var hasPending = await context.DeadlineChangeRequests
            .AnyAsync(d => d.TicketId == ticket.Id && d.Decision == DeadlineDecision.Pending);
        if (hasPending)
        {
            return new Result<int>(ErrorType.Conflict, "There is already a pending deadline change request");
        }

        var deadlineReq = new DeadlineChangeRequestEntity
        {
            Id = Guid.NewGuid(),
            NewDate = request.NewDate,
            Reason = request.Reason.Trim(),
            CreatedAt = DateTime.UtcNow,
            Decision = DeadlineDecision.Pending,
            TicketId = ticket.Id,
            AuthorId = caller.Id
        };
        await context.DeadlineChangeRequests.AddAsync(deadlineReq);

        var history = new HistoryEntryEntity
        {
            Id = Guid.NewGuid(),
            Text = $"Запрошен перенос дедлайна на {request.NewDate}. Причина: {request.Reason.Trim()}",
            CreatedAt = DateTime.UtcNow,
            TicketId = ticket.Id,
            AuthorId = caller.Id
        };
        await context.HistoryEntries.AddAsync(history);

        var leads = await context.ProjectMembers
            .Where(pm => pm.ProjectId == ticket.ProjectId && pm.Role == ProjectRole.Lead && pm.Status == MemberStatus.Active && pm.UserId != userId)
            .ToListAsync();

        foreach (var lead in leads)
        {
            await context.Notifications.AddAsync(new NotificationEntity
            {
                Id = Guid.NewGuid(),
                Text = $"Запрос переноса дедлайна по тикету #{ticket.Number}",
                CreatedAt = DateTime.UtcNow,
                isRead = false,
                UserId = lead.UserId
            });
        }

        await context.SaveChangesAsync();

        return new Result<int>(1);
    }

    public async Task<Result<int>> ResolveDeadline(Guid ticketId, Guid requestId, ResolveDeadlineRequest request, Guid userId)
    {
        var ticket = await context.Tickets
            .Include(t => t.Reservation)
            .FirstOrDefaultAsync(t => t.Id == ticketId);
        if (ticket == null)
        {
            return new Result<int>(ErrorType.NotFound, "Ticket not found");
        }

        var caller = await context.ProjectMembers
            .FirstOrDefaultAsync(pm => pm.ProjectId == ticket.ProjectId && pm.UserId == userId && pm.Status == MemberStatus.Active);
        if (caller == null || caller.Role != ProjectRole.Lead)
        {
            return new Result<int>(ErrorType.Forbidden, "Only project lead can resolve deadline requests");
        }

        var deadlineReq = await context.DeadlineChangeRequests
            .Include(d => d.Author)
            .FirstOrDefaultAsync(d => d.Id == requestId && d.TicketId == ticketId);
        if (deadlineReq == null)
        {
            return new Result<int>(ErrorType.NotFound, "Deadline request not found");
        }

        if (deadlineReq.Decision != DeadlineDecision.Pending)
        {
            return new Result<int>(ErrorType.Conflict, "Deadline request is already resolved");
        }

        deadlineReq.ResolvedById = caller.Id;
        string historyText;
        string notifyText;

        if (request.Decision == DeadlineDecision.Approved)
        {
            deadlineReq.Decision = DeadlineDecision.Approved;
            ticket.Deadline = deadlineReq.NewDate;
            historyText = $"Запрос на перенос дедлайна одобрен ({ticket.Deadline})";
            notifyText = $"Запрос на перенос дедлайна по тикету #{ticket.Number} одобрен";
        }
        else if (request.Decision == DeadlineDecision.Rejected)
        {
            deadlineReq.Decision = DeadlineDecision.Rejected;
            historyText = "Запрос на перенос дедлайна отклонен";
            notifyText = $"Запрос на перенос дедлайна по тикету #{ticket.Number} отклонен";
        }
        else if (request.Decision == DeadlineDecision.CounterOffered)
        {
            if (request.CounterDate!.Value <= ticket.PlannedStart)
            {
                return new Result<int>(ErrorType.BadRequest, "CounterDate must be after planned start");
            }

            deadlineReq.Decision = DeadlineDecision.CounterOffered;
            deadlineReq.CounterDate = request.CounterDate.Value;
            ticket.Deadline = request.CounterDate.Value;
            historyText = $"Предложен встречный дедлайн: {request.CounterDate.Value}";
            notifyText = $"По тикету #{ticket.Number} установлен встречный дедлайн {request.CounterDate.Value}";
        }
        else
        {
            return new Result<int>(ErrorType.BadRequest, "Invalid decision");
        }

        var history = new HistoryEntryEntity
        {
            Id = Guid.NewGuid(),
            Text = historyText,
            CreatedAt = DateTime.UtcNow,
            TicketId = ticket.Id,
            AuthorId = caller.Id
        };
        await context.HistoryEntries.AddAsync(history);

        if (deadlineReq.Author != null && deadlineReq.Author.UserId != userId)
        {
            await context.Notifications.AddAsync(new NotificationEntity
            {
                Id = Guid.NewGuid(),
                Text = notifyText,
                CreatedAt = DateTime.UtcNow,
                isRead = false,
                UserId = deadlineReq.Author.UserId
            });
        }

        await context.SaveChangesAsync();

        return new Result<int>(1);
    }

    public async Task<Result<CommentResponse>> AddComment(Guid ticketId, AddCommentRequest request, Guid userId)
    {
        var ticket = await context.Tickets.FirstOrDefaultAsync(t => t.Id == ticketId);
        if (ticket == null)
        {
            return new Result<CommentResponse>(ErrorType.NotFound, "Ticket not found");
        }

        var caller = await context.ProjectMembers
            .Include(pm => pm.User)
            .FirstOrDefaultAsync(pm => pm.ProjectId == ticket.ProjectId && pm.UserId == userId && pm.Status == MemberStatus.Active);
        if (caller == null)
        {
            return new Result<CommentResponse>(ErrorType.Forbidden, "Access denied");
        }

        var comment = new CommentEntity
        {
            Id = Guid.NewGuid(),
            Text = request.Text.Trim(),
            CreatedAt = DateTime.UtcNow,
            TicketId = ticket.Id,
            AuthorId = caller.Id
        };
        await context.Comments.AddAsync(comment);

        var history = new HistoryEntryEntity
        {
            Id = Guid.NewGuid(),
            Text = "Добавлен комментарий",
            CreatedAt = DateTime.UtcNow,
            TicketId = ticket.Id,
            AuthorId = caller.Id
        };
        await context.HistoryEntries.AddAsync(history);

        await context.SaveChangesAsync();

        return new Result<CommentResponse>(new CommentResponse(comment.Id, comment.Text, comment.CreatedAt, caller.Id, caller.User.FullName));
    }

    public async Task<Result<AttachmentResponse>> AddAttachment(Guid ticketId, string filePath, long size, AttachmentType type, Guid userId)
    {
        var ticket = await context.Tickets.FirstOrDefaultAsync(t => t.Id == ticketId);
        if (ticket == null)
        {
            return new Result<AttachmentResponse>(ErrorType.NotFound, "Ticket not found");
        }

        var caller = await context.ProjectMembers
            .Include(pm => pm.User)
            .FirstOrDefaultAsync(pm => pm.ProjectId == ticket.ProjectId && pm.UserId == userId && pm.Status == MemberStatus.Active);
        if (caller == null)
        {
            return new Result<AttachmentResponse>(ErrorType.Forbidden, "Access denied");
        }

        var attachment = new AttachmentEntity
        {
            Id = Guid.NewGuid(),
            FilePath = filePath.Trim(),
            Size = size,
            Type = type,
            TicketId = ticket.Id,
            UploadedById = caller.Id
        };
        await context.Attachments.AddAsync(attachment);

        var history = new HistoryEntryEntity
        {
            Id = Guid.NewGuid(),
            Text = $"Добавлено вложение: {filePath.Trim()}",
            CreatedAt = DateTime.UtcNow,
            TicketId = ticket.Id,
            AuthorId = caller.Id
        };
        await context.HistoryEntries.AddAsync(history);

        await context.SaveChangesAsync();

        return new Result<AttachmentResponse>(new AttachmentResponse(attachment.Id, attachment.FilePath, attachment.Size, attachment.Type, caller.Id, caller.User.FullName));
    }

    public async Task<Result<TicketDetailsResponse>> GetById(Guid ticketId, Guid userId)
    {
        var ticket = await context.Tickets
            .Include(t => t.Project)
            .Include(t => t.Release)
            .Include(t => t.Author).ThenInclude(a => a.User)
            .Include(t => t.Verifier).ThenInclude(v => v!.User)
            .Include(t => t.Reservation).ThenInclude(r => r!.Developer).ThenInclude(d => d.User)
            .Include(t => t.Comments).ThenInclude(c => c.Author).ThenInclude(a => a.User)
            .Include(t => t.HistoryEntries).ThenInclude(h => h.Author).ThenInclude(a => a.User)
            .Include(t => t.Fixes).ThenInclude(f => f.Author).ThenInclude(a => a.User)
            .Include(t => t.DeadlineChangeRequests).ThenInclude(d => d.Author).ThenInclude(a => a.User)
            .Include(t => t.DeadlineChangeRequests).ThenInclude(d => d.ResolvedBy).ThenInclude(r => r!.User)
            .Include(t => t.Attachments).ThenInclude(at => at.UploadedBy).ThenInclude(u => u.User)
            .FirstOrDefaultAsync(t => t.Id == ticketId);

        if (ticket == null)
        {
            return new Result<TicketDetailsResponse>(ErrorType.NotFound, "Ticket not found");
        }

        var isMember = await context.ProjectMembers
            .AnyAsync(pm => pm.ProjectId == ticket.ProjectId && pm.UserId == userId && pm.Status == MemberStatus.Active);
        if (!isMember)
        {
            return new Result<TicketDetailsResponse>(ErrorType.Forbidden, "Access denied");
        }

        var comments = ticket.Comments
            .OrderBy(c => c.CreatedAt)
            .Select(c => new CommentResponse(c.Id, c.Text, c.CreatedAt, c.AuthorId, c.Author.User.FullName))
            .ToList();

        var history = ticket.HistoryEntries
            .OrderBy(h => h.CreatedAt)
            .Select(h => new HistoryEntryResponse(h.Id, h.Text, h.CreatedAt, h.AuthorId, h.Author.User.FullName))
            .ToList();

        var fixes = ticket.Fixes
            .OrderBy(f => f.CreatedAt)
            .Select(f => new FixResponse(f.Id, f.Result, f.CommitLinks, f.ReleaseVersion, f.CreatedAt, f.CheckComment, f.Decision, f.AuthorId, f.Author.User.FullName))
            .ToList();

        var deadlineRequests = ticket.DeadlineChangeRequests
            .OrderBy(d => d.CreatedAt)
            .Select(d => new DeadlineChangeRequestResponse(d.Id, d.NewDate, d.Reason, d.CounterDate, d.CreatedAt, d.Decision, d.AuthorId, d.Author.User.FullName, d.ResolvedById, d.ResolvedBy != null ? d.ResolvedBy.User.FullName : null))
            .ToList();

        var attachments = ticket.Attachments
            .OrderBy(a => a.Id)
            .Select(a => new AttachmentResponse(a.Id, a.FilePath, a.Size, a.Type, a.UploadedById, a.UploadedBy.User.FullName))
            .ToList();

        var response = new TicketDetailsResponse(
            ticket.Id,
            ticket.Number,
            $"{ticket.Project.Code}-{ticket.Number}",
            ticket.Title,
            ticket.Description,
            ticket.Steps,
            ticket.ExpectedResult,
            ticket.ActualResult,
            ticket.Priority,
            ticket.Status,
            ticket.PlannedStart,
            ticket.Deadline,
            ticket.CreatedAt,
            ticket.ProjectId,
            ticket.Project.Code,
            ticket.Project.Name,
            ticket.ReleaseId,
            ticket.Release != null ? ticket.Release.Version : null,
            ticket.AuthorId,
            ticket.Author.User.FullName,
            ticket.VerifierId,
            ticket.Verifier != null ? ticket.Verifier.User.FullName : null,
            ticket.Reservation != null ? (Guid?)ticket.Reservation.DeveloperId : null,
            ticket.Reservation != null ? ticket.Reservation.Developer.User.FullName : null,
            ticket.Reservation != null ? (DateTime?)ticket.Reservation.TakenAt : null,
            comments,
            history,
            fixes,
            deadlineRequests,
            attachments
        );

        return new Result<TicketDetailsResponse>(response);
    }

    public async Task<Result<List<TicketShortResponse>>> GetProjectTickets(Guid projectId, TicketStatus? status, Priority? priority, Guid? assigneeId, string? search, Guid userId)
    {
        var isMember = await context.ProjectMembers
            .AnyAsync(pm => pm.ProjectId == projectId && pm.UserId == userId && pm.Status == MemberStatus.Active);
        if (!isMember)
        {
            return new Result<List<TicketShortResponse>>(ErrorType.Forbidden, "Access denied");
        }

        var query = context.Tickets
            .Include(t => t.Project)
            .Include(t => t.Author).ThenInclude(a => a.User)
            .Include(t => t.Verifier).ThenInclude(v => v!.User)
            .Include(t => t.Reservation).ThenInclude(r => r!.Developer).ThenInclude(d => d.User)
            .Include(t => t.Comments)
            .Include(t => t.Attachments)
            .Where(t => t.ProjectId == projectId);

        if (status.HasValue)
        {
            query = query.Where(t => t.Status == status.Value);
        }

        if (priority.HasValue)
        {
            query = query.Where(t => t.Priority == priority.Value);
        }

        if (assigneeId.HasValue)
        {
            query = query.Where(t => t.Reservation != null && (t.Reservation.DeveloperId == assigneeId.Value || t.Reservation.Developer.UserId == assigneeId.Value));
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(t => t.Title.ToLower().Contains(s) || t.Number.ToString().Contains(s));
        }

        var list = await query
            .OrderByDescending(t => t.Number)
            .Select(t => new TicketShortResponse(
                t.Id,
                t.Number,
                $"{t.Project.Code}-{t.Number}",
                t.Title,
                t.Status,
                t.Priority,
                t.PlannedStart,
                t.Deadline,
                t.CreatedAt,
                t.ProjectId,
                t.Project.Code,
                t.Reservation != null ? (Guid?)t.Reservation.DeveloperId : null,
                t.Reservation != null ? t.Reservation.Developer.User.FullName : null,
                t.AuthorId,
                t.Author.User.FullName,
                t.VerifierId,
                t.Verifier != null ? t.Verifier.User.FullName : null,
                t.Comments.Count,
                t.Attachments.Count
            ))
            .ToListAsync();

        return new Result<List<TicketShortResponse>>(list);
    }

    public async Task<Result<List<TicketShortResponse>>> GetMyTickets(Guid userId)
    {
        var memberIds = await context.ProjectMembers
            .Where(pm => pm.UserId == userId && pm.Status == MemberStatus.Active)
            .Select(pm => pm.Id)
            .ToListAsync();

        if (memberIds.Count == 0)
        {
            return new Result<List<TicketShortResponse>>(new List<TicketShortResponse>());
        }

        var query = context.Tickets
            .Include(t => t.Project)
            .Include(t => t.Author).ThenInclude(a => a.User)
            .Include(t => t.Verifier).ThenInclude(v => v!.User)
            .Include(t => t.Reservation).ThenInclude(r => r!.Developer).ThenInclude(d => d.User)
            .Include(t => t.Comments)
            .Include(t => t.Attachments)
            .Where(t => memberIds.Contains(t.AuthorId)
                     || (t.VerifierId.HasValue && memberIds.Contains(t.VerifierId.Value))
                     || (t.Reservation != null && memberIds.Contains(t.Reservation.DeveloperId)));

        var list = await query
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new TicketShortResponse(
                t.Id,
                t.Number,
                $"{t.Project.Code}-{t.Number}",
                t.Title,
                t.Status,
                t.Priority,
                t.PlannedStart,
                t.Deadline,
                t.CreatedAt,
                t.ProjectId,
                t.Project.Code,
                t.Reservation != null ? (Guid?)t.Reservation.DeveloperId : null,
                t.Reservation != null ? t.Reservation.Developer.User.FullName : null,
                t.AuthorId,
                t.Author.User.FullName,
                t.VerifierId,
                t.Verifier != null ? t.Verifier.User.FullName : null,
                t.Comments.Count,
                t.Attachments.Count
            ))
            .ToListAsync();

        return new Result<List<TicketShortResponse>>(list);
    }
}
