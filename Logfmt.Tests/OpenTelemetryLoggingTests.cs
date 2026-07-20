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

      // An active exporter reports Success; a disposed one reports Failure without throwing.
      Assert.Equal(ExportResult.Success, exporter.Export(default));

      exporter.Dispose();

      Assert.Equal(ExportResult.Failure, exporter.Export(default));
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

      // The custom-logger exporter is functional: Export returns Success on an empty batch.
      Assert.Equal(ExportResult.Success, exporter.Export(default));

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

      Assert.True(outputStream.CanWrite);

      exporter.Dispose();

      // Dispose trickles down to the inner Logger, which disposes the underlying stream.
      Assert.False(outputStream.CanWrite);

      // Double dispose is idempotent and does not throw.
      Assert.Null(Record.Exception(() => exporter.Dispose()));
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

      // Structural: parse the record and assert exact field values.
      var fields = ParseFields(output);
      Assert.Equal("info", fields["level"]);
      Assert.Equal("Test message from OpenTelemetry", fields["msg"]);
      Assert.True(fields.ContainsKey("ts"));
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

      Assert.Equal("debug", ParseFields(debugLine)["level"]);
      Assert.Equal("info", ParseFields(infoLine)["level"]);
      Assert.Equal("warn", ParseFields(warningLine)["level"]);
      Assert.Equal("error", ParseFields(errorLine)["level"]);
      Assert.Equal("Debug message", ParseFields(debugLine)["msg"]);
      Assert.Equal("Error message", ParseFields(errorLine)["msg"]);
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

    /// <summary>
    /// Tests that the ConsoleLogExporter handles a category name in log records.
    /// </summary>
    [Fact]
    public void TestOpenTelemetryCategoryName()
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

      var logger = loggerFactory.CreateLogger("MyService");
      logger.LogInformation("test");

      outputStream.Seek(0, SeekOrigin.Begin);
      var reader = new StreamReader(outputStream);
      var output = reader.ReadLine();

      Assert.Contains("category=MyService", output);
    }

    /// <summary>
    /// Tests that the ConsoleLogExporter handles an EventId.
    /// </summary>
    [Fact]
    public void TestOpenTelemetryEventId()
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
      logger.LogInformation(new EventId(42, "MyEvent"), "test");

      outputStream.Seek(0, SeekOrigin.Begin);
      var reader = new StreamReader(outputStream);
      var output = reader.ReadLine();

      Assert.Contains("event_id=42", output);
      Assert.Contains("event_name=MyEvent", output);
    }

    /// <summary>
    /// Tests that an exception with a null StackTrace (a never-thrown exception) is exported with an
    /// empty exception_stack rather than crashing.
    /// </summary>
    [Fact]
    public void TestOpenTelemetryExceptionWithNullStackTraceEmitsEmptyStack()
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

      var logger = loggerFactory.CreateLogger("cat");
      logger.LogError(new InvalidOperationException("boom"), "err");

      outputStream.Seek(0, SeekOrigin.Begin);
      var output = new StreamReader(outputStream).ReadLine();
      var fields = new Dictionary<string, string>();
      foreach (var kvp in LogfmtParser.Parse(output))
      {
        fields[kvp.Key] = kvp.Value;
      }

      Assert.Equal("boom", fields["exception_msg"]);
      Assert.Equal(string.Empty, fields["exception_stack"]);
    }

    /// <summary>
    /// Tests that an exception whose Message getter throws does not escape export (never-throw); the
    /// exporter falls back to the exception type name.
    /// </summary>
    [Fact]
    public void TestOpenTelemetryExceptionWithThrowingMessageIsContained()
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

      var logger = loggerFactory.CreateLogger("cat");

      var ex = Record.Exception(() => logger.LogError(new ThrowingMessageException(), "err"));
      Assert.Null(ex);

      outputStream.Seek(0, SeekOrigin.Begin);
      var output = new StreamReader(outputStream).ReadLine();
      Assert.Contains("exception_msg=ThrowingMessageException", output);
    }

    /// <summary>
    /// Tests that a hostile record (whose exception StackTrace getter throws inside ExtractAttributes)
    /// is contained per-record with an [EXPORT ERROR] marker and does not abort export of the other
    /// records in the SAME batch (a batching processor delivers all three to one Export call).
    /// </summary>
    [Fact]
    public void TestOpenTelemetryHostileRecordDoesNotAbortBatch()
    {
      var outputStream = new MemoryStream();
      var customLogger = new Logger(outputStream);

      // A batching processor so all three records reach the exporter in a SINGLE Export call; this
      // proves the per-record try/catch lets the loop CONTINUE past the hostile middle record.
      var loggerFactory = LoggerFactory.Create(builder =>
      {
        builder.AddOpenTelemetry(options =>
        {
          options.AddProcessor(new BatchLogRecordExportProcessor(new ConsoleLogExporter(customLogger)));
        });
      });

      var logger = loggerFactory.CreateLogger("cat");
      logger.LogInformation("first");
      logger.LogError(new ThrowingStackTraceException(), "hostile");
      logger.LogInformation("third");

      // Dispose flushes the queued batch through one Export call before we read. It also disposes the
      // underlying stream, so read the captured bytes via ToArray() (valid after a MemoryStream close).
      loggerFactory.Dispose();

      var text = System.Text.Encoding.UTF8.GetString(outputStream.ToArray());
      var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries);

      Assert.Equal(3, lines.Length);
      Assert.Equal("first", ParseFields(lines[0])["msg"]);
      Assert.Contains("[EXPORT ERROR:", ParseFields(lines[1])["msg"]);
      Assert.Equal("third", ParseFields(lines[2])["msg"]);
    }

    private static Dictionary<string, string> ParseFields(string line)
    {
      var fields = new Dictionary<string, string>();
      foreach (var kvp in LogfmtParser.Parse(line))
      {
        fields[kvp.Key] = kvp.Value;
      }

      return fields;
    }

    private sealed class ThrowingStackTraceException : Exception
    {
      public override string StackTrace => throw new InvalidOperationException("stack getter threw");
    }

    private sealed class ThrowingMessageException : Exception
    {
      public override string Message => throw new InvalidOperationException("message getter threw");
    }
  }
}
