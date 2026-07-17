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

      var dict = ParseToDict(output);

      Assert.Equal("info", dict["level"]);
      Assert.Equal("GET", dict["Method"]);
      Assert.Equal("/api/users", dict["Path"]);
      Assert.Equal("200", dict["StatusCode"]);
      Assert.Equal("12", dict["Elapsed"]);
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

      var dict = ParseToDict(output);

      Assert.Equal("error", dict["level"]);
      Assert.Equal("payment gateway timeout", dict["exception_msg"]);
      Assert.Equal("4567", dict["OrderId"]);

      // The spaced exception message is quoted verbatim on the wire (defeats symmetric masking).
      Assert.Contains("exception_msg=\"payment gateway timeout\"", output);
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

      var dict = ParseToDict(output);

      // trace_id/span_id must be present with valid non-empty W3C hex ids -- not merely "trace_id="
      // (a bare-key assertion would pass even if the ids were dropped/empty).
      Assert.Matches("^[0-9a-f]{32}$", dict["trace_id"]);
      Assert.Matches("^[0-9a-f]{16}$", dict["span_id"]);
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

      var dict = ParseToDict(output);

      Assert.Equal("http_requests_total", dict["metric"]);
      Assert.Equal("1024", dict["value"]);
      Assert.Equal("count", dict["unit"]);
      Assert.Equal("/api/orders", dict["endpoint"]);
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

      // The spaced value is quoted verbatim on the wire, not split into a forged field.
      Assert.Contains("reason=\"insufficient permissions\"", output);
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

      // Order is deterministic (sequential writes to one stream); assert each record exactly,
      // including the final auth "logout" line the substring checks previously skipped.
      var login = ParseToDict(lines[0]);
      Assert.Equal("auth", login["source"]);
      Assert.Equal("alice", login["user"]);
      Assert.Equal("login ok", login["msg"]);

      var query = ParseToDict(lines[1]);
      Assert.Equal("db", query["source"]);
      Assert.Equal("42", query["rows"]);
      Assert.Equal("query executed", query["msg"]);

      var request = ParseToDict(lines[2]);
      Assert.Equal("api", request["source"]);
      Assert.Equal("200", request["status"]);
      Assert.Equal("request served", request["msg"]);

      var logout = ParseToDict(lines[3]);
      Assert.Equal("auth", logout["source"]);
      Assert.Equal("alice", logout["user"]);
      Assert.Equal("logout", logout["msg"]);
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
