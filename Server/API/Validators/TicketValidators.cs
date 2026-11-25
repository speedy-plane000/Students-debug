using Core.Enums;
using FluentValidation;

namespace API.Validators;

public class CreateTicketRequestValidator : AbstractValidator<CreateTicketRequest>
{
    public CreateTicketRequestValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(t => t.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title cannot exceed 200 characters");

        RuleFor(t => t.Description)
            .NotEmpty().WithMessage("Description is required");

        RuleFor(t => t.Steps)
            .NotEmpty().WithMessage("Steps are required");

        RuleFor(t => t.Priority)
            .IsInEnum().WithMessage("Invalid priority");

        RuleFor(t => t.PlannedStart)
            .NotEmpty().WithMessage("Planned start is required");

        RuleFor(t => t.Deadline)
            .NotEmpty().WithMessage("Deadline is required")
            .GreaterThanOrEqualTo(t => t.PlannedStart).WithMessage("Deadline cannot be earlier than PlannedStart");

        RuleFor(t => t.ProjectId)
            .NotEmpty().WithMessage("ProjectId is required");
    }
}

public class UpdateTicketRequestValidator : AbstractValidator<UpdateTicketRequest>
{
    public UpdateTicketRequestValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(t => t.Title)
            .MaximumLength(200).WithMessage("Title cannot exceed 200 characters")
            .When(t => !string.IsNullOrEmpty(t.Title));

        RuleFor(t => t.Priority)
            .IsInEnum().WithMessage("Invalid priority")
            .When(t => t.Priority.HasValue);

        RuleFor(t => t.Deadline)
            .GreaterThanOrEqualTo(t => t.PlannedStart!.Value).WithMessage("Deadline cannot be earlier than PlannedStart")
            .When(t => t.PlannedStart.HasValue && t.Deadline.HasValue);

        RuleFor(t => t.ProjectId)
            .NotEmpty().WithMessage("ProjectId cannot be empty")
            .When(t => t.ProjectId.HasValue);
    }
}

public class AssignTicketRequestValidator : AbstractValidator<AssignTicketRequest>
{
    public AssignTicketRequestValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(t => t.DeveloperId)
            .NotEmpty().WithMessage("DeveloperId is required");
    }
}

public class SubmitFixRequestValidator : AbstractValidator<SubmitFixRequest>
{
    public SubmitFixRequestValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(t => t.Result)
            .NotEmpty().WithMessage("Result is required");

        RuleFor(t => t.CommitLinks)
            .NotEmpty().WithMessage("At least one commit link is required")
            .Must(links => links != null && links.Count > 0 && links.Any(l => !string.IsNullOrWhiteSpace(l)))
            .WithMessage("At least one non-empty commit link is required");

        RuleFor(t => t.ReleaseVersion)
            .NotEmpty().WithMessage("Release version is required");
    }
}

public class AcceptFixRequestValidator : AbstractValidator<AcceptFixRequest>
{
    public AcceptFixRequestValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(t => t.Comment)
            .MaximumLength(1000).WithMessage("Comment cannot exceed 1000 characters")
            .When(t => !string.IsNullOrEmpty(t.Comment));
    }
}

public class ReturnFixRequestValidator : AbstractValidator<ReturnFixRequest>
{
    public ReturnFixRequestValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(t => t.Comment)
            .NotEmpty().WithMessage("Comment is required when returning a fix")
            .MaximumLength(1000).WithMessage("Comment cannot exceed 1000 characters");
    }
}

public class RequestDeadlineRequestValidator : AbstractValidator<RequestDeadlineRequest>
{
    public RequestDeadlineRequestValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(t => t.NewDate)
            .NotEmpty().WithMessage("New date is required");

        RuleFor(t => t.Reason)
            .NotEmpty().WithMessage("Reason is required")
            .MaximumLength(1000).WithMessage("Reason cannot exceed 1000 characters");
    }
}

public class ResolveDeadlineRequestValidator : AbstractValidator<ResolveDeadlineRequest>
{
    public ResolveDeadlineRequestValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(t => t.Decision)
            .IsInEnum().WithMessage("Invalid decision");

        RuleFor(t => t.CounterDate)
            .NotNull().WithMessage("CounterDate is required for counter offer")
            .NotEmpty().WithMessage("CounterDate is required for counter offer")
            .When(t => t.Decision == DeadlineDecision.CounterOffered);
    }
}

public class AddCommentRequestValidator : AbstractValidator<AddCommentRequest>
{
    public AddCommentRequestValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(t => t.Text)
            .NotEmpty().WithMessage("Comment text is required")
            .MaximumLength(2000).WithMessage("Comment cannot exceed 2000 characters");
    }
}

public class AddAttachmentRequestValidator : AbstractValidator<AddAttachmentRequest>
{
    public AddAttachmentRequestValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(t => t.FilePath)
            .NotEmpty().WithMessage("FilePath is required");

        RuleFor(t => t.Size)
            .GreaterThan(0).WithMessage("Size must be greater than 0");

        RuleFor(t => t.Type)
            .IsInEnum().WithMessage("Invalid attachment type");
    }
}
