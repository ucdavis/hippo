using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading.Tasks;
using Hippo.Core.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace Test;

public sealed class PublishedApplicationFactAttribute : FactAttribute
{
    public PublishedApplicationFactAttribute()
    {
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("HIPPO_TEST_SQLSERVER")) ||
            string.IsNullOrEmpty(Environment.GetEnvironmentVariable("HIPPO_PUBLISH_ROOT")))
            Skip = "Set HIPPO_TEST_SQLSERVER and HIPPO_PUBLISH_ROOT to test local published executables.";
        else if (OperatingSystem.IsWindows())
            Skip = "This process smoke test uses a Unix SIGINT for graceful web shutdown.";
    }
}

public class PublishedApplicationTests
{
    [PublishedApplicationFact]
    public async Task ExecutablesStartWithNoCollectorAndReportEarlyFailures()
    {
        var root = Environment.GetEnvironmentVariable("HIPPO_PUBLISH_ROOT")!;
        var connection = new SqlConnectionStringBuilder(Environment.GetEnvironmentVariable("HIPPO_TEST_SQLSERVER"))
        { InitialCatalog = "HippoUpgradeTest_" + Guid.NewGuid().ToString("N") };
        await using var db = new AppDbContextSqlServer(new DbContextOptionsBuilder<AppDbContextSqlServer>().UseSqlServer(connection.ConnectionString).Options);
        var temp = Directory.CreateTempSubdirectory("hippo-publish-test-").FullName;
        try
        {
            db.Database.HasPendingModelChanges().ShouldBeFalse();
            await db.Database.MigrateAsync();
            await File.WriteAllTextAsync(Path.Combine(temp, "appsettings.json"), "{}");
            foreach (var job in new[] { "PuppetSync", "OrderProcess", "Notifications" })
            {
                using var process = Start(Path.Combine(root, "jobs", job, $"Hippo.Jobs.{job}.dll"), temp, connection.ConnectionString);
                var output = process.StandardOutput.ReadToEndAsync();
                var errors = process.StandardError.ReadToEndAsync();
                try { await process.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(30)); }
                finally { if (!process.HasExited) process.Kill(true); }
                var log = await output + await errors;
                process.ExitCode.ShouldBe(0, log);
                log.ShouldContain("Running Hippo.Jobs.");
                log.ShouldNotContain("Unhandled exception");
            }
            var port = FreePort();
            using (var process = Start(Path.Combine(root, "web", "Hippo.Web.dll"), Path.Combine(root, "web"), connection.ConnectionString, port))
            {
                var output = process.StandardOutput.ReadToEndAsync();
                var errors = process.StandardError.ReadToEndAsync();
                try
                {
                    using var client = new HttpClient { BaseAddress = new Uri($"http://127.0.0.1:{port}"), Timeout = TimeSpan.FromSeconds(2) };
                    string? openApi = null;
                    for (var attempt = 0; attempt < 100 && !process.HasExited; attempt++)
                    {
                        try { openApi = await client.GetStringAsync("/swagger/v1/swagger.json"); break; }
                        catch (HttpRequestException) { await Task.Delay(100); }
                    }
                    openApi.ShouldNotBeNull("Published web app must serve OpenAPI without contacting external services.");
                    using var json = JsonDocument.Parse(openApi!);
                    json.RootElement.GetProperty("paths").EnumerateObject().ShouldNotBeEmpty();
                    using var signal = Process.Start("kill", $"-INT {process.Id}")!;
                    await signal.WaitForExitAsync();
                    await process.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(15));
                    process.ExitCode.ShouldBe(0, await output + await errors);
                }
                finally { if (!process.HasExited) process.Kill(true); }
            }
            // Invalid configuration must still be visible through the console bootstrap logger.
            await File.WriteAllTextAsync(Path.Combine(temp, "appsettings.json"), "{ invalid json");
            var executables = new[] { "PuppetSync", "OrderProcess", "Notifications" }
                .Select(job => Path.Combine(root, "jobs", job, $"Hippo.Jobs.{job}.dll"))
                .Append(Path.Combine(root, "web", "Hippo.Web.dll"));
            foreach (var executable in executables)
            {
                using var process = Start(executable, temp, connection.ConnectionString);
                var output = process.StandardOutput.ReadToEndAsync();
                var errors = process.StandardError.ReadToEndAsync();
                try { await process.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(15)); }
                finally { if (!process.HasExited) process.Kill(true); }
                process.ExitCode.ShouldBe(1);
                (await output + await errors).ShouldContain("appsettings.json");
            }
        }
        finally
        {
            await db.Database.EnsureDeletedAsync();
            Directory.Delete(temp, recursive: true);
        }
    }

    private static Process Start(string executable, string workingDirectory, string connection, int port = 0)
    {
        var start = new ProcessStartInfo("dotnet") { WorkingDirectory = workingDirectory, RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false };
        start.ArgumentList.Add(executable);
        // Prevent ambient user secrets, OTLP credentials, and deployment settings entering the smoke test.
        var path = Environment.GetEnvironmentVariable("PATH");
        start.Environment.Clear();
        start.Environment["PATH"] = path;
        start.Environment["ASPNETCORE_ENVIRONMENT"] = "Production";
        start.Environment["DOTNET_ENVIRONMENT"] = "Production";
        start.Environment["Provider"] = "SqlServer";
        start.Environment["ConnectionStrings__DefaultConnection"] = connection;
        start.Environment["Dev__RecreateDb"] = "false";
        start.Environment["Dev__InitializeDb"] = "false";
        start.Environment["Authentication__ClientId"] = "test-client";
        start.Environment["Authentication__Authority"] = "https://127.0.0.1:1";
        start.Environment["Azure__TenantId"] = "11111111-1111-1111-1111-111111111111";
        start.Environment["Azure__ClientId"] = "22222222-2222-2222-2222-222222222222";
        start.Environment["Azure__ClientSecret"] = "local-test-unused";
        start.Environment["Azure__KeyVaultUrl"] = "https://127.0.0.1:1";
        start.Environment["Email__Host"] = "127.0.0.1";
        start.Environment["Email__Port"] = "1";
        start.Environment["Email__DisableSend"] = "Yes";
        start.Environment["AggieEnterprise__GraphQlUrl"] = "https://127.0.0.1:1";
        start.Environment["AggieEnterprise__TokenEndpoint"] = "https://127.0.0.1:1";
        start.Environment["OTEL_EXPORTER_OTLP_ENDPOINT"] = "http://127.0.0.1:1";
        start.Environment["OTEL_EXPORTER_OTLP_PROTOCOL"] = "http/protobuf";
        if (port != 0) start.Environment["ASPNETCORE_URLS"] = $"http://127.0.0.1:{port}";
        return Process.Start(start)!;
    }

    private static int FreePort()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        return ((IPEndPoint)listener.LocalEndpoint).Port;
    }
}
