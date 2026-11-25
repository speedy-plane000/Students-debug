
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("users")]
public class UserController : ControllerBase
{
    private readonly IUserService userService;
    public UserController(IUserService _userService)
    {
        userService = _userService;
    }
    [HttpPost("Register")]
    public async Task<IActionResult> Register(
        [FromBody]CreateUserRequest request)
    {
        var result = await userService.Register(request);
        if (!result.IsSuccess)
        {
            return result.ErrorType switch
            {
                ErrorType.Conflict => StatusCode(409, result.TextError),
                _ => BadRequest(result.TextError)
            };
        }

        SetTokenCookie(result.Value!);
        return Ok();
    }
    [HttpPost("Login")]
    public async Task<IActionResult> Login(
        [FromBody]LoginUserRequest request)
    {
        var result = await userService.Login(request);
        if (!result.IsSuccess)
        {
            return result.ErrorType switch
            {
                ErrorType.Conflict => StatusCode(409, result.TextError),
                ErrorType.NotFound => NotFound(result.TextError),
                _ => BadRequest(result.TextError)
            };
        }

        SetTokenCookie(result.Value!);
        return Ok();
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserResponse>> GetMe()
    {
        var userId = GetUserId();
        var result = await userService.GetMe(userId);
        if (!result.IsSuccess)
        {
            return NotFound(result.TextError);
        }
        return Ok(result.Value);
    }

    [HttpPost("Logout")]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("tasty-cookies", new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Lax
        });
        return Ok();
    }

    private Guid GetUserId()
    {
        var claim = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
        return Guid.TryParse(claim, out var userId) ? userId : Guid.Empty;
    }
    private void SetTokenCookie(string token)
    {
        Response.Cookies.Append("tasty-cookies", token, new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,                 
            SameSite = SameSiteMode.Lax,   
            Expires = DateTimeOffset.UtcNow.AddHours(12) 
        });
    }
}