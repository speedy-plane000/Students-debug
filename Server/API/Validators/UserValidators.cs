using FluentValidation;

namespace API.Validators;

public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(u => u.FullName)
            .NotEmpty().WithMessage("Full name is required")
            .MaximumLength(100).WithMessage("Full name cannot exceed 100 characters");

        RuleFor(u => u.Password)
            .NotEmpty().WithMessage("must be at least 8 characters")
            .MinimumLength(8).WithMessage("must be at least 8 characters")
            .Matches(@"[a-zA-Z]").WithMessage("must be at least 8 characters")
            .Matches(@"[0-9]").WithMessage("must be at least 8 characters");

        RuleFor(u => u.Email)
            .NotEmpty().WithMessage("invalid email format")
            .MaximumLength(254).WithMessage("invalid email format")
            .Matches(@"^[a-zA-Z0-9_+!#&'*/=?^~{}|`%$-][0-9a-zA-Z._+!#&'*/=?^~{}|`$%-]{0,63}@(?!.*(?:-\.|\.-))([a-zA-Z0-9][a-zA-Z0-9-]{0,62}\.)+[a-zA-Z]{2,63}$")
            .WithMessage("invalid email format")
            .Must(email => email != null && !email.Contains("..") && !email.Contains(".@"))
            .WithMessage("invalid email format");
    }
}

public class LoginUserRequestValidator : AbstractValidator<LoginUserRequest>
{
    public LoginUserRequestValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(u => u.Email)
            .NotEmpty().WithMessage("invalid email format")
            .MaximumLength(254).WithMessage("invalid email format")
            .Matches(@"^[a-zA-Z0-9_+!#&'*/=?^~{}|`%$-][0-9a-zA-Z._+!#&'*/=?^~{}|`$%-]{0,63}@(?!.*(?:-\.|\.-))([a-zA-Z0-9][a-zA-Z0-9-]{0,62}\.)+[a-zA-Z]{2,63}$")
            .WithMessage("invalid email format")
            .Must(email => email != null && !email.Contains("..") && !email.Contains(".@"))
            .WithMessage("invalid email format");

        RuleFor(u => u.Password)
            .NotEmpty().WithMessage("Password is required");
    }
}
