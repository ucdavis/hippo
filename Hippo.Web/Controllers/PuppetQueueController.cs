
using Hippo.Web.Handlers;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;


namespace Hippo.Web.Controllers;

[ApiKey]
[ApiController]
[Route("api/[controller]")]
[SwaggerResponse(400, typeof(void))]
[SwaggerResponse(401, typeof(void))]
public class PuppetQueueController : Controller
{
    [HttpGet]
    public IActionResult Test()
    {
        return Ok();
    }
}