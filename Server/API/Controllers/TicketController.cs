using Core.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
[ApiController]
[Route("tickets")]
public class TicketController : ControllerBase
{
    private readonly ITicketService ticketService;

    public TicketController(ITicketService _ticketService)
    {
        ticketService = _ticketService;
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Create([FromBody] CreateTicketRequest request)
    {
        var userId = GetUserId();
        var result = await ticketService.Create(userId, request);
        if (!result.IsSuccess)
        {
            return HandleError(result);
        }
        return Ok(result.Value);
    }

    [HttpGet("{ticketId}")]
    public async Task<ActionResult<TicketDetailsResponse>> GetById([FromRoute] Guid ticketId)
    {
        var userId = GetUserId();
        var result = await ticketService.GetById(ticketId, userId);
        if (!result.IsSuccess)
        {
            return HandleError(result);
        }
        return Ok(result.Value);
    }

    [HttpPut("{ticketId}")]
    public async Task<IActionResult> Edit(
        [FromRoute] Guid ticketId,
        [FromBody] UpdateTicketRequest request)
    {
        var userId = GetUserId();
        var result = await ticketService.Edit(ticketId, request, userId);
        if (!result.IsSuccess)
        {
            return HandleError(result);
        }
        return Ok();
    }

    [HttpDelete("{ticketId}")]
    public async Task<IActionResult> Delete([FromRoute] Guid ticketId)
    {
        var userId = GetUserId();
        var result = await ticketService.Delete(ticketId, userId);
        if (!result.IsSuccess)
        {
            return HandleError(result);
        }
        return Ok();
    }

    [HttpPost("{ticketId}/assign")]
    public async Task<IActionResult> Assign(
        [FromRoute] Guid ticketId,
        [FromBody] AssignTicketRequest request)
    {
        var userId = GetUserId();
        var result = await ticketService.Assign(ticketId, request, userId);
        if (!result.IsSuccess)
        {
            return HandleError(result);
        }
        return Ok();
    }

    [HttpPost("{ticketId}/pool")]
    public async Task<IActionResult> SendToPool([FromRoute] Guid ticketId)
    {
        var userId = GetUserId();
        var result = await ticketService.SendToPool(ticketId, userId);
        if (!result.IsSuccess)
        {
            return HandleError(result);
        }
        return Ok();
    }

    [HttpPost("{ticketId}/take")]
    public async Task<IActionResult> Take([FromRoute] Guid ticketId)
    {
        var userId = GetUserId();
        var result = await ticketService.Take(ticketId, userId);
        if (!result.IsSuccess)
        {
            return HandleError(result);
        }
        return Ok();
    }

    [HttpPost("{ticketId}/start")]
    public async Task<IActionResult> StartWork([FromRoute] Guid ticketId)
    {
        var userId = GetUserId();
        var result = await ticketService.StartWork(ticketId, userId);
        if (!result.IsSuccess)
        {
            return HandleError(result);
        }
        return Ok();
    }

    [HttpPost("{ticketId}/pause")]
    public async Task<IActionResult> Pause([FromRoute] Guid ticketId)
    {
        var userId = GetUserId();
        var result = await ticketService.Pause(ticketId, userId);
        if (!result.IsSuccess)
        {
            return HandleError(result);
        }
        return Ok();
    }

    [HttpPost("{ticketId}/fix")]
    public async Task<IActionResult> SubmitFix(
        [FromRoute] Guid ticketId,
        [FromBody] SubmitFixRequest request)
    {
        var userId = GetUserId();
        var result = await ticketService.SubmitFix(ticketId, request, userId);
        if (!result.IsSuccess)
        {
            return HandleError(result);
        }
        return Ok();
    }

    [HttpPost("{ticketId}/check")]
    public async Task<IActionResult> TakeForCheck([FromRoute] Guid ticketId)
    {
        var userId = GetUserId();
        var result = await ticketService.TakeForCheck(ticketId, userId);
        if (!result.IsSuccess)
        {
            return HandleError(result);
        }
        return Ok();
    }

    [HttpPost("{ticketId}/accept")]
    public async Task<IActionResult> Accept(
        [FromRoute] Guid ticketId,
        [FromBody] AcceptFixRequest request)
    {
        var userId = GetUserId();
        var result = await ticketService.Accept(ticketId, request, userId);
        if (!result.IsSuccess)
        {
            return HandleError(result);
        }
        return Ok();
    }

    [HttpPost("{ticketId}/return")]
    public async Task<IActionResult> Return(
        [FromRoute] Guid ticketId,
        [FromBody] ReturnFixRequest request)
    {
        var userId = GetUserId();
        var result = await ticketService.Return(ticketId, request, userId);
        if (!result.IsSuccess)
        {
            return HandleError(result);
        }
        return Ok();
    }

    [HttpPost("{ticketId}/deadline")]
    public async Task<IActionResult> RequestDeadline(
        [FromRoute] Guid ticketId,
        [FromBody] RequestDeadlineRequest request)
    {
        var userId = GetUserId();
        var result = await ticketService.RequestDeadline(ticketId, request, userId);
        if (!result.IsSuccess)
        {
            return HandleError(result);
        }
        return Ok();
    }

    [HttpPut("{ticketId}/deadline/{requestId}")]
    public async Task<IActionResult> ResolveDeadline(
        [FromRoute] Guid ticketId,
        [FromRoute] Guid requestId,
        [FromBody] ResolveDeadlineRequest request)
    {
        var userId = GetUserId();
        var result = await ticketService.ResolveDeadline(ticketId, requestId, request, userId);
        if (!result.IsSuccess)
        {
            return HandleError(result);
        }
        return Ok();
    }

    [HttpPost("{ticketId}/comments")]
    public async Task<ActionResult<CommentResponse>> AddComment(
        [FromRoute] Guid ticketId,
        [FromBody] AddCommentRequest request)
    {
        var userId = GetUserId();
        var result = await ticketService.AddComment(ticketId, request, userId);
        if (!result.IsSuccess)
        {
            return HandleError(result);
        }
        return Ok(result.Value);
    }

    [HttpPost("{ticketId}/attachments")]
    public async Task<ActionResult<AttachmentResponse>> AddAttachment([FromRoute] Guid ticketId)
    {
        var userId = GetUserId();
        if (Request.HasFormContentType)
        {
            var file = Request.Form.Files.FirstOrDefault();
            if (file == null || file.Length == 0)
            {
                return BadRequest("File is empty");
            }
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            var type = ext switch
            {
                ".png" or ".jpg" or ".jpeg" or ".gif" or ".webp" or ".svg" => AttachmentType.Image,
                ".mp4" or ".mov" or ".avi" or ".mkv" or ".webm" => AttachmentType.Video,
                _ => AttachmentType.Document
            };
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }
            var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            var result = await ticketService.AddAttachment(ticketId, file.FileName, file.Length, type, userId);
            if (!result.IsSuccess)
            {
                return HandleError(result);
            }
            return Ok(result.Value);
        }
        else
        {
            var req = await Request.ReadFromJsonAsync<AddAttachmentRequest>();
            if (req == null)
            {
                return BadRequest("Invalid attachment request");
            }
            var result = await ticketService.AddAttachment(ticketId, req.FilePath, req.Size, req.Type, userId);
            if (!result.IsSuccess)
            {
                return HandleError(result);
            }
            return Ok(result.Value);
        }
    }

    [HttpGet("project/{projectId}")]
    public async Task<ActionResult<List<TicketShortResponse>>> GetProjectTickets(
        [FromRoute] Guid projectId,
        [FromQuery] TicketStatus? status,
        [FromQuery] Priority? priority,
        [FromQuery] Guid? assigneeId,
        [FromQuery] string? search)
    {
        var userId = GetUserId();
        var result = await ticketService.GetProjectTickets(projectId, status, priority, assigneeId, search, userId);
        if (!result.IsSuccess)
        {
            return HandleError(result);
        }
        return Ok(result.Value);
    }

    [HttpGet("my")]
    public async Task<ActionResult<List<TicketShortResponse>>> GetMyTickets()
    {
        var userId = GetUserId();
        var result = await ticketService.GetMyTickets(userId);
        if (!result.IsSuccess)
        {
            return HandleError(result);
        }
        return Ok(result.Value);
    }

    private Guid GetUserId()
    {
        var claim = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
        return Guid.TryParse(claim, out var userId) ? userId : Guid.Empty;
    }

    private ActionResult HandleError<T>(Result<T> result)
    {
        return result.ErrorType switch
        {
            ErrorType.NotFound => NotFound(result.TextError),
            ErrorType.Forbidden => StatusCode(403, result.TextError),
            ErrorType.Conflict => StatusCode(409, result.TextError),
            _ => BadRequest(result.TextError)
        };
    }
}
