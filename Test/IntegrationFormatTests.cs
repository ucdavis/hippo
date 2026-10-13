using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Hippo.Core.Domain;
using Hippo.Core.Models;
using Hippo.Core.Models.Email;
using Hippo.Core.Models.Settings;
using Hippo.Core.Services;
using Microsoft.Extensions.Options;
using Mjml.Net;
using Renci.SshNet;
using Shouldly;
using Test.Helpers;
using Xunit;
using YamlDotNet.Serialization;

namespace Test;

public class IntegrationFormatTests
{
    [Theory]
    [InlineData("")]
    [InlineData("ssh-ed25519 AAAATEST fixture")]
    public void PuppetYamlPreservesNamesAndOptionalKey(string key)
    {
        // This legacy implementation remains supported; validate the upgraded serializer at its boundary.
#pragma warning disable CS0618
        var service = new AccountUpdateYamlService(null!, null!);
        var method = typeof(AccountUpdateYamlService).GetMethod("GetYaml", BindingFlags.NonPublic | BindingFlags.Instance)!;
#pragma warning restore CS0618
        var yaml = (string)method.Invoke(service, new object[] { new QueuedEventModel { Data = new QueuedEventDataModel
        {
            Cluster = "farm", Groups = new() { new() { Name = "research" } },
            Accounts = new() { new() { Kerberos = "pat", Name = "Pat: Researcher", Email = "pat@example.test", Iam = "00123", Mothra = "mothra", Key = key } }
        } } })!;
        var parsed = new DeserializerBuilder().Build().Deserialize<Dictionary<string, object>>(yaml);
        var account = (Dictionary<object, object>)parsed["account"];
        account["name"].ShouldBe("Pat: Researcher");
        account["iam"].ShouldBe("00123");
        account.ContainsKey("key").ShouldBe(key.Length > 0);
        if (key.Length > 0) account["key"].ShouldBe(key);
    }

    [Fact]
    public void SshPrivateKeysAndShellPathQuotingRemainSupported()
    {
        using var rsa = RSA.Create(2048);
        using var stream = new MemoryStream(Encoding.ASCII.GetBytes(rsa.ExportRSAPrivateKeyPem()));
        using var key = new PrivateKeyFile(stream);
        key.HostKeyAlgorithms.ShouldNotBeEmpty();
        RemotePathTransformation.ShellQuote.Transform("/tmp/Pat's file; echo unsafe").ShouldBe("'/tmp/Pat'\"'\"'s file; echo unsafe'");
    }

    [Fact]
    public async Task InvalidChartStringIsRejectedWithoutCallingExternalApi()
    {
        var service = new AggieEnterpriseService(Options.Create(new AggieEnterpriseSettings
        { GraphQlUrl = "https://127.0.0.1:1", TokenEndpoint = "https://127.0.0.1:1" }));
        var result = await service.IsChartStringValid("invalid", "Debit");
        result.IsValid.ShouldBeFalse();
        result.Messages.ShouldContain("Invalid Aggie Enterprise COA format");
    }

    [Fact]
    public async Task NotificationRetainsSubjectRecipientsAndTextAlongsideRenderedHtml()
    {
        using var db = TestDbContextFactory.Create();
        var email = new CaptureEmail();
        var service = new NotificationService(db, email, Options.Create(new EmailSettings { BaseUrl = "https://hippo.example" }), new MjmlRenderer());
        var model = new SimpleNotificationModel { Emails = new[] { "pat@example.test" }, CcEmails = new[] { "admin@example.test" }, Subject = "Research update", Header = "Action needed", Paragraphs = new() { "First paragraph", "Second paragraph" } };
        (await service.SimpleNotification(model)).ShouldBeTrue();
        email.Email!.Emails.ShouldBe(model.Emails);
        email.Email.CcEmails.ShouldBe(model.CcEmails);
        email.Email.Subject.ShouldBe(model.Subject);
        email.Email.TextBody.ShouldBe($"First paragraph{Environment.NewLine}{Environment.NewLine}Second paragraph");
        email.Email.HtmlBody.ShouldContain("Action needed");
        email.Email.HtmlBody.ShouldContain("https://hippo.example/media/hpcLogo.png");
        email.Email.HtmlBody.ShouldNotContain("<mj-");
    }

    private sealed class CaptureEmail : IEmailService
    {
        public EmailModel? Email { get; private set; }
        public Task SendEmail(EmailModel emailModel) { Email = emailModel; return Task.CompletedTask; }
    }
}
