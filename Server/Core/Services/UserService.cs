
using Microsoft.EntityFrameworkCore;

public class UserService : IUserService
{
    private readonly IDebugDbContext context;
    private readonly IPasswordHasher passwordHasher;
    private readonly IJwtProvider jwtProvider;
    public UserService(IDebugDbContext _context
    ,IPasswordHasher _passwordHasher,IJwtProvider _jwtProvider)
    {
        context = _context;
        passwordHasher = _passwordHasher;
        jwtProvider = _jwtProvider;
    }
    public async Task<Result<string>> Register(CreateUserRequest request)
    {
        var isExistEmail = await context.Users.AnyAsync(u => u.Email == request.Email);
        if(isExistEmail == true)
        {
            return new Result<string>(ErrorType.Conflict,"already exist");
        }

        var user = new UserEntity
        {
            Id = Guid.NewGuid(),
            FullName = request.FullName,
            PasswordHash = passwordHasher.Generate(request.Password),
            Email = request.Email
        };
        
        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();

        var token = jwtProvider.GenerateToken(user);
        return new Result<string>(token);
    }
    public async Task<Result<string>> Login(LoginUserRequest request)
    {
        var user = await GetByEmail(request.Email);
        
        if(user == null)
        {
            return new Result<string>(ErrorType.NotFound,"Пользователя не существует");
        }

        if (!passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            return new Result<string>(ErrorType.BadRequest,"Почта или пароль введены неверно");
        }

        var token = jwtProvider.GenerateToken(user);

        return new Result<string>(token);
    }
    public async Task<UserEntity?> GetByEmail(string email)
    {   
        return await context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }
    public async Task<Result<UserResponse>> GetMe(Guid userId)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
        {
            return new Result<UserResponse>(ErrorType.NotFound, "User not found");
        }
        return new Result<UserResponse>(new UserResponse(user.Id, user.FullName, user.Email));
    }
}