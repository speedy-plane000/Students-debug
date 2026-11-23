
public record CreateUserRequest(
    string FullName,
    string Password,
    string Email
);

public record LoginUserRequest(
    string FullName,
    string Password,
    string Email
);