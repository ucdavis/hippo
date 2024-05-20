
using System.Text.Json;
using Hippo.Core.Data;
using Hippo.Core.Domain;
using Hippo.Core.Models;
using Hippo.Core.Services;
using Hippo.Web.Handlers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NSwag.Annotations;


namespace Hippo.Web.Controllers;

[ApiKey]
[ApiController]
[Route("api/[controller]")]
[SwaggerResponse(400, typeof(void), Description = "Bad Request")]
[SwaggerResponse(401, typeof(void), Description = "Unauthorized")]
public class EventQueueController : Controller
{
    private readonly AppDbContext _dbContext;
    private readonly IAccountUpdateService _accountUpdateService;

    public EventQueueController(AppDbContext dbContext, IAccountUpdateService accountUpdateService)
    {
        _dbContext = dbContext;
        _accountUpdateService = accountUpdateService;
    }

    /// <summary>
    /// Retrieves a list of all events having a status of "Pending"
    /// </summary>
    [HttpGet("PendingEvents")]
    [SwaggerResponse(200, typeof(List<QueuedEventModel>), Description = "A list of QueuedEvent objects")]
    public async Task<ActionResult<List<QueuedEventModel>>> PendingEvents()
    {
        var events = await _dbContext.QueuedEvents
            .Where(ae => ae.Status == QueuedEvent.Statuses.Pending)
            .OrderBy(ae => ae.CreatedAt)
            .Select(ae => new QueuedEventModel
            {
                Id = ae.Id,
                Action = ae.Action,
                Status = ae.Status,
                Data = ae.Data,
                CreatedAt = ae.CreatedAt,
                UpdatedAt = ae.UpdatedAt
            })
            .ToListAsync();
        return Ok(events);
    }

    /// <summary>
    /// Updates the status of a QueuedEvent
    /// </summary>
    [HttpPost("UpdateStatus")]
    [SwaggerResponse(200, typeof(void), Description = "Success")]
    [SwaggerResponse(404, typeof(void), Description = "Not Found")]
    public async Task<ActionResult> UpdateStatus([FromBody] QueuedEventUpdateModel model)
    {
        var queuedEvent = await _dbContext.QueuedEvents
            .Include(qe => qe.Request)
            .FirstOrDefaultAsync(qe => qe.Id == model.Id);
        if (queuedEvent == null)
        {
            return NotFound();
        }

        if (queuedEvent.Status == model.Status)
        {
            return BadRequest("Status is already set to " + model.Status);
        }

        var result = await _accountUpdateService.UpdateEvent(queuedEvent, model.Status);

        if (result.IsError)
        {
            return BadRequest(result.Message);
        }

        return Ok();
    }
}