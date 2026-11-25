public interface IUserService
{
    Task<Result<string>> Register(CreateUserRequest request);
    Task<Result<string>> Login(LoginUserRequest request);
    Task<UserEntity?> GetByEmail(string email);
    Task<Result<UserResponse>> GetMe(Guid userId);
}