
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
public class ActionController : Controller
{
    private readonly IAccountSyncService _accountSyncService;

    public ActionController(IAccountSyncService accountSyncService, IEmailService emailService)
    {
        _accountSyncService = accountSyncService;
    }

    /// <summary>
    /// Synchronizes all groups and accounts for all clusters
    /// </summary>
    [HttpPost("SyncPuppetAccounts")]
    [SwaggerResponse(200, typeof(void), Description = "Success")]
    public async Task<ActionResult> SyncPuppetAccounts()
    {
        Log.Information($"Account sync initiated by api");

        var success = await _accountSyncService.Run();
        if (success)
        {
            Log.Information("Account sync completed successfully.");
            return Ok();
        }
        else
        {
            Log.Error("Account sync failed.");
            return StatusCode(500, "Account sync failed.");
        }
    }
}