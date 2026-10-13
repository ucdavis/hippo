using System;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Exceptions.Core;
using Serilog.Exceptions.EntityFrameworkCore.Destructurers;
using Hippo.Core.Extensions;

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
            var logger = GetConfiguration()
                .Enrich.WithProperty("jobname", jobName ?? Assembly.GetEntryAssembly()?.GetName().Name)
                .Enrich.WithProperty("jobid", jobId ?? Guid.NewGuid())
                .CreateLogger();
            Log.CloseAndFlush();
            Log.Logger = logger;

            AppDomain.CurrentDomain.UnhandledException += (sender, e) => Log.Fatal(e.ExceptionObject as Exception, e.ExceptionObject?.ToString() ?? "");

            _loggingSetup = true;
        }

        /// <summary>
        /// Get a logger configuration that logs to console and OTLP
        /// </summary>
        /// <returns></returns>
        public static LoggerConfiguration GetConfiguration()
        {
            if (_configuration == null) throw new InvalidOperationException("Call Setup() before requesting a Logger Configuration");

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

            return logConfig.WriteToHippoSinks(_configuration);
        }
    }
}
