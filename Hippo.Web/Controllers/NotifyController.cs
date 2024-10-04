
using System.Text.Json;
using Hippo.Core.Data;
using Hippo.Core.Domain;
using Hippo.Core.Models;
using Hippo.Core.Models.Email;
using Hippo.Core.Services;
using Hippo.Web.Handlers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NSwag.Annotations;
using Serilog;


namespace Hippo.Web.Controllers;

[ApiKey]
[ApiController]
[Route("api/[controller]")]
[SwaggerResponse(400, typeof(void), Description = "Bad Request")]
[SwaggerResponse(401, typeof(void), Description = "Unauthorized")]
public class NotifyController : Controller
{
    private readonly INotificationService _notificationService;
    private readonly IEmailService _emailService;

    public NotifyController(INotificationService notificationService, IEmailService emailService)
    {
        _notificationService = notificationService;
        _emailService = emailService;
    }

    /// <summary>
    /// Sends an email as supplied
    /// </summary>
    [HttpPost("Raw")]
    [SwaggerResponse(200, typeof(void), Description = "Success")]
    public async Task<ActionResult> Raw([FromBody] EmailModel model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            await _emailService.SendEmail(model);
        }
        catch (Exception ex)
        {
            Log.Error("Error sending email", ex);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                "Failed to send email. The error has been logged for review");
        }

        return Ok();
    }

    /// <summary>
    /// Sends an email styled to be consistent with other Hippo emails
    /// </summary>
    [HttpPost("Styled")]
    [SwaggerResponse(200, typeof(void), Description = "Success")]
    public async Task<ActionResult> Styled([FromBody] SimpleNotificationModel model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var success = await _notificationService.SimpleNotification(model);
        if (!success)
        {
            // error has been logged by _notificationService
            return StatusCode(StatusCodes.Status500InternalServerError, 
                "Failed to send email. The error has been logged for review");
        }

        return Ok();
    }
}