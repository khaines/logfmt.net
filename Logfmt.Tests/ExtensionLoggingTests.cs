// Copyright (c) Ken Haines. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Logfmt.Tests
{
  using System;
  using System.Collections.Generic;
  using System.IO;

  using Logfmt.ExtensionLogging;
  using Microsoft.Extensions.Logging;
  using Xunit;

  /// <summary>
  /// Tests covering the functionality found in the Logfmt.ExtensionLogging namespace.
  /// </summary>
  public class ExtensionLoggingTests
  {
    /// <summary>
    /// Tests basic output of the Extensionlogger instance via the ILogger interface.
    /// </summary>
    [Fact]
    public void TestILoggerBasicOutput()
    {
      var outputStream = new MemoryStream();
      ILogger logger = new ExtensionLogger(new Logger(outputStream), this.GetConfiguration, "test");

      logger.LogInformation(new EventId(1, "test"), null, "test message");

      outputStream.Seek(0, SeekOrigin.Begin);
      var reader = new StreamReader(outputStream);
      var output = reader.ReadLine();

      Assert.EndsWith("level=info _OriginalFormat_=\"test message\" msg=\"test message\" event_id=1 event_name=test", output);
    }

    /// <summary>
    /// Tests output of the extension logger, ensuring that a debug message isn't emitted
    /// the level is set to INFO.
    /// </summary>
    [Fact]
    public void TestILoggerFilteredOutput()
    {
      var outputStream = new MemoryStream();
      ILogger logger = new ExtensionLogger(new Logger(outputStream, SeverityLevel.Info), this.GetConfiguration, "test");
      logger.LogDebug(new EventId(1, "test"), null, "test message {1} {2}", "foo", "bar");

      outputStream.Seek(0, SeekOrigin.Begin);
      var reader = new StreamReader(outputStream);
      var output = reader.ReadLine();
      Assert.True(output == null);
    }

    /// <summary>
    /// Tests to ensure provided state properties are logged.
    /// </summary>
    [Fact]
    public void TestILoggerStatePropOutput()
    {
      var outputStream = new MemoryStream();
      ILogger logger = new ExtensionLogger(new Logger(outputStream, SeverityLevel.Info), this.GetConfiguration, "test");
      var state = new Dictionary<string, object>
      {
        ["foo"] = "bar",
        [Logger.MessageKey] = "test message",
      };
      logger.Log(LogLevel.Warning, new EventId(1, "test"), state, null, null);

      outputStream.Seek(0, SeekOrigin.Begin);
      var reader = new StreamReader(outputStream);
      var output = reader.ReadLine();
      Assert.EndsWith("level=warn foo=bar msg=\"test message\" event_id=1 event_name=test", output);
    }

    /// <summary>
    /// Tests to ensure that duplicate keys are removed from the output and that the last set value wins
    /// in the case where the state collection is an ordered set like a list or array.
    /// </summary>
    [Fact]
    public void TestILoggerDuplicateStatePropOutput()
    {
      using var outputStream = new MemoryStream();
      ILogger logger = new ExtensionLogger(new Logger(outputStream, SeverityLevel.Info), this.GetConfiguration, "test");
      var state = new List<KeyValuePair<string, object>>
              {
                new KeyValuePair<string, object>("foo", "bar"),
                new KeyValuePair<string, object>(Logger.MessageKey, "test message"),
                new KeyValuePair<string, object>("dupe", "test message"),
                new KeyValuePair<string, object>("dupe", "test message2"),
                new KeyValuePair<string, object>("dupe", "test message3"),
                new KeyValuePair<string, object>("dupe", "test message4"),
              };
      logger.Log(LogLevel.Warning, new EventId(1, "test"), state, null, null);

      outputStream.Seek(0, SeekOrigin.Begin);
      var reader = new StreamReader(outputStream);
      var output = reader.ReadLine();
      Assert.EndsWith("level=warn foo=bar msg=\"test message\" dupe=\"test message4\" event_id=1 event_name=test", output, StringComparison.InvariantCultureIgnoreCase);
    }

    /// <summary>
    /// Tests that ExtensionLogger.WithData adds fields to subsequent log entries.
    /// </summary>
    [Fact]
    public void TestExtensionLoggerWithData()
    {
      var outputStream = new MemoryStream();
      var extLogger = new ExtensionLogger(new Logger(outputStream), this.GetConfiguration, "test");

      extLogger.WithData(new KeyValuePair<string, string>("request_id", "abc123"));
      extLogger.Log(LogLevel.Information, new EventId(0), "test", null, (s, e) => s.ToString());

      outputStream.Seek(0, SeekOrigin.Begin);
      var reader = new StreamReader(outputStream);
      var output = reader.ReadLine();

      Assert.Contains("request_id=abc123", output);
    }

    /// <summary>
    /// Tests that exception info is included in ILogger Log output.
    /// </summary>
    [Fact]
    public void TestILoggerExceptionOutput()
    {
      var outputStream = new MemoryStream();
      ILogger logger = new ExtensionLogger(new Logger(outputStream), this.GetConfiguration, "test");

      var exception = new InvalidOperationException("something broke");
      logger.LogError(exception, "An error occurred");

      outputStream.Seek(0, SeekOrigin.Begin);
      var reader = new StreamReader(outputStream);
      var output = reader.ReadLine();

      Assert.Contains("level=error", output);
      Assert.Contains("msg=\"An error occurred\"", output);
    }

    /// <summary>
    /// Tests BeginScope returns a non-null disposable (NoOpScope).
    /// </summary>
    [Fact]
    public void TestBeginScopeReturnsDisposable()
    {
      var outputStream = new MemoryStream();
      ILogger logger = new ExtensionLogger(new Logger(outputStream), this.GetConfiguration, "test");

      using var scope = logger.BeginScope("test scope");

      Assert.NotNull(scope);
    }

    /// <summary>
    /// Tests IsEnabled with the Default log level configuration.
    /// </summary>
    [Fact]
    public void TestIsEnabledWithDefaultConfig()
    {
      var outputStream = new MemoryStream();

      ExtensionLoggerConfiguration GetDefaultConfig()
      {
        var config = new ExtensionLoggerConfiguration();
        config.LogLevel["Default"] = LogLevel.Warning;
        return config;
      }

      ILogger logger = new ExtensionLogger(new Logger(outputStream), GetDefaultConfig, "unconfigured_category");

      Assert.False(logger.IsEnabled(LogLevel.Debug));
      Assert.False(logger.IsEnabled(LogLevel.Information));
      Assert.True(logger.IsEnabled(LogLevel.Warning));
      Assert.True(logger.IsEnabled(LogLevel.Error));
    }

    /// <summary>
    /// Tests IsEnabled returns false when no matching category or Default exists.
    /// </summary>
    [Fact]
    public void TestIsEnabledReturnsFalseWithNoConfig()
    {
      var outputStream = new MemoryStream();

      ExtensionLoggerConfiguration GetEmptyConfig()
      {
        return new ExtensionLoggerConfiguration();
      }

      ILogger logger = new ExtensionLogger(new Logger(outputStream), GetEmptyConfig, "unknown");

      Assert.False(logger.IsEnabled(LogLevel.Information));
      Assert.False(logger.IsEnabled(LogLevel.Error));
    }

    /// <summary>
    /// Tests that CreateLogger returns the same logger for the same category name.
    /// </summary>
    [Fact]
    public void TestCreateLoggerCacheHit()
    {
      var config = Microsoft.Extensions.Options.Options.Create(new ExtensionLoggerConfiguration());
      var monitor = new TestOptionsMonitor(config.Value);
      var provider = new ExtensionLoggerProvider(monitor);

      var logger1 = provider.CreateLogger("MyCategory");
      var logger2 = provider.CreateLogger("MyCategory");

      Assert.Same(logger1, logger2);
    }

    /// <summary>
    /// Tests that Log with EventId 0 and no name does not emit event fields.
    /// </summary>
    [Fact]
    public void TestLogWithoutEventId()
    {
      var outputStream = new MemoryStream();
      ILogger logger = new ExtensionLogger(new Logger(outputStream), this.GetConfiguration, "test");

      logger.LogInformation("no event id");

      outputStream.Seek(0, SeekOrigin.Begin);
      var reader = new StreamReader(outputStream);
      var output = reader.ReadLine();

      Assert.DoesNotContain("event_id", output);
      Assert.DoesNotContain("event_name", output);
    }

    private ExtensionLoggerConfiguration GetConfiguration()
    {
      var config = new ExtensionLoggerConfiguration();
      config.LogLevel["test"] = LogLevel.Information;
      return config;
    }

    private sealed class TestOptionsMonitor : Microsoft.Extensions.Options.IOptionsMonitor<ExtensionLoggerConfiguration>
    {
#nullable enable
      public TestOptionsMonitor(ExtensionLoggerConfiguration value)
      {
        this.CurrentValue = value;
      }

      public ExtensionLoggerConfiguration CurrentValue { get; }

      public ExtensionLoggerConfiguration Get(string? name)
      {
        return this.CurrentValue;
      }

      public IDisposable? OnChange(Action<ExtensionLoggerConfiguration, string?> listener)
      {
        return null;
      }
#nullable restore
    }
  }
}