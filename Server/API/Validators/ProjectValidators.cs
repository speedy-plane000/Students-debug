using Core.Enums;
using FluentValidation;

namespace API.Validators;

public class CreateProjectRequestValidator : AbstractValidator<CreateProjectRequest>
{
    public CreateProjectRequestValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(p => p.Code)
            .NotEmpty().WithMessage("Code is required")
            .MaximumLength(10).WithMessage("Code cannot exceed 10 characters")
            .Matches(@"^[A-Z0-9_-]+$").WithMessage("Code must contain only uppercase alphanumeric characters, hyphens or underscores");

        RuleFor(p => p.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(100).WithMessage("Name cannot exceed 100 characters");

        RuleFor(p => p.RepositoryUrl)
            .NotEmpty().WithMessage("Repository URL is required")
            .MaximumLength(500).WithMessage("Repository URL cannot exceed 500 characters");

        RuleFor(p => p.Description)
            .MaximumLength(2000).WithMessage("Description cannot exceed 2000 characters")
            .When(p => !string.IsNullOrEmpty(p.Description));
    }
}

public class UpdateProjectRequestValidator : AbstractValidator<UpdateProjectRequest>
{
    public UpdateProjectRequestValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(p => p.Code)
            .MaximumLength(10).WithMessage("Code cannot exceed 10 characters")
            .Matches(@"^[A-Z0-9_-]+$").WithMessage("Code must contain only uppercase alphanumeric characters, hyphens or underscores")
            .When(p => !string.IsNullOrEmpty(p.Code));

        RuleFor(p => p.Name)
            .MaximumLength(100).WithMessage("Name cannot exceed 100 characters")
            .When(p => !string.IsNullOrEmpty(p.Name));

        RuleFor(p => p.RepositoryUrl)
            .MaximumLength(500).WithMessage("Repository URL cannot exceed 500 characters")
            .When(p => !string.IsNullOrEmpty(p.RepositoryUrl));

        RuleFor(p => p.Description)
            .MaximumLength(2000).WithMessage("Description cannot exceed 2000 characters")
            .When(p => !string.IsNullOrEmpty(p.Description));
    }
}

public class InviteMemberRequestValidator : AbstractValidator<InviteMemberRequest>
{
    public InviteMemberRequestValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(p => p.Email)
            .NotEmpty().WithMessage("Email is required")
            .MaximumLength(254).WithMessage("invalid email format")
            .Matches(@"^[a-zA-Z0-9_+!#&'*/=?^~{}|`%$-][0-9a-zA-Z._+!#&'*/=?^~{}|`$%-]{0,63}@(?!.*(?:-\.|\.-))([a-zA-Z0-9][a-zA-Z0-9-]{0,62}\.)+[a-zA-Z]{2,63}$")
            .WithMessage("invalid email format")
            .Must(email => email != null && !email.Contains("..") && !email.Contains(".@"))
            .WithMessage("invalid email format");
    }
}

public class JoinProjectRequestValidator : AbstractValidator<JoinProjectRequest>
{
    public JoinProjectRequestValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(p => p.Code)
            .NotEmpty().WithMessage("Invite code is required")
            .MaximumLength(20).WithMessage("Invite code is invalid");
    }
}

public class SetRoleRequestValidator : AbstractValidator<SetRoleRequest>
{
    public SetRoleRequestValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(p => p.Role)
            .IsInEnum().WithMessage("Invalid role");
    }
}

public class DeleteProjectRequestValidator : AbstractValidator<DeleteProjectRequest>
{
    public DeleteProjectRequestValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(p => p.ConfirmName)
            .NotEmpty().WithMessage("Project name confirmation is required");
    }
}
