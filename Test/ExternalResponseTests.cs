using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ietws;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Octokit;
using Shouldly;
using Xunit;

namespace Test;

public class ExternalResponseTests
{
    [Fact]
    public void IamKerberosResponseRetainsClaimsSourceFields()
    {
        var person = JsonConvert.DeserializeObject<KerberosResult>("""
            { "iamId": "00123", "userId": "pat", "dFirstName": "Pat", "dLastName": "Researcher", "mothraId": "mothra1" }
            """)!;
        person.IamId.ShouldBe("00123");
        person.UserId.ShouldBe("pat");
        person.FirstName.ShouldBe("Pat");
        person.LastName.ShouldBe("Researcher");
        person.MothraId.ShouldBe("mothra1");
    }

    [Fact]
    public async Task GitHubContentsRequestRetainsBranchPathAndDecodesYaml()
    {
        var builder = WebApplication.CreateSlimBuilder();
        builder.WebHost.ConfigureKestrel(options => options.Listen(IPAddress.Loopback, 0));
        await using var app = builder.Build();
        const string yaml = "group:\n  research:\n    sponsors: [pat]\nuser:\n  pat:\n    fullname: Pat Researcher\n";
        var receive = new TaskCompletionSource<(string Path, string Branch)>(TaskCreationOptions.RunContinuationsAsynchronously);
        app.Run(async context =>
        {
            var path = context.Request.Path.Value!;
            var branch = context.Request.Query["ref"].ToString();
            var bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new { name = "all.yaml", path = "domains/test/merged/all.yaml", type = "file", encoding = "base64", content = Convert.ToBase64String(Encoding.UTF8.GetBytes(yaml)), sha = "test-sha", size = yaml.Length }));
            context.Response.ContentType = "application/json";
            await context.Response.Body.WriteAsync(bytes, context.RequestAborted);
            receive.TrySetResult((path, branch));
        });
        await app.StartAsync();
        try
        {
            var address = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single();
            var client = new GitHubClient(new ProductHeaderValue("Hippo"), new Uri(address));
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var contents = await client.Repository.Content.GetAllContentsByRef("fixture-owner", "fixture-repo", "domains/test/merged/all.yaml", "main")
                .WaitAsync(timeout.Token);
            contents.Single().Content.ShouldBe(yaml);
            var (path, branch) = await receive.Task.WaitAsync(timeout.Token);
            path.ShouldBe("/api/v3/repos/fixture-owner/fixture-repo/contents/domains/test/merged/all.yaml");
            branch.ShouldBe("main");
        }
        finally
        {
            using var shutdownTimeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            await app.StopAsync(shutdownTimeout.Token);
        }
    }
}
