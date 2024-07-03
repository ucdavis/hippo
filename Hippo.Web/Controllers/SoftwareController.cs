using Hippo.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Hippo.Core.Models;
using Hippo.Email.Models;

namespace Hippo.Web.Controllers;

[Authorize]
public class SoftwareController : SuperController
{
    private readonly IUserService _userService;
    private readonly IHistoryService _historyService;
    private readonly IEmailService _emailService;
    private readonly INotificationService _notificationService;
    private readonly IIdentityService _identityService;

    public SoftwareController(IUserService userService, IHistoryService historyService,
        IEmailService emailService, INotificationService notificationService, IIdentityService identityService)
    {
        _userService = userService;
        _historyService = historyService;
        _emailService = emailService;
        _notificationService = notificationService;
        _identityService = identityService;
    }

    [HttpPost]
    public async Task<ActionResult> RequestInstall([FromBody] SoftwareRequestModel softwareRequestModel)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var currentUser = await _userService.GetCurrentUser();
        // call into IAM to ensure we have the latest name for user...
        var currentIdentity = await _identityService.GetByKerberos(currentUser.Kerberos);

        // generate plain text email that will be forwarded to ServiceNow
        var emailModel = new EmailModel
        {
            Emails = new string[] { "hpc-help@ucdavis.edu" },
            Subject = "HPC Software Install Request",
            TextBody = @$"Category: Request

Subcategory: New

Caller: {currentIdentity.Name}

ConfigurationItem: HPC Software

Cluster Name: {softwareRequestModel.ClusterName}

Email: {softwareRequestModel.Email}

Account Name: {softwareRequestModel.AccountName}

Account Kerberos: {softwareRequestModel.AccountKerberos}

Requester Name: {currentIdentity.Name}

Requester Kerberos: {currentIdentity.Kerberos}

Software Title: {softwareRequestModel.SoftwareTitle}

Software License: {softwareRequestModel.SoftwareLicense}

Software Home Page: {softwareRequestModel.SoftwareHomePage}

Benefit Description: {softwareRequestModel.BenefitDescription}

Additional Information: {softwareRequestModel.AdditionalInformation}
"
        };

        await _emailService.SendEmail(emailModel);
        await _notificationService.SimpleNotification(new SimpleNotificationModel
        {
            Subject = "HPC Software Install Request",
            Header = "Software request received",
            Paragraphs = new List<string>
            {
                "Thank you for submitting your request. We appreciate your interest and will begin evaluating the details provided. Our team will review your submission thoroughly and respond as soon as possible.",
                "If we need any further information, we will contact you directly.",
                "Thank you for your patience."
            }
        }, new string[] { currentUser.Email });
        await _historyService.SoftwareInstallRequested(softwareRequestModel);

        return Ok();
    }

}
