using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Ietws;
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
        using var socket = new TcpListener(IPAddress.Loopback, 0);
        socket.Start();
        var port = ((IPEndPoint)socket.LocalEndpoint).Port;
        socket.Stop();
        using var listener = new HttpListener();
        listener.Prefixes.Add($"http://127.0.0.1:{port}/");
        listener.Start();
        const string yaml = "group:\n  research:\n    sponsors: [pat]\nuser:\n  pat:\n    fullname: Pat Researcher\n";
        var receive = Task.Run(async () =>
        {
            var context = await listener.GetContextAsync().WaitAsync(TimeSpan.FromSeconds(10));
            var path = context.Request.Url!.AbsolutePath;
            var branch = context.Request.QueryString["ref"];
            var bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new { name = "all.yaml", path = "domains/test/merged/all.yaml", type = "file", encoding = "base64", content = Convert.ToBase64String(Encoding.UTF8.GetBytes(yaml)), sha = "test-sha", size = yaml.Length }));
            context.Response.ContentType = "application/json";
            await context.Response.OutputStream.WriteAsync(bytes);
            context.Response.Close();
            return (path, branch);
        });
        var client = new GitHubClient(new ProductHeaderValue("Hippo"), new Uri($"http://127.0.0.1:{port}/"));
        var contents = await client.Repository.Content.GetAllContentsByRef("fixture-owner", "fixture-repo", "domains/test/merged/all.yaml", "main");
        contents.Single().Content.ShouldBe(yaml);
        var (path, branch) = await receive;
        path.ShouldBe("/api/v3/repos/fixture-owner/fixture-repo/contents/domains/test/merged/all.yaml");
        branch.ShouldBe("main");
    }
}
