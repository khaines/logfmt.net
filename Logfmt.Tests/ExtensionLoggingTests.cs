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

    /// <summary>
    /// Tests that a formatter that throws an exception does not crash the logger.
    /// </summary>
    [Fact]
    public void TestFormatterExceptionDoesNotCrash()
    {
      var outputStream = new MemoryStream();
      ILogger logger = new ExtensionLogger(new Logger(outputStream), this.GetConfiguration, "test");

      logger.Log(
          LogLevel.Information,
          new EventId(0),
          "test state",
          null,
          (Func<string, Exception, string>)((s, e) => throw new InvalidOperationException("formatter broke")));

      outputStream.Seek(0, SeekOrigin.Begin);
      var reader = new StreamReader(outputStream);
      var output = reader.ReadLine();

      Assert.Contains("FORMATTER ERROR", output);
    }

    /// <summary>
    /// Tests that state properties with null values are skipped.
    /// </summary>
    [Fact]
    public void TestStateWithNullPropertyValuesAreSkipped()
    {
      var outputStream = new MemoryStream();
      ILogger logger = new ExtensionLogger(new Logger(outputStream), this.GetConfiguration, "test");

      var state = new Dictionary<string, object>
      {
        ["present"] = "yes",
        ["missing"] = null,
      };
      logger.Log(LogLevel.Warning, new EventId(0), state, null, null);

      outputStream.Seek(0, SeekOrigin.Begin);
      var reader = new StreamReader(outputStream);
      var output = reader.ReadLine();

      Assert.Contains("present=yes", output);
      Assert.DoesNotContain("missing", output);
    }

    /// <summary>
    /// Tests that a non-enumerable state is handled gracefully.
    /// </summary>
    [Fact]
    public void TestNonEnumerableStateIsHandled()
    {
      var outputStream = new MemoryStream();
      ILogger logger = new ExtensionLogger(new Logger(outputStream), this.GetConfiguration, "test");

      logger.Log(
          LogLevel.Information,
          new EventId(0),
          42,
          null,
          (s, e) => $"state is {s}");

      outputStream.Seek(0, SeekOrigin.Begin);
      var reader = new StreamReader(outputStream);
      var output = reader.ReadLine();

      Assert.Contains("msg=\"state is 42\"", output);
    }

    /// <summary>
    /// Tests that a large state dictionary (over 1000 properties) is fully logged.
    /// </summary>
    [Fact]
    public void TestLargeStateDictionaryIsFullyLogged()
    {
      var outputStream = new MemoryStream();
      ILogger logger = new ExtensionLogger(new Logger(outputStream, SeverityLevel.Info), this.GetConfiguration, "test");

      const int count = 1500;
      var state = new Dictionary<string, object>();
      for (int i = 0; i < count; i++)
      {
        state[$"key{i}"] = $"val{i}";
      }

      logger.Log(LogLevel.Information, new EventId(0), state, null, null);

      outputStream.Seek(0, SeekOrigin.Begin);
      var output = new StreamReader(outputStream).ReadLine();

      var fields = new Dictionary<string, string>();
      foreach (var kvp in LogfmtParser.Parse(output))
      {
        fields[kvp.Key] = kvp.Value;
      }

      // Every one of the properties must be present with its exact value; remove each as verified.
      for (int i = 0; i < count; i++)
      {
        Assert.True(fields.TryGetValue($"key{i}", out var value), $"key{i} missing from output");
        Assert.Equal($"val{i}", value);
        fields.Remove($"key{i}");
      }

      // Only the framework-added fields may remain: no dropped, extra, or leaked state field of
      // any name (not merely key-prefixed ones).
      fields.Remove("ts");
      fields.Remove("level");
      Assert.Empty(fields);
    }

    /// <summary>
    /// Tests that a complex nested object state value is rendered via ToString without crashing.
    /// </summary>
    [Fact]
    public void TestComplexNestedObjectStateUsesToString()
    {
      var outputStream = new MemoryStream();
      ILogger logger = new ExtensionLogger(new Logger(outputStream, SeverityLevel.Info), this.GetConfiguration, "test");

      var nested = new Dictionary<string, object>
      {
        ["inner"] = new List<int> { 1, 2, 3 },
      };
      var state = new Dictionary<string, object>
      {
        ["payload"] = nested,
      };

      var ex = Record.Exception(() => logger.Log(LogLevel.Information, new EventId(0), state, null, null));

      Assert.Null(ex);
      outputStream.Seek(0, SeekOrigin.Begin);
      var output = new StreamReader(outputStream).ReadLine();

      var fields = new Dictionary<string, string>();
      foreach (var kvp in LogfmtParser.Parse(output))
      {
        fields[kvp.Key] = kvp.Value;
      }

      // The nested object is rendered via its ToString() (the default type name here), not dropped/emptied.
      Assert.Equal(nested.ToString(), fields["payload"]);
    }

    /// <summary>
    /// Tests that a very deeply nested state value cannot cause recursion or a StackOverflow,
    /// because the logger renders each value via ToString() exactly once and never traverses graphs.
    /// </summary>
    [Fact]
    public void TestDeeplyNestedStateValueIsRenderedWithoutRecursion()
    {
      var outputStream = new MemoryStream();
      ILogger logger = new ExtensionLogger(new Logger(outputStream, SeverityLevel.Info), this.GetConfiguration, "test");

      object deep = "leaf";
      for (int i = 0; i < 10000; i++)
      {
        deep = new List<object> { deep };
      }

      var state = new Dictionary<string, object>
      {
        ["deep"] = deep,
      };

      var ex = Record.Exception(() => logger.Log(LogLevel.Information, new EventId(0), state, null, null));

      Assert.Null(ex);
      outputStream.Seek(0, SeekOrigin.Begin);
      var output = new StreamReader(outputStream).ReadLine();

      var fields = new Dictionary<string, string>();
      foreach (var kvp in LogfmtParser.Parse(output))
      {
        fields[kvp.Key] = kvp.Value;
      }

      // Rendered via the top-level object's ToString() (its List type name), never traversed.
      Assert.Equal(deep.ToString(), fields["deep"]);
    }

    /// <summary>
    /// Tests that a circular reference among state values is harmless because values are not traversed.
    /// </summary>
    [Fact]
    public void TestCircularReferenceInStateValuesIsHandled()
    {
      var outputStream = new MemoryStream();
      ILogger logger = new ExtensionLogger(new Logger(outputStream, SeverityLevel.Info), this.GetConfiguration, "test");

      var a = new Node();
      var b = new Node();
      a.Other = b;
      b.Other = a;

      var state = new Dictionary<string, object>
      {
        ["node"] = a,
      };

      var ex = Record.Exception(() => logger.Log(LogLevel.Information, new EventId(0), state, null, null));

      Assert.Null(ex);

      outputStream.Seek(0, SeekOrigin.Begin);
      var output = new StreamReader(outputStream).ReadLine();

      var fields = new Dictionary<string, string>();
      foreach (var kvp in LogfmtParser.Parse(output))
      {
        fields[kvp.Key] = kvp.Value;
      }

      // Rendered via the node's own ToString() exactly once (never traversed), so the cycle is harmless.
      Assert.Equal(a.ToString(), fields["node"]);
    }

    /// <summary>
    /// Tests that concurrent logging with per-call state dictionaries does not throw or drop entries.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async System.Threading.Tasks.Task TestConcurrentStateLoggingDoesNotThrow()
    {
      var outputStream = new MemoryStream();
      ILogger logger = new ExtensionLogger(new Logger(outputStream, SeverityLevel.Info), this.GetConfiguration, "test");
      var exceptions = new System.Collections.Concurrent.ConcurrentBag<Exception>();

      var tasks = new System.Threading.Tasks.Task[8];
      for (int t = 0; t < tasks.Length; t++)
      {
        var id = t;
        tasks[t] = System.Threading.Tasks.Task.Run(() =>
        {
          try
          {
            for (int i = 0; i < 50; i++)
            {
              var state = new Dictionary<string, object>
              {
                ["thread"] = id,
                ["iter"] = i,
              };
              logger.Log(LogLevel.Information, new EventId(0), state, null, null);
            }
          }
          catch (Exception ex)
          {
            exceptions.Add(ex);
          }
        });
      }

      await System.Threading.Tasks.Task.WhenAll(tasks);

      Assert.Empty(exceptions);
      outputStream.Seek(0, SeekOrigin.Begin);
      var reader = new StreamReader(outputStream);
      var seen = new HashSet<string>();
      var count = 0;
      string line;
      while ((line = reader.ReadLine()) != null)
      {
        Assert.StartsWith("ts=", line);

        var fields = new Dictionary<string, string>();
        foreach (var kvp in LogfmtParser.Parse(line))
        {
          fields[kvp.Key] = kvp.Value;
        }

        Assert.True(fields.TryGetValue("thread", out var thread), "thread field missing from a log line");
        Assert.True(fields.TryGetValue("iter", out var iter), "iter field missing from a log line");
        var pair = thread + ":" + iter;
        Assert.True(seen.Add(pair), $"duplicate entry {pair}");
        count++;
      }

      // Every (thread, iter) pair was logged exactly once: no throw, no drop, no duplication.
      Assert.Equal(400, count);
      Assert.Equal(400, seen.Count);
      for (int t = 0; t < 8; t++)
      {
        for (int i = 0; i < 50; i++)
        {
          Assert.Contains(t + ":" + i, seen);
        }
      }
    }

    /// <summary>
    /// Tests that multiple null values interleaved with valid values are skipped.
    /// </summary>
    [Fact]
    public void TestStateWithMultipleNullAndValidValues()
    {
      var outputStream = new MemoryStream();
      ILogger logger = new ExtensionLogger(new Logger(outputStream, SeverityLevel.Info), this.GetConfiguration, "test");

      var state = new Dictionary<string, object>
      {
        ["a"] = "1",
        ["b"] = null,
        ["c"] = "3",
        ["d"] = null,
        ["e"] = "5",
      };

      logger.Log(LogLevel.Information, new EventId(0), state, null, null);

      outputStream.Seek(0, SeekOrigin.Begin);
      var output = new StreamReader(outputStream).ReadLine();

      Assert.Contains("a=1", output);
      Assert.Contains("c=3", output);
      Assert.Contains("e=5", output);
      Assert.DoesNotContain("b=", output);
      Assert.DoesNotContain("d=", output);
    }

    /// <summary>
    /// Tests that an ordered array state deduplicates keys with the last value winning.
    /// </summary>
    [Fact]
    public void TestOrderedArrayStateDedupesLastValueWins()
    {
      var outputStream = new MemoryStream();
      ILogger logger = new ExtensionLogger(new Logger(outputStream, SeverityLevel.Info), this.GetConfiguration, "test");

      var state = new[]
      {
        new KeyValuePair<string, object>("k", "first"),
        new KeyValuePair<string, object>("k", "second"),
        new KeyValuePair<string, object>("k", "third"),
      };

      logger.Log(LogLevel.Information, new EventId(0), state, null, null);

      outputStream.Seek(0, SeekOrigin.Begin);
      var output = new StreamReader(outputStream).ReadLine();

      Assert.Contains("k=third", output);
      Assert.DoesNotContain("k=first", output);
      Assert.DoesNotContain("k=second", output);
    }

    /// <summary>
    /// Tests that a state value whose ToString throws does not crash the logging call, and that the
    /// exception message is interpolated into the VALUE ERROR placeholder.
    /// </summary>
    [Fact]
    public void TestStateValueWithThrowingToStringDoesNotCrash()
    {
      var outputStream = new MemoryStream();
      ILogger logger = new ExtensionLogger(new Logger(outputStream, SeverityLevel.Info), this.GetConfiguration, "test");

      var state = new Dictionary<string, object>
      {
        ["bad"] = new ThrowingToString(),
        ["good"] = "value",
      };

      var ex = Record.Exception(() => logger.Log(LogLevel.Information, new EventId(0), state, null, null));

      Assert.Null(ex);

      outputStream.Seek(0, SeekOrigin.Begin);
      var output = new StreamReader(outputStream).ReadLine();
      Assert.Contains("good=value", output);
      Assert.Contains("[VALUE ERROR: boom]", output);
    }

    /// <summary>
    /// Tests that a ToString exception whose message contains logfmt-significant characters is escaped
    /// into a single record and cannot inject a forged field via the VALUE ERROR path.
    /// </summary>
    [Fact]
    public void TestStateValueToStringExceptionMessageIsEscaped()
    {
      var outputStream = new MemoryStream();
      ILogger logger = new ExtensionLogger(new Logger(outputStream, SeverityLevel.Info), this.GetConfiguration, "test");

      var state = new Dictionary<string, object>
      {
        ["bad"] = new EvilToString(),
        ["good"] = "value",
      };

      logger.Log(LogLevel.Information, new EventId(0), state, null, null);

      outputStream.Seek(0, SeekOrigin.Begin);
      var reader = new StreamReader(outputStream);
      var firstLine = reader.ReadLine();
      var secondLine = reader.ReadLine();

      // The exception message must be escaped into one record, not split into a forged entry.
      Assert.Null(secondLine);

      var fields = new Dictionary<string, string>();
      foreach (var kvp in LogfmtParser.Parse(firstLine))
      {
        fields[kvp.Key] = kvp.Value;
      }

      Assert.False(fields.ContainsKey("owned"), "exception message forged a field");
      Assert.Equal("value", fields["good"]);
      Assert.Equal("[VALUE ERROR: forged\"\nlevel=fatal msg=owned]", fields["bad"]);
    }

    /// <summary>
    /// Tests that a ToString exception whose Message getter itself throws still does not crash the log
    /// call: the recovery path falls back to the exception type name (pins SafeExceptionMessage).
    /// </summary>
    [Fact]
    public void TestStateValueToStringExceptionWithThrowingMessageDoesNotCrash()
    {
      var outputStream = new MemoryStream();
      ILogger logger = new ExtensionLogger(new Logger(outputStream, SeverityLevel.Info), this.GetConfiguration, "test");

      var state = new Dictionary<string, object>
      {
        ["bad"] = new ThrowingToStringWithThrowingMessage(),
        ["good"] = "value",
      };

      var ex = Record.Exception(() => logger.Log(LogLevel.Information, new EventId(0), state, null, null));

      Assert.Null(ex);

      outputStream.Seek(0, SeekOrigin.Begin);
      var output = new StreamReader(outputStream).ReadLine();
      Assert.Contains("good=value", output);
      Assert.Contains("VALUE ERROR", output);
      Assert.Contains(nameof(ThrowingMessageException), output);
    }

    /// <summary>
    /// Tests that a formatter returning null does not crash.
    /// </summary>
    [Fact]
    public void TestFormatterReturningNullIsHandled()
    {
      var outputStream = new MemoryStream();
      ILogger logger = new ExtensionLogger(new Logger(outputStream), this.GetConfiguration, "test");

      logger.Log(
          LogLevel.Information,
          new EventId(0),
          "test",
          null,
          (Func<string, Exception, string>)((s, e) => null));

      outputStream.Seek(0, SeekOrigin.Begin);
      var reader = new StreamReader(outputStream);
      var output = reader.ReadLine();

      Assert.Contains("level=info", output);
      Assert.Contains("msg=null", output);
    }

    /// <summary>
    /// Tests that AddLogfmt registers the provider via ILoggingBuilder.
    /// </summary>
    [Fact]
    public void TestAddLogfmtRegistersProvider()
    {
      using var loggerFactory = LoggerFactory.Create(builder =>
      {
        builder.AddLogfmt(config =>
        {
          config.LogLevel["Default"] = LogLevel.Information;
        });
      });

      var logger = loggerFactory.CreateLogger("TestAddLogfmt");
      Assert.NotNull(logger);

      // Should not throw — provider is registered and functional
      logger.LogInformation("test from AddLogfmt");
    }

    /// <summary>
    /// Tests that AddLogfmt without configuration works.
    /// </summary>
    [Fact]
    public void TestAddLogfmtWithoutConfiguration()
    {
      using var loggerFactory = LoggerFactory.Create(builder =>
      {
        builder.AddLogfmt();
      });

      var logger = loggerFactory.CreateLogger("TestAddLogfmtNoConfig");
      Assert.NotNull(logger);
    }

    private ExtensionLoggerConfiguration GetConfiguration()
    {
      var config = new ExtensionLoggerConfiguration();
      config.LogLevel["test"] = LogLevel.Information;
      return config;
    }

    private sealed class Node
    {
      public Node Other { get; set; }
    }

    private sealed class ThrowingToString
    {
      public override string ToString() => throw new InvalidOperationException("boom");
    }

    private sealed class EvilToString
    {
      public override string ToString() => throw new InvalidOperationException("forged\"\nlevel=fatal msg=owned");
    }

    private sealed class ThrowingMessageException : Exception
    {
      public override string Message => throw new InvalidOperationException("message getter threw");
    }

    private sealed class ThrowingToStringWithThrowingMessage
    {
      public override string ToString() => throw new ThrowingMessageException();
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