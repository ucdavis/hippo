#nullable enable

using System.Reflection;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Sinks.OpenTelemetry;

namespace Hippo.Core.Extensions;

public static class LoggingExtensions
{
    public static void EnableLoggingDiagnostics() =>
        Serilog.Debugging.SelfLog.Enable(TextWriter.Synchronized(Console.Error));

    public static LoggerConfiguration WriteToHippoSinks(this LoggerConfiguration logger, IConfiguration configuration,
        string? environmentName = null)
    {
        var settings = configuration.GetSection("Serilog");
        logger
            .Enrich.WithProperty("Application", settings["AppName"])
            .Enrich.WithProperty("AppEnvironment", settings["Environment"])
            .WriteTo.Console();

        var otlp = ReadOtlpSettings(configuration, environmentName);
        if (otlp == null)
        {
            Console.Error.WriteLine("Hippo: OTLP log export is disabled because no endpoint is configured; using console logging only.");
            return logger;
        }

        return logger.WriteTo.OpenTelemetry(options =>
        {
            // Export logs only, even when trace settings exist in the environment.
            options.Endpoint = null;
            options.LogsEndpoint = otlp.Endpoint;
            options.TracesEndpoint = null;
            options.Protocol = otlp.Protocol;
            options.Headers = otlp.Headers;
            options.ResourceAttributes = otlp.ResourceAttributes;
        }, ignoreEnvironment: true);
    }

    internal sealed record OtlpSettings(string Endpoint, OtlpProtocol Protocol,
        Dictionary<string, string> Headers, Dictionary<string, object> ResourceAttributes);

    internal static OtlpSettings? ReadOtlpSettings(IConfiguration configuration, string? environmentName)
    {
        var endpoint = NonBlank(configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);
        const string signalKey = "OTEL_EXPORTER_OTLP_LOGS_ENDPOINT";
        var signalEndpoint = NonBlank(configuration[signalKey]);
        if (endpoint == null && signalEndpoint == null) return null;

        ValidateEndpoint(endpoint, "OTEL_EXPORTER_OTLP_ENDPOINT");
        ValidateEndpoint(signalEndpoint, signalKey);
        var protocol = NonBlank(configuration["OTEL_EXPORTER_OTLP_PROTOCOL"]) switch
        {
            null or "http/protobuf" => OtlpProtocol.HttpProtobuf,
            "grpc" => OtlpProtocol.Grpc,
            _ => throw new InvalidOperationException("OTEL_EXPORTER_OTLP_PROTOCOL must be http/protobuf or grpc.")
        };

        // Preserve the Serilog sink's base URL normalization.
        var normalized = new OpenTelemetrySinkOptions { Endpoint = endpoint, Protocol = protocol };
        var fullEndpoint = signalEndpoint?.Trim() ?? normalized.LogsEndpoint!;
        var headers = ParsePairs(configuration, "OTEL_EXPORTER_OTLP_HEADERS", StringComparer.OrdinalIgnoreCase);
        // Validate without exposing malformed credentials through HttpClient exception messages.
        using (var request = new HttpRequestMessage())
        {
            try { foreach (var header in headers) request.Headers.Add(header.Key, header.Value); }
            catch (Exception ex) when (ex is FormatException or InvalidOperationException or ArgumentException)
            { throw new InvalidOperationException("OTEL_EXPORTER_OTLP_HEADERS contains an invalid HTTP header."); }
        }

        var settings = configuration.GetSection("Serilog");
        var assembly = Assembly.GetEntryAssembly() ?? typeof(LoggingExtensions).Assembly;
        var version = NonBlank(assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion)
            ?? assembly.GetName().Version?.ToString() ?? "unknown";
        var deploymentEnvironment = MetadataValue(settings["Environment"])
            ?? MetadataValue(environmentName)
            ?? MetadataValue(configuration["ASPNETCORE_ENVIRONMENT"])
            ?? "Production";

        var resources = new Dictionary<string, object>
        {
            ["service.name"] = assembly.GetName().Name!,
            ["service.version"] = version,
            ["deployment.environment.name"] = deploymentEnvironment
        };
        foreach (var pair in ParsePairs(configuration, "OTEL_RESOURCE_ATTRIBUTES", StringComparer.Ordinal))
            resources[pair.Key] = pair.Value;
        if (NonBlank(configuration["OTEL_SERVICE_NAME"]) is { } serviceName)
            resources["service.name"] = serviceName;
        return new OtlpSettings(fullEndpoint, protocol, headers, resources);
    }

    private static Dictionary<string, string> ParsePairs(IConfiguration configuration, string key, StringComparer comparer)
    {
        var pairs = new Dictionary<string, string>(comparer);
        if (NonBlank(configuration[key]) is not { } value) return pairs;
        foreach (var member in value.Split(','))
        {
            var equals = member.IndexOf('=');
            if (equals < 1 || string.IsNullOrWhiteSpace(member[..equals]))
                throw new InvalidOperationException($"{key} must contain comma-separated name=value pairs.");
            pairs[member[..equals].Trim()] = Uri.UnescapeDataString(member[(equals + 1)..].Trim());
        }
        return pairs;
    }

    private static string? NonBlank(string? value) => string.IsNullOrWhiteSpace(value) ? null : value;

    private static string? MetadataValue(string? value) =>
        string.Equals(value?.Trim(), "[External]", StringComparison.OrdinalIgnoreCase) ? null : NonBlank(value);

    private static void ValidateEndpoint(string? endpoint, string settingName)
    {
        if (endpoint == null) return;
        if (!Uri.TryCreate(endpoint, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            throw new InvalidOperationException($"{settingName} must be an absolute HTTP or HTTPS URL.");
    }
}
