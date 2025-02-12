using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Exceptions.Core;
using Serilog.Exceptions.EntityFrameworkCore.Destructurers;
using Serilog.Sinks.Elasticsearch;

namespace Hippo.Jobs.Core
{
    public static class LogConfiguration
    {
        private static bool _loggingSetup;

        private static IConfigurationRoot _configuration = null!;

        public static void Setup(IConfigurationRoot configuration, string? jobName, Guid? jobId)
        {
            if (_loggingSetup) return;

            // save configuration for later calls
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            // create global logger with standard configuration
            Log.Logger = GetConfiguration()
                .CreateLogger()
                .ForContext("jobname", jobName ?? Assembly.GetEntryAssembly()?.GetName().Name)
                .ForContext("jobid", jobId ?? Guid.NewGuid());

            AppDomain.CurrentDomain.UnhandledException += (sender, e) => Log.Fatal(e.ExceptionObject as Exception, e.ExceptionObject?.ToString() ?? "");

            AppDomain.CurrentDomain.ProcessExit += (sender, e) => Log.CloseAndFlush();

#if DEBUG
            Serilog.Debugging.SelfLog.Enable(msg => Debug.WriteLine(msg));
#endif

            _loggingSetup = true;
        }

        /// <summary>
        /// Get a logger configuration that logs to stackify
        /// </summary>
        /// <returns></returns>
        public static LoggerConfiguration GetConfiguration()
        {
            if (_configuration == null) throw new InvalidOperationException("Call Setup() before requesting a Logger Configuration"); ;

            // standard logger
            var logConfig = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                // .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning) // uncomment this to hide EF core general info logs
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .Enrich.WithExceptionDetails(new DestructuringOptionsBuilder()
                    .WithDefaultDestructurers()
                    .WithDestructurers(new[] { new DbUpdateExceptionDestructurer() }));

            // various sinks
            logConfig = logConfig
                .WriteTo.Console()
                .WriteToElasticSearchCustom();

            return logConfig;
        }

        private static LoggerConfiguration WriteToElasticSearchCustom(this LoggerConfiguration logConfig)
        {
            // get logging config for ES endpoint (re-use some stackify settings for now)
            var loggingSection = _configuration.GetSection("Serilog");

            var esUrl = loggingSection.GetValue<string>("ElasticUrl"); //logging

            // only continue if a valid http url is setup in the config
            if (esUrl == null || !esUrl.StartsWith("http"))
            {
                return logConfig;
            }

            logConfig.Enrich.WithProperty("Application", loggingSection.GetValue<string>("AppName"));
            logConfig.Enrich.WithProperty("AppEnvironment", loggingSection.GetValue<string>("Environment"));

            if (Uri.TryCreate(esUrl, UriKind.Absolute, out var elasticUri))
            {
                return logConfig.WriteTo.Elasticsearch(new ElasticsearchSinkOptions(elasticUri)
                {
                    IndexFormat = "aspnet-Hippo-{0:yyyy.MM}",
                    TypeName = null
                });
            }

            throw new Exception("Couldn't get log configured");
        }
    }
}
