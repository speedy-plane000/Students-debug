using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
[ApiController]
[Route("notifications")]
public class NotificationController : ControllerBase
{
    private readonly INotificationService notificationService;

    public NotificationController(INotificationService _notificationService)
    {
        notificationService = _notificationService;
    }

    [HttpGet]
    public async Task<ActionResult<List<NotificationResponse>>> GetNotifications()
    {
        var userId = GetUserId();
        var result = await notificationService.GetUserNotifications(userId);
        if (!result.IsSuccess)
        {
            return BadRequest(result.TextError);
        }
        return Ok(result.Value);
    }

    [HttpPut("{notificationId}/read")]
    public async Task<IActionResult> MarkRead([FromRoute] Guid notificationId)
    {
        var userId = GetUserId();
        var result = await notificationService.MarkRead(notificationId, userId);
        if (!result.IsSuccess)
        {
            return result.ErrorType switch
            {
                ErrorType.NotFound => NotFound(result.TextError),
                _ => BadRequest(result.TextError)
            };
        }
        return Ok();
    }

    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllRead()
    {
        var userId = GetUserId();
        var result = await notificationService.MarkAllRead(userId);
        if (!result.IsSuccess)
        {
            return BadRequest(result.TextError);
        }
        return Ok();
    }

    private Guid GetUserId()
    {
        var claim = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
        return Guid.TryParse(claim, out var userId) ? userId : Guid.Empty;
    }
}
