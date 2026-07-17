// Copyright (c) Ken Haines. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Logfmt.Tests
{
  using System;
  using System.Collections.Generic;
  using System.Diagnostics;
  using System.IO;
  using Logfmt;
  using Logfmt.ExtensionLogging;
  using Logfmt.OpenTelemetryLogging;
  using Microsoft.Extensions.Logging;
  using OpenTelemetry;
  using OpenTelemetry.Logs;
  using Xunit;

  /// <summary>
  /// Integration tests exercising real-world logging scenarios through the supported
  /// Microsoft.Extensions.Logging and OpenTelemetry entry points.
  /// </summary>
  /// <remarks>
  /// Serilog and NLog scenarios are intentionally out of scope: this library provides
  /// Microsoft.Extensions.Logging and OpenTelemetry integrations only, with no Serilog or NLog adapter.
  /// </remarks>
  public class IntegrationScenarioTests
  {
    /// <summary>
    /// Tests an ASP.NET Core-style request completion log with structured route data.
    /// </summary>
    [Fact]
    public void AspNetCoreStyleRequestLoggingProducesStructuredOutput()
    {
      var outputStream = new MemoryStream();
      ILogger logger = new ExtensionLogger(new Logger(outputStream, SeverityLevel.Info), AllEnabledConfig, "Microsoft.AspNetCore.Hosting");

      logger.LogInformation("Request finished {Method} {Path} {StatusCode} in {Elapsed}ms", "GET", "/api/users", 200, 12);

      outputStream.Seek(0, SeekOrigin.Begin);
      var output = new StreamReader(outputStream).ReadLine();

      Assert.Contains("level=info", output);
      Assert.Contains("Method=GET", output);
      Assert.Contains("Path=/api/users", output);
      Assert.Contains("StatusCode=200", output);
      Assert.Contains("Elapsed=12", output);
    }

    /// <summary>
    /// Tests a real application error scenario where an exception and context are exported through OpenTelemetry.
    /// </summary>
    [Fact]
    public void RealApplicationErrorScenarioLogsExceptionViaOpenTelemetry()
    {
      var outputStream = new MemoryStream();
      var customLogger = new Logger(outputStream);

      using var loggerFactory = LoggerFactory.Create(builder =>
      {
        builder.AddOpenTelemetry(options =>
        {
          options.IncludeFormattedMessage = true;
          options.AddProcessor(new SimpleLogRecordExportProcessor(new ConsoleLogExporter(customLogger)));
        });
      });

      var logger = loggerFactory.CreateLogger("OrderService");

      try
      {
        throw new InvalidOperationException("payment gateway timeout");
      }
      catch (InvalidOperationException ex)
      {
        logger.LogError(ex, "Failed to process order {OrderId}", 4567);
      }

      outputStream.Seek(0, SeekOrigin.Begin);
      var output = new StreamReader(outputStream).ReadLine();

      Assert.Contains("level=error", output);
      Assert.Contains("exception_msg=\"payment gateway timeout\"", output);
      Assert.Contains("OrderId=4567", output);
    }

    /// <summary>
    /// Tests that distributed-tracing context (trace and span ids) is exported when logging within an activity.
    /// </summary>
    [Fact]
    public void DistributedTracingContextIsExported()
    {
      var outputStream = new MemoryStream();
      var customLogger = new Logger(outputStream);

      using var listener = new ActivityListener
      {
        ShouldListenTo = _ => true,
        Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
      };
      ActivitySource.AddActivityListener(listener);

      using var source = new ActivitySource("Logfmt.Tests.Integration");

      using var loggerFactory = LoggerFactory.Create(builder =>
      {
        builder.AddOpenTelemetry(options =>
        {
          options.AddProcessor(new SimpleLogRecordExportProcessor(new ConsoleLogExporter(customLogger)));
        });
      });

      var logger = loggerFactory.CreateLogger("Traced");

      using (source.StartActivity("request"))
      {
        logger.LogInformation("handled request");
      }

      outputStream.Seek(0, SeekOrigin.Begin);
      var output = new StreamReader(outputStream).ReadLine();

      Assert.Contains("trace_id=", output);
      Assert.Contains("span_id=", output);
    }

    /// <summary>
    /// Tests logging metric-style structured data.
    /// </summary>
    [Fact]
    public void MetricsStyleLoggingProducesStructuredOutput()
    {
      var outputStream = new MemoryStream();
      using var logger = new Logger(outputStream);

      logger.Info("metric recorded", "metric", "http_requests_total", "value", "1024", "unit", "count", "endpoint", "/api/orders");

      outputStream.Seek(0, SeekOrigin.Begin);
      var output = new StreamReader(outputStream).ReadLine();

      Assert.Contains("metric=http_requests_total", output);
      Assert.Contains("value=1024", output);
      Assert.Contains("unit=count", output);
      Assert.Contains("endpoint=/api/orders", output);
    }

    /// <summary>
    /// Tests a security-audit log format, including a spaced value that must be quoted and round-trip.
    /// </summary>
    [Fact]
    public void SecurityAuditLoggingRoundTrips()
    {
      var outputStream = new MemoryStream();
      using var logger = new Logger(outputStream);

      logger.Info("audit event", "actor", "user:alice", "action", "DELETE", "resource", "/vault/secret-1", "outcome", "denied", "reason", "insufficient permissions");

      outputStream.Seek(0, SeekOrigin.Begin);
      var output = new StreamReader(outputStream).ReadLine();
      var dict = ParseToDict(output);

      Assert.Equal("user:alice", dict["actor"]);
      Assert.Equal("DELETE", dict["action"]);
      Assert.Equal("/vault/secret-1", dict["resource"]);
      Assert.Equal("denied", dict["outcome"]);
      Assert.Equal("insufficient permissions", dict["reason"]);
    }

    /// <summary>
    /// Tests batch logging from multiple sources sharing a single output stream via WithData.
    /// </summary>
    [Fact]
    public void BatchLoggingFromMultipleSourcesInterleavesOnOneStream()
    {
      var outputStream = new MemoryStream();
      var baseLogger = new Logger(outputStream, SeverityLevel.Info);
      var authLogger = baseLogger.WithData("source", "auth");
      var dbLogger = baseLogger.WithData("source", "db");
      var apiLogger = baseLogger.WithData("source", "api");

      authLogger.Info("login ok", "user", "alice");
      dbLogger.Info("query executed", "rows", "42");
      apiLogger.Info("request served", "status", "200");
      authLogger.Info("logout", "user", "alice");

      outputStream.Seek(0, SeekOrigin.Begin);
      var reader = new StreamReader(outputStream);
      var lines = new List<string>();
      string line;
      while ((line = reader.ReadLine()) != null)
      {
        lines.Add(line);
      }

      Assert.Equal(4, lines.Count);
      Assert.Contains(lines, l => l.Contains("source=auth") && l.Contains("user=alice"));
      Assert.Contains(lines, l => l.Contains("source=db") && l.Contains("rows=42"));
      Assert.Contains(lines, l => l.Contains("source=api") && l.Contains("status=200"));
    }

    private static ExtensionLoggerConfiguration AllEnabledConfig()
    {
      var config = new ExtensionLoggerConfiguration();
      config.LogLevel["Default"] = LogLevel.Trace;
      return config;
    }

    private static Dictionary<string, string> ParseToDict(string line)
    {
      var dict = new Dictionary<string, string>();
      foreach (var kvp in LogfmtParser.Parse(line))
      {
        dict[kvp.Key] = kvp.Value;
      }

      return dict;
    }
  }
}
