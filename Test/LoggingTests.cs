using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Hippo.Core.Extensions;
using Hippo.Core.Models;
using Hippo.Web.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Context;
using Serilog.Core;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Exceptions.Core;
using Serilog.Exceptions.EntityFrameworkCore.Destructurers;
using Microsoft.EntityFrameworkCore;
using Test.Helpers;
using Hippo.Core.Domain;
using Shouldly;
using Xunit;

namespace Test;

[CollectionDefinition("Logging", DisableParallelization = true)]
public class LoggingCollection { }

[Collection("Logging")]
public class LoggingTests
{
    private static IConfiguration Config(params (string Key, string? Value)[] values) =>
        new ConfigurationBuilder().AddInMemoryCollection(values.ToDictionary(v => v.Key, v => v.Value)).Build();

    [Theory]
    [InlineData("https://collector.test/base", null, "https://collector.test/base/v1/logs")]
    [InlineData("https://collector.test/base/", null, "https://collector.test/base/v1/logs")]
    [InlineData("https://collector.test/base/v1/logs", null, "https://collector.test/base/v1/logs")]
    [InlineData("https://collector.test/base", "https://logs.test/custom", "https://logs.test/custom")]
    [InlineData(null, "https://logs.test/custom", "https://logs.test/custom")]
    public void LogEndpointsUseSignalOverrideOrNormalizedBase(string? general, string? specific, string expected)
    {
        var config = Config(("OTEL_EXPORTER_OTLP_ENDPOINT", general), ("OTEL_EXPORTER_OTLP_LOGS_ENDPOINT", specific));
        LoggingExtensions.ReadOtlpSettings(config, "Development")!.Endpoint.ShouldBe(expected);
    }

    [Fact]
    public void SharedSettingsPreserveEncodedValuesAndServiceIdentity()
    {
        var config = Config(("OTEL_EXPORTER_OTLP_ENDPOINT", "https://collector.test"),
            ("OTEL_EXPORTER_OTLP_HEADERS", "Authorization=ApiKey%20secret,x-custom=a%2Cb%3Dc,x-CUSTOM=last%2Cvalue"),
            ("OTEL_RESOURCE_ATTRIBUTES", "service.name=resource-name,service.version=release%201,deployment.environment.name=staging,custom=a%2Cb%3Dc"),
            ("OTEL_SERVICE_NAME", "final-service"));
        var logs = LoggingExtensions.ReadOtlpSettings(config, "Development")!;
        logs.Headers["Authorization"].ShouldBe("ApiKey secret");
        logs.Headers["x-custom"].ShouldBe("last,value");
        logs.ResourceAttributes["service.name"].ShouldBe("final-service");
        logs.ResourceAttributes["service.version"].ShouldBe("release 1");
        logs.ResourceAttributes["deployment.environment.name"].ShouldBe("staging");
        logs.ResourceAttributes["custom"].ShouldBe("a,b=c");
    }

    [Fact]
    public void EnvironmentProviderOverridesConfigurationWithoutSettingsBeingCopiedIntoEnvironment()
    {
        const string key = "OTEL_SERVICE_NAME";
        var previous = Environment.GetEnvironmentVariable(key);
        try
        {
            Environment.SetEnvironmentVariable(key, "environment-service");
            var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
            {
                [key] = "secrets-service", ["OTEL_EXPORTER_OTLP_ENDPOINT"] = "https://collector.test"
            }).AddEnvironmentVariables().Build();
            LoggingExtensions.ReadOtlpSettings(config, "Development")!.ResourceAttributes["service.name"]
                .ShouldBe("environment-service");
            var suppliedOnly = Config((key, "configuration-service"), ("OTEL_EXPORTER_OTLP_ENDPOINT", "https://collector.test"));
            LoggingExtensions.ReadOtlpSettings(suppliedOnly, "Development")!.ResourceAttributes["service.name"]
                .ShouldBe("configuration-service");
            Environment.GetEnvironmentVariable(key).ShouldBe("environment-service");
        }
        finally { Environment.SetEnvironmentVariable(key, previous); }
    }

    [Fact]
    public async Task ResultOverloadsAndUserMiddlewarePreserveContextWithoutLeaking()
    {
        var events = new CaptureSink();
        var previous = Log.Logger;
        using var logger = new LoggerConfiguration().Enrich.FromLogContext().WriteTo.Sink(events).CreateLogger();
        Log.Logger = logger;
        try
        {
            using (LogContext.PushProperty("LineNumber", 999))
            {
                Result first = Result.Error("plain", callerFilePath: "/tmp/Caller.cs", callerLineNumber: 1);
                Result<int> second = Result.Error("One {Value}", 42, callerLineNumber: 2);
                Result.Error("Two {A} {B}", 1, 2, callerLineNumber: 3);
                Result.Error("Three {A} {B} {C}", 1, 2, 3, logLevel: LogEventLevel.Warning, callerLineNumber: 4);
                first.IsError.ShouldBeTrue();
                second.Message.ShouldBe("One 42");
                Log.Information("Outer scope");
            }
            var context = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "researcher") }, "test")) };
            await new LogUserNameMiddleware(_ => { Log.Information("Inside request"); return Task.CompletedTask; }).Invoke(context);
            Log.Information("After request");
            events.Events[0].Properties["FileName"].ToString().ShouldBe("\"Caller.cs\"");
            for (var i = 0; i < 4; i++) events.Events[i].Properties["LineNumber"].ToString().ShouldBe((i + 1).ToString());
            events.Events[3].Level.ShouldBe(LogEventLevel.Warning);
            events.Events[4].Properties["LineNumber"].ToString().ShouldBe("999");
            events.Events[4].Properties.ContainsKey("FileName").ShouldBeFalse();
            events.Events[5].Properties["User"].ToString().ShouldBe("\"researcher\"");
            events.Events[6].Properties.ContainsKey("User").ShouldBeFalse();
            events.Events[6].Properties.ContainsKey("LineNumber").ShouldBeFalse();
        }
        finally { Log.Logger = previous; }
    }

    [Fact]
    public async Task EfExceptionEnrichmentRetainsExceptionAndEntryDetails()
    {
        using var db = TestDbContextFactory.Create();
        db.Users.Add(new User { FirstName = null!, LastName = "Researcher", Email = "pat@example.test" });
        var exception = await Should.ThrowAsync<DbUpdateException>(() => db.SaveChangesAsync());
        var events = new CaptureSink();
        using var logger = new LoggerConfiguration()
            .Enrich.WithExceptionDetails(new DestructuringOptionsBuilder().WithDefaultDestructurers()
                .WithDestructurers(new[] { new DbUpdateExceptionDestructurer() }))
            .WriteTo.Sink(events).CreateLogger();
        using var diagnostics = new StringWriter();
        Serilog.Debugging.SelfLog.Enable(diagnostics);
        try { logger.Error(exception, "Database write failed"); }
        finally { Serilog.Debugging.SelfLog.Disable(); }
        diagnostics.ToString().ShouldBeEmpty();
        var logEvent = events.Events.Single();
        logEvent.Exception.ShouldBeSameAs(exception);
        logEvent.Properties["ExceptionDetail"].ToString().ShouldContain("DbUpdateException");
        logEvent.Properties["ExceptionDetail"].ToString().ShouldContain("Researcher");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task OtlpEnvironmentOverridesAndShutdownExportEachEventOnce(bool failure)
    {
        var port = FreePort();
        using var listener = new HttpListener();
        listener.Prefixes.Add($"http://127.0.0.1:{port}/");
        listener.Start();
        using var environment = new OtlpEnvironment(new Dictionary<string, string?>
        {
            ["OTEL_EXPORTER_OTLP_ENDPOINT"] = $"http://127.0.0.1:{port}",
            ["OTEL_EXPORTER_OTLP_PROTOCOL"] = "http/protobuf",
            ["OTEL_EXPORTER_OTLP_HEADERS"] = "x-hippo-test=local-only",
            ["OTEL_SERVICE_NAME"] = "hippo-log-test",
            ["OTEL_RESOURCE_ATTRIBUTES"] = "deployment.environment.name=test,service.version=10,service.name=resource-name"
        });
        var received = Task.Run(async () =>
        {
            var context = await listener.GetContextAsync().WaitAsync(TimeSpan.FromSeconds(10));
            using var body = new MemoryStream();
            await context.Request.InputStream.CopyToAsync(body);
            context.Response.StatusCode = 200;
            context.Response.Close();
            return (context.Request.Url!.AbsolutePath, context.Request.Headers["x-hippo-test"], body.ToArray());
        });
        var previous = Log.Logger;
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Serilog:AppName"] = "Hippo", ["Serilog:Environment"] = "Test",
            ["OTEL_EXPORTER_OTLP_ENDPOINT"] = "http://127.0.0.1:1",
            ["OTEL_EXPORTER_OTLP_PROTOCOL"] = "grpc",
            ["OTEL_EXPORTER_OTLP_HEADERS"] = "x-hippo-test=lower-priority",
            ["OTEL_SERVICE_NAME"] = "lower-priority",
            ["OTEL_RESOURCE_ATTRIBUTES"] = "deployment.environment.name=lower-priority"
        }).AddEnvironmentVariables().Build();
        using var activity = new Activity("existing-activity").Start();
        try
        {
            Log.Logger = new LoggerConfiguration().Enrich.FromLogContext()
                .Enrich.WithProperty("jobname", "test-job").Enrich.WithProperty("jobid", "test-id")
                .WriteToHippoSinks(config).CreateLogger();
            using var services = new ServiceCollection().AddLogging(b => b.AddSerilog(Log.Logger, dispose: false)).BuildServiceProvider();
            using var scope = LogContext.PushProperty("User", "researcher");
            if (failure) Log.Error(new InvalidOperationException("local failure"), "Work failed {Count}", 42);
            else services.GetRequiredService<ILogger<LoggingTests>>().LogInformation("Work succeeded {Count}", 42);
        }
        finally { Log.CloseAndFlush(); Log.Logger = previous; }
        var (path, header, payload) = await received;
        path.ShouldBe("/v1/logs");
        header.ShouldBe("local-only");
        // Decode using the sink's own generated OTLP messages, without another telemetry SDK/provider.
        var messageType = typeof(Serilog.Sinks.OpenTelemetry.OtlpProtocol).Assembly.GetType("OpenTelemetry.Proto.Collector.Logs.V1.ExportLogsServiceRequest")!;
        var parser = (Google.Protobuf.MessageParser)messageType.GetProperty("Parser")!.GetValue(null)!;
        var json = Google.Protobuf.JsonFormatter.Default.Format(parser.ParseFrom(payload));
        using var document = System.Text.Json.JsonDocument.Parse(json);
        var resources = document.RootElement.GetProperty("resourceLogs").EnumerateArray().ToArray();
        resources.Length.ShouldBe(1);
        var records = resources[0].GetProperty("scopeLogs").EnumerateArray().SelectMany(s => s.GetProperty("logRecords").EnumerateArray()).ToArray();
        records.Length.ShouldBe(1);
        json.ShouldContain("hippo-log-test");
        var attributes = ResourceAttributes(document);
        attributes["service.name"].ShouldBe("hippo-log-test");
        attributes["service.version"].ShouldBe("10");
        attributes["deployment.environment.name"].ShouldBe("test");
        json.ShouldContain("test-job");
        json.ShouldContain("test-id");
        json.ShouldContain("researcher");
        json.ShouldContain("Application");
        json.ShouldContain("AppEnvironment");
        json.ShouldContain("Count");
        records[0].GetProperty("traceId").GetString().ShouldBe(Convert.ToBase64String(Convert.FromHexString(activity.TraceId.ToHexString())));
        records[0].GetProperty("spanId").GetString().ShouldBe(Convert.ToBase64String(Convert.FromHexString(activity.SpanId.ToHexString())));
        if (failure) { json.ShouldContain("exception.type"); json.ShouldContain("local failure"); }
    }

    [Fact]
    public void UnavailableCollectorKeepsConsoleFunctionalAndShutdownBounded()
    {
        using var environment = new OtlpEnvironment(new Dictionary<string, string?>
        {
            ["OTEL_EXPORTER_OTLP_ENDPOINT"] = $"http://127.0.0.1:{FreePort()}",
            ["OTEL_EXPORTER_OTLP_PROTOCOL"] = "http/protobuf"
        });
        using var output = new StringWriter();
        using var diagnostics = new StringWriter();
        var original = Console.Out;
        var originalError = Console.Error;
        var stopwatch = Stopwatch.StartNew();
        try
        {
            Console.SetOut(output);
            Console.SetError(diagnostics);
            LoggingExtensions.EnableLoggingDiagnostics();
            using (var logger = new LoggerConfiguration().WriteToHippoSinks(new ConfigurationBuilder().AddEnvironmentVariables().Build()).CreateLogger())
                logger.Information("Application operation completed");
        }
        finally
        {
            Serilog.Debugging.SelfLog.Disable();
            Console.SetOut(original);
            Console.SetError(originalError);
        }
        output.ToString().ShouldContain("Application operation completed");
        diagnostics.ToString().ShouldNotBeEmpty();
        diagnostics.ToString().ShouldNotContain("OTLP log export is disabled");
        stopwatch.Elapsed.ShouldBeLessThan(TimeSpan.FromSeconds(15));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void MissingLogEndpointsUseConsoleOnlyDespiteTraceSettings(string? endpoint)
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["OTEL_EXPORTER_OTLP_ENDPOINT"] = endpoint,
            ["OTEL_EXPORTER_OTLP_LOGS_ENDPOINT"] = endpoint,
            ["OTEL_EXPORTER_OTLP_TRACES_ENDPOINT"] = "https://collector.test/v1/traces",
            ["OTEL_TRACES_SAMPLER"] = "invalid",
            ["OTEL_TRACES_SAMPLER_ARG"] = "invalid",
            ["OTEL_RESOURCE_ATTRIBUTES"] = "invalid",
            // These would fail validation or parsing if export were registered.
            ["OTEL_EXPORTER_OTLP_PROTOCOL"] = "invalid",
            ["OTEL_EXPORTER_OTLP_HEADERS"] = "invalid"
        }).Build();
        using var output = new StringWriter();
        using var diagnostics = new StringWriter();
        var original = Console.Out;
        var originalError = Console.Error;
        try
        {
            Console.SetOut(output);
            Console.SetError(diagnostics);
            LoggingExtensions.EnableLoggingDiagnostics();
            using var logger = new LoggerConfiguration().WriteToHippoSinks(config).CreateLogger();
            logger.Information("Console-only operation completed");
        }
        finally
        {
            Serilog.Debugging.SelfLog.Disable();
            Console.SetOut(original);
            Console.SetError(originalError);
        }
        output.ToString().ShouldContain("Console-only operation completed");
        diagnostics.ToString().Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)
            .ShouldHaveSingleItem().ShouldContain("OTLP log export is disabled");
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task ConfigurationProvidersSupportDefaultHttpAndLogsEndpoint(bool logsSpecific, bool includeGeneral)
    {
        // In-memory values stand in for the shared user-secrets provider, without reading real credentials.
        var request = await CaptureExportAsync(endpoint => new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["OTEL_EXPORTER_OTLP_ENDPOINT"] = !logsSpecific ? endpoint + "/collector" : includeGeneral ? endpoint + "/unused" : " ",
                ["OTEL_EXPORTER_OTLP_LOGS_ENDPOINT"] = logsSpecific ? endpoint + "/custom/logs" : " ",
                ["OTEL_EXPORTER_OTLP_HEADERS"] = "x-hippo-test=local%20secret",
                ["OTEL_RESOURCE_ATTRIBUTES"] = "service.version=release%201,service.name=from-resource,deployment.environment.name=Development"
            }).Build());
        request.Path.ShouldBe(logsSpecific ? "/custom/logs" : "/collector/v1/logs");
        request.ContentType.ShouldBe("application/x-protobuf");
        request.Header.ShouldBe("local secret");
        using var document = DecodeLogs(request.Payload);
        var attributes = ResourceAttributes(document);
        attributes["service.version"].ShouldBe("release 1");
        attributes["service.name"].ShouldBe("from-resource");
        attributes["deployment.environment.name"].ShouldBe("Development");
    }

    [Fact]
    public async Task ExplicitGrpcProtocolExportsOverHttp2()
    {
        var request = await CaptureExportAsync(endpoint => new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["OTEL_EXPORTER_OTLP_ENDPOINT"] = endpoint,
                ["OTEL_EXPORTER_OTLP_PROTOCOL"] = "grpc"
            }).Build(), grpc: true);
        request.Path.ShouldBe("/opentelemetry.proto.collector.logs.v1.LogsService/Export");
        request.ContentType.ShouldStartWith("application/grpc");
        request.Protocol.ShouldBe("HTTP/2");
        request.Payload[0].ShouldBe((byte)0); // Uncompressed gRPC message; the next four bytes are its length.
        using var document = DecodeLogs(request.Payload[5..]);
        document.RootElement.GetProperty("resourceLogs").GetArrayLength().ShouldBe(1);
    }

    [Theory]
    [InlineData("Staging", "Development", "JobEnvironment", "Staging")]
    [InlineData("[External]", "Development", "JobEnvironment", "Development")]
    [InlineData(" ", null, "Development", "Development")]
    [InlineData(null, null, null, "Production")]
    [InlineData(" [External] ", " ", "[External]", "Production")]
    public async Task ResourceDefaultsIdentifyExecutableAndResolveEnvironment(string? configuredEnvironment,
        string? hostEnvironment, string? jobEnvironment, string expectedEnvironment)
    {
        var request = await CaptureExportAsync(endpoint => new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["OTEL_EXPORTER_OTLP_ENDPOINT"] = endpoint,
                ["Serilog:Environment"] = configuredEnvironment,
                ["ASPNETCORE_ENVIRONMENT"] = jobEnvironment
            }).Build(), hostEnvironment);
        using var document = DecodeLogs(request.Payload);
        var attributes = ResourceAttributes(document);
        var assembly = Assembly.GetEntryAssembly()!;
        attributes["service.name"].ShouldBe(assembly.GetName().Name);
        attributes["service.version"].ShouldBe(assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
            ?? assembly.GetName().Version!.ToString());
        attributes["deployment.environment.name"].ShouldBe(expectedEnvironment);
        attributes.Values.ShouldNotContain("[External]");
    }

    [Theory]
    [InlineData("OTEL_EXPORTER_OTLP_ENDPOINT", "not-a-url")]
    [InlineData("OTEL_EXPORTER_OTLP_LOGS_ENDPOINT", "ftp://example.test/logs")]
    [InlineData("OTEL_EXPORTER_OTLP_PROTOCOL", "http/json")]
    [InlineData("OTEL_EXPORTER_OTLP_HEADERS", "secret")]
    [InlineData("OTEL_EXPORTER_OTLP_HEADERS", "Authorization=secret%0D%0Ainjected")]
    [InlineData("OTEL_RESOURCE_ATTRIBUTES", "secret")]
    public void InvalidExplicitConnectionSettingsFailAtStartupWithoutEchoingValues(string key, string value)
    {
        var values = new Dictionary<string, string?> { ["OTEL_EXPORTER_OTLP_ENDPOINT"] = "http://127.0.0.1:4318", [key] = value };
        var config = new ConfigurationBuilder().AddInMemoryCollection(values).Build();
        var exception = Should.Throw<InvalidOperationException>(() => new LoggerConfiguration().WriteToHippoSinks(config));
        exception.Message.ShouldContain(key);
        exception.Message.ShouldNotContain(value);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task TraceSettingsAndSpanEventsDoNotEnableTraceExport(bool grpc)
    {
        using var environment = new OtlpEnvironment(new Dictionary<string, string?>
        {
            ["OTEL_EXPORTER_OTLP_TRACES_ENDPOINT"] = "ftp://unused.test/traces",
            ["OTEL_TRACES_SAMPLER"] = "invalid",
            ["OTEL_TRACES_SAMPLER_ARG"] = "invalid"
        });
        var request = await CaptureExportAsync(endpoint => Config(
            ("OTEL_EXPORTER_OTLP_ENDPOINT", endpoint),
            ("OTEL_EXPORTER_OTLP_PROTOCOL", grpc ? "grpc" : "http/protobuf"),
            ("OTEL_EXPORTER_OTLP_TRACES_ENDPOINT", endpoint + "/v1/traces"),
            ("OTEL_TRACES_SAMPLER", "invalid"),
            ("OTEL_TRACES_SAMPLER_ARG", "invalid")), grpc: grpc, emit: logger =>
        {
            using var activity = new Activity("ambient-activity").Start();
            logger.Information("Ordinary log event");
            // The sink can translate SerilogTracing-shaped events into spans when enabled.
            logger.ForContext("SpanStartTimestamp", DateTime.UtcNow.AddSeconds(-1))
                .Information("Completed operation");
        });
        using var document = DecodeLogs(grpc ? request.Payload[5..] : request.Payload);
        document.RootElement.GetProperty("resourceLogs").GetArrayLength().ShouldBe(1);
        document.RootElement.ToString().ShouldContain("Ordinary log event");
    }

    private static async Task<CapturedRequest> CaptureExportAsync(Func<string, IConfiguration> configuration,
        string? environmentName = null, bool grpc = false, Action<Logger>? emit = null)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Development" });
        builder.Logging.ClearProviders();
        builder.WebHost.ConfigureKestrel(options => options.Listen(IPAddress.Loopback, 0,
            listen => listen.Protocols = grpc ? HttpProtocols.Http2 : HttpProtocols.Http1));
        await using var app = builder.Build();
        var received = new ConcurrentQueue<CapturedRequest>();
        app.Run(async context =>
        {
            using var body = new MemoryStream();
            await context.Request.Body.CopyToAsync(body);
            var request = new CapturedRequest(context.Request.Path.Value!, context.Request.ContentType!,
                context.Request.Headers["x-hippo-test"].ToString(), context.Request.Protocol, body.ToArray());
            if (grpc)
            {
                context.Response.ContentType = "application/grpc";
                context.Response.DeclareTrailer("grpc-status");
                await context.Response.Body.WriteAsync(new byte[5]); // Framed empty ExportLogsServiceResponse.
                context.Response.AppendTrailer("grpc-status", "0");
            }
            else
            {
                context.Response.ContentType = "application/x-protobuf";
            }
            received.Enqueue(request);
        });
        await app.StartAsync();
        using (var logger = new LoggerConfiguration().WriteToHippoSinks(configuration(app.Urls.Single()), environmentName).CreateLogger())
        {
            if (emit == null) logger.Information("Configuration test event");
            else emit(logger);
        }
        // Logger disposal flushes all batches; inspect every request, including any trace export.
        await app.StopAsync();
        received.Count.ShouldBe(1);
        var exported = received.Single();
        if (grpc) exported.Path.ShouldBe("/opentelemetry.proto.collector.logs.v1.LogsService/Export");
        else exported.Path.ShouldEndWith("/logs");
        return exported;
    }

    private sealed record CapturedRequest(string Path, string ContentType, string Header, string Protocol, byte[] Payload);

    private static JsonDocument DecodeLogs(byte[] payload)
    {
        var messageType = typeof(Serilog.Sinks.OpenTelemetry.OtlpProtocol).Assembly.GetType("OpenTelemetry.Proto.Collector.Logs.V1.ExportLogsServiceRequest")!;
        var parser = (Google.Protobuf.MessageParser)messageType.GetProperty("Parser")!.GetValue(null)!;
        return JsonDocument.Parse(Google.Protobuf.JsonFormatter.Default.Format(parser.ParseFrom(payload)));
    }

    private static Dictionary<string, string> ResourceAttributes(JsonDocument document) =>
        document.RootElement.GetProperty("resourceLogs")[0].GetProperty("resource").GetProperty("attributes")
            .EnumerateArray().ToDictionary(a => a.GetProperty("key").GetString()!, a => a.GetProperty("value").GetProperty("stringValue").GetString()!);

    private static int FreePort()
    {
        using var socket = new TcpListener(IPAddress.Loopback, 0);
        socket.Start();
        return ((IPEndPoint)socket.LocalEndpoint).Port;
    }

    private sealed class CaptureSink : ILogEventSink
    {
        public List<LogEvent> Events { get; } = new();
        public void Emit(LogEvent logEvent) => Events.Add(logEvent);
    }

    private sealed class OtlpEnvironment : IDisposable
    {
        private readonly Dictionary<string, string?> _previous = new();
        public OtlpEnvironment(Dictionary<string, string?> values)
        {
            foreach (var key in new[] { "OTEL_EXPORTER_OTLP_ENDPOINT", "OTEL_EXPORTER_OTLP_LOGS_ENDPOINT", "OTEL_EXPORTER_OTLP_TRACES_ENDPOINT", "OTEL_EXPORTER_OTLP_PROTOCOL", "OTEL_EXPORTER_OTLP_HEADERS", "OTEL_SERVICE_NAME", "OTEL_RESOURCE_ATTRIBUTES", "OTEL_TRACES_SAMPLER", "OTEL_TRACES_SAMPLER_ARG" })
            {
                _previous[key] = Environment.GetEnvironmentVariable(key);
                Environment.SetEnvironmentVariable(key, values.GetValueOrDefault(key));
            }
        }
        public void Dispose()
        {
            foreach (var pair in _previous) Environment.SetEnvironmentVariable(pair.Key, pair.Value);
        }
    }
}
