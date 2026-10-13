using System.Collections.Generic;
using System.Threading.Tasks;
using Hippo.Core.Extensions;
using Hippo.Core.Models.Email;
using Mjml.Net;
using Shouldly;
using Xunit;

namespace Test;

public class EmailRenderingTests
{
    public static IEnumerable<object[]> Templates()
    {
        foreach (var name in new[] { "AccountDecision", "GroupDecision", "AdminOverrideDecision" })
            yield return new object[] { name, new DecisionModel { RequesterName = "Pat Researcher", Decision = "Approved", DecisionDetails = "Approved for research", RequestUrl = "https://hippo.example/requests/1" }, "Pat Researcher" };
        foreach (var name in new[] { "AccountRequest", "GroupRequest" })
            yield return new object[] { name, new NewRequestModel { RequesterName = "Pat Researcher", GroupName = "Research group", RequestUrl = "https://hippo.example/requests/1" }, "Pat Researcher" };
        foreach (var name in new[] { "OrderNotification", "OrderNotificationTwoButton", "OrderAdminPaymentFail", "OrdersInCreated" })
            yield return new object[] { name, new OrderNotificationModel { Header = "Order update", Paragraphs = new() { "https://hippo.example/order/1" }, ButtonUrl = "https://hippo.example/orders", ButtonTwoUrl = "https://hippo.example/cancel", ButtonTwoText = "Cancel" }, "Order update" };
        yield return new object[] { "SimpleNotification", new SimpleNotificationModel { Header = "Important notice", Paragraphs = new() { "Notification paragraph" } }, "Notification paragraph" };
        yield return new object[] { "PendingDecisions", new PendingDecisionsModel { Header = "Pending decisions", ButtonUrl = "https://hippo.example/requests" }, "Pending decisions" };
        yield return new object[] { "OrderPaymentNotification", new EmailOrderPaymentModel { Header = "Payment update", ButtonUrl = "https://hippo.example/order/1", Transfers = new() { new() { Amount = 10m, Direction = "Debit", FinancialSegmentString = "Chart-string" } } }, "Chart-string" };
        yield return new object[] { "Sample", new SampleModel { Name = "Pat Researcher", SomeText = "Sample paragraph" }, "Pat Researcher" };
    }

    [Theory]
    [MemberData(nameof(Templates))]
    public async Task EveryTemplateRendersContentAndSharedLayout(string name, object model, string expected)
    {
        var html = await new MjmlRenderer().RenderView($"/Views/Emails/{name}_mjml.cshtml", model);
        html.ShouldContain(expected);
        html.ShouldContain("This email was automatically generated");
        html.ShouldNotContain("<mj-");
        html.ShouldNotContain("<mjml");
    }

    [Theory]
    [InlineData("AccountDecision")]
    [InlineData("GroupDecision")]
    [InlineData("AdminOverrideDecision")]
    public async Task DecisionDetailsAreConditional(string name)
    {
        var model = new DecisionModel { Decision = "Approved" };
        var renderer = new MjmlRenderer();
        var html = await renderer.RenderView($"/Views/Emails/{name}_mjml.cshtml", model);
        html.ShouldNotContain("Decision Details");
        model.DecisionDetails = "Only for the research project";
        html = await renderer.RenderView($"/Views/Emails/{name}_mjml.cshtml", model);
        html.ShouldContain(model.DecisionDetails);
        html.ShouldNotContain("<mj-");
    }

    [Fact]
    public async Task ButtonsAndPaymentLinksRemainUsable()
    {
        var html = await new MjmlRenderer().RenderView("/Views/Emails/OrderNotificationTwoButton_mjml.cshtml",
            new OrderNotificationModel { ButtonText = "Approve", ButtonUrl = "https://hippo.example/approve", ButtonTwoText = "Reject", ButtonTwoUrl = "https://hippo.example/reject" });
        html.ShouldContain("href=\"https://hippo.example/approve\"");
        html.ShouldContain("href=\"https://hippo.example/reject\"");
        html.ShouldContain("Approve");
        html.ShouldContain("Reject");
    }
}
