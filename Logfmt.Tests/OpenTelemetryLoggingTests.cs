// Copyright (c) Ken Haines. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Logfmt.Tests
{
  using System;
  using System.Collections.Generic;
  using System.IO;
  using Logfmt.OpenTelemetryLogging;
  using Microsoft.Extensions.Logging;
  using OpenTelemetry;
  using OpenTelemetry.Logs;
  using Xunit;

  /// <summary>
  /// Tests covering the functionality found in the Logfmt.OpenTelemetryLogging namespace.
  /// </summary>
  public class OpenTelemetryLoggingTests
  {
    /// <summary>
    /// Tests that the extension method properly registers the logfmt exporter.
    /// </summary>
    [Fact]
    public void TestAddLogfmtConsoleExporterExtension()
    {
      var options = new OpenTelemetryLoggerOptions();
      
      // This should not throw an exception
      var result = options.AddLogfmtConsoleExporter();
      
      Assert.NotNull(result);
      Assert.Same(options, result);
    }

    /// <summary>
    /// Tests that the ConsoleLogExporter can be created with default constructor.
    /// </summary>
    [Fact]
    public void TestConsoleLogExporterDefaultConstructor()
    {
      var exporter = new ConsoleLogExporter();
      
      Assert.NotNull(exporter);
      
      // Should not throw when disposing
      exporter.Dispose();
    }

    /// <summary>
    /// Tests that the ConsoleLogExporter can be created with custom logger.
    /// </summary>
    [Fact]
    public void TestConsoleLogExporterWithCustomLogger()
    {
      var outputStream = new MemoryStream();
      var logger = new Logger(outputStream);
      var exporter = new ConsoleLogExporter(logger);
      
      Assert.NotNull(exporter);
      
      exporter.Dispose();
    }

    /// <summary>
    /// Tests that disposing the exporter works correctly.
    /// </summary>
    [Fact]
    public void TestConsoleLogExporterDispose()
    {
      var outputStream = new MemoryStream();
      var logger = new Logger(outputStream);
      var exporter = new ConsoleLogExporter(logger);

      // Should not throw
      exporter.Dispose();

      // Double dispose should not throw
      exporter.Dispose();
    }

    /// <summary>
    /// Tests integration with Microsoft.Extensions.Logging and OpenTelemetry.
    /// </summary>
    [Fact]
    public void TestOpenTelemetryIntegration()
    {
      var outputStream = new MemoryStream();
      var customLogger = new Logger(outputStream);

      using var loggerFactory = LoggerFactory.Create(builder =>
      {
        builder.AddOpenTelemetry(options =>
        {
          options.AddProcessor(new SimpleLogRecordExportProcessor(new ConsoleLogExporter(customLogger)));
        });
      });

      var logger = loggerFactory.CreateLogger("TestCategory");
      
      // Log a message
      logger.LogInformation("Test message from OpenTelemetry");

      // Check output
      outputStream.Seek(0, SeekOrigin.Begin);
      var reader = new StreamReader(outputStream);
      var output = reader.ReadLine();

      // Should contain basic log structure with timestamp
      Assert.Contains("level=info", output, StringComparison.InvariantCultureIgnoreCase);
      Assert.Contains("msg=\"Test message from OpenTelemetry\"", output, StringComparison.InvariantCultureIgnoreCase);
      Assert.Contains("ts=", output, StringComparison.InvariantCultureIgnoreCase);
    }

    /// <summary>
    /// Tests OpenTelemetry logging with different log levels.
    /// </summary>
    [Fact]
    public void TestOpenTelemetryDifferentLogLevels()
    {
      var outputStream = new MemoryStream();
      var customLogger = new Logger(outputStream, SeverityLevel.Debug);

      using var loggerFactory = LoggerFactory.Create(builder =>
      {
        builder.AddOpenTelemetry(options =>
        {
          options.AddProcessor(new SimpleLogRecordExportProcessor(new ConsoleLogExporter(customLogger)));
        });
        builder.SetMinimumLevel(LogLevel.Debug);
      });

      var logger = loggerFactory.CreateLogger("TestCategory");
      
      // Log messages at different levels
      logger.LogDebug("Debug message");
      logger.LogInformation("Info message");
      logger.LogWarning("Warning message");
      logger.LogError("Error message");

      // Check output
      outputStream.Seek(0, SeekOrigin.Begin);
      var reader = new StreamReader(outputStream);
      
      var debugLine = reader.ReadLine();
      var infoLine = reader.ReadLine();
      var warningLine = reader.ReadLine();
      var errorLine = reader.ReadLine();

      Assert.Contains("level=debug", debugLine, StringComparison.InvariantCultureIgnoreCase);
      Assert.Contains("level=info", infoLine, StringComparison.InvariantCultureIgnoreCase);
      Assert.Contains("level=warn", warningLine, StringComparison.InvariantCultureIgnoreCase);
      Assert.Contains("level=error", errorLine, StringComparison.InvariantCultureIgnoreCase);
    }

    /// <summary>
    /// Tests OpenTelemetry logging with structured data.
    /// </summary>
    [Fact]
    public void TestOpenTelemetryWithStructuredData()
    {
      var outputStream = new MemoryStream();
      var customLogger = new Logger(outputStream);

      using var loggerFactory = LoggerFactory.Create(builder =>
      {
        builder.AddOpenTelemetry(options =>
        {
          options.AddProcessor(new SimpleLogRecordExportProcessor(new ConsoleLogExporter(customLogger)));
        });
      });

      var logger = loggerFactory.CreateLogger("TestCategory");
      
      // Log with structured data
      logger.LogInformation("User {UserId} performed action {Action}", 123, "login");

      // Check output
      outputStream.Seek(0, SeekOrigin.Begin);
      var reader = new StreamReader(outputStream);
      var output = reader.ReadLine();

      Assert.Contains("level=info", output, StringComparison.InvariantCultureIgnoreCase);
      Assert.Contains("UserId=123", output, StringComparison.InvariantCultureIgnoreCase);
      Assert.Contains("Action=login", output, StringComparison.InvariantCultureIgnoreCase);
    }

    /// <summary>
    /// Tests OpenTelemetry logging with exceptions.
    /// </summary>
    [Fact]
    public void TestOpenTelemetryWithException()
    {
      var outputStream = new MemoryStream();
      var customLogger = new Logger(outputStream);

      using var loggerFactory = LoggerFactory.Create(builder =>
      {
        builder.AddOpenTelemetry(options =>
        {
          options.AddProcessor(new SimpleLogRecordExportProcessor(new ConsoleLogExporter(customLogger)));
        });
      });

      var logger = loggerFactory.CreateLogger("TestCategory");
      
      var exception = new InvalidOperationException("Test exception");
      
      // Log with exception
      logger.LogError(exception, "An error occurred");

      // Check output
      outputStream.Seek(0, SeekOrigin.Begin);
      var reader = new StreamReader(outputStream);
      var output = reader.ReadLine();

      Assert.Contains("level=error", output, StringComparison.InvariantCultureIgnoreCase);
      Assert.Contains("msg=\"An error occurred\"", output, StringComparison.InvariantCultureIgnoreCase);
      Assert.Contains("exception_msg=\"Test exception\"", output, StringComparison.InvariantCultureIgnoreCase);
    }
  }
}
