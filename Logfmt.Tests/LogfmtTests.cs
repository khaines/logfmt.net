// Copyright (c) Ken Haines. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Logfmt.Tests
{
  using System;
  using System.Collections.Generic;
  using System.IO;
  using Logfmt;
  using Xunit;

  /// <summary>
  /// Test class.
  /// </summary>
  public class LogfmtTests
  {
    /// <summary>
    /// Test for creating a basic logger and validating that fields are formatted correctly.
    /// </summary>
    [Fact]
    public void CreateLoggerWithDefaultFieldsTest()
    {
      var outputStream = new MemoryStream();
      using var logger = new Logger(outputStream).WithData(new KeyValuePair<string, string>("module", "foo"));

      // write a log entry
      logger.Info("hello logs!");

      outputStream.Seek(0, SeekOrigin.Begin);
      using var reader = new StreamReader(outputStream);

      var output = reader.ReadLine();

      Assert.Contains("level=info msg=\"hello logs!\" module=foo", output, StringComparison.InvariantCultureIgnoreCase);
    }

    /// <summary>
    /// A test that expects no exception if the log is sent to a closed stream.
    /// </summary>
    [Fact]
    public void ExpectNoExceptionOnClosedStream()
    {
      var outputStream = new MemoryStream();
      var logger = new Logger(outputStream);

      outputStream.Close();
      outputStream.Dispose();
      try
      {
        // write a log entry. The stream is closed/disposed, but this shouldn't result in an unhandled exception to the caller
        logger.Info("hello logs!");
      }
      catch (ObjectDisposedException e)
      {
        Assert.Null(e);
      }
    }

    /// <summary>
    /// Testing for escaping of invalid characters in a "key" parameter.
    /// </summary>
    [Fact]
    public void TestInvalidKeyEscaping()
    {
      var outputStream = new MemoryStream();
      using var logger = new Logger(outputStream);

      // write a log entry, but use a KVPair key containing a space. The spaces should be replaced with an underscore
      logger.Log(SeverityLevel.Info, "hello logs!", "not valid key", "blue");

      outputStream.Seek(0, SeekOrigin.Begin);
      using var reader = new StreamReader(outputStream);

      var output = reader.ReadLine();

      // validate the output.
      Assert.Contains(
          "level=info msg=\"hello logs!\" not_valid_key=blue",
          output,
          StringComparison.InvariantCultureIgnoreCase);
    }

    /// <summary>
    /// Testing debug output mode.
    /// </summary>
    [Fact]
    public void LogDebugOutput()
    {
      var outputStream = new MemoryStream();
      using var logger = new Logger(outputStream, SeverityLevel.Debug);

      // write a log entry
      logger.Debug(msg: "hello logs!");

      outputStream.Seek(0, SeekOrigin.Begin);
      using var reader = new StreamReader(outputStream);

      var output = reader.ReadLine();

      Assert.Contains("level=debug msg=\"hello logs!\"", output, StringComparison.InvariantCultureIgnoreCase);
    }

    /// <summary>
    /// Tests Error output.
    /// </summary>
    [Fact]
    public void LogErrorOutput()
    {
      var outputStream = new MemoryStream();
      var logger = new Logger(outputStream);

      // write a log entry
      logger.Error("hello logs!");

      outputStream.Seek(0, SeekOrigin.Begin);
      var reader = new StreamReader(outputStream);

      var output = reader.ReadLine();

      Assert.Contains("level=error msg=\"hello logs!\"", output);
    }

    /// <summary>
    /// Tests info output.
    /// </summary>
    [Fact]
    public void LogInfoOutput()
    {
      var outputStream = new MemoryStream();
      var logger = new Logger(outputStream);

      // write a log entry
      logger.Info("hello logs!");

      outputStream.Seek(0, SeekOrigin.Begin);
      var reader = new StreamReader(outputStream);

      var output = reader.ReadLine();

      Assert.Contains("level=info msg=\"hello logs!\"", output);
    }

    /// <summary>
    /// Tests warning output.
    /// </summary>
    [Fact]
    public void LogIWarnOutput()
    {
      var outputStream = new MemoryStream();
      var logger = new Logger(outputStream);

      // write a log entry
      logger.Warn("hello logs!");

      outputStream.Seek(0, SeekOrigin.Begin);
      var reader = new StreamReader(outputStream);

      var output = reader.ReadLine();

      Assert.Contains("level=warn msg=\"hello logs!\"", output);
    }

    /// <summary>
    /// Tests output of log entries that contain collection of k/v parameters.
    /// </summary>
    [Fact]
    public void LogOutputWithKVPairs()
    {
      var outputStream = new MemoryStream();
      var logger = new Logger(outputStream);

      // write a log entry
      logger.Info("hello logs!", "color", "blue", "country", "United States");

      outputStream.Seek(0, SeekOrigin.Begin);
      var reader = new StreamReader(outputStream);

      var output = reader.ReadLine();

      Assert.Contains("msg=\"hello logs!\"", output);
      Assert.Contains("color=blue", output);
      Assert.Contains("country=\"United States\"", output);
    }

    /// <summary>
    /// Tests the severity filter so that only Info and above messages are logged.
    /// </summary>
    [Fact]
    public void TestInfoSeverityFilter()
    {
      var outputStream = new MemoryStream();
      var logger = new Logger(outputStream, SeverityLevel.Info);

      // this entry should be filtered out.
      logger.Debug("This is a debug line", "color", "blue", "country", "United States");

      // write a log entry
      logger.Info("hello logs!", "color", "blue", "country", "United States");

      outputStream.Seek(0, SeekOrigin.Begin);
      var reader = new StreamReader(outputStream);

      var output = reader.ReadLine();

      Assert.Contains("msg=\"hello logs!\"", output);
      Assert.Contains("color=blue", output);
      Assert.Contains("country=\"United States\"", output);
    }

    /// <summary>
    /// Testing debug output mode with default fields.
    /// </summary>
    [Fact]
    public void LogDebugOutputWithDefaultFieldsTest()
    {
      var outputStream = new MemoryStream();
      var logger = new Logger(outputStream, SeverityLevel.Debug).WithData(new KeyValuePair<string, string>("module", "foo"));

      // write a log entry
      logger.Debug(msg: "hello logs!");

      outputStream.Seek(0, SeekOrigin.Begin);
      var reader = new StreamReader(outputStream);

      var output = reader.ReadLine();

      Assert.Contains("level=debug msg=\"hello logs!\" module=foo", output);
    }

    /// <summary>
    /// Tests that line breaks and tabs in the output are escaped.
    /// </summary>
    [Fact]
    public void EscapeLineBreaksAndTabsInOutputTest()
    {
      var outputStream = new MemoryStream();
      using var logger = new Logger(outputStream, SeverityLevel.Info);

      var jsonMsg = @"{
          'foo':'bar',
          'test':'true'
      }";

      logger.Info(msg: jsonMsg);

      outputStream.Seek(0, SeekOrigin.Begin);
      using var reader = new StreamReader(outputStream);

      var output = reader.ReadLine();

      var expected = "level=info msg=\"{\\n          'foo':'bar',\\n          'test':'true'\\n      }\"";
      Assert.Contains(expected, output);
    }

    /// <summary>
    /// Tests that SetSeverityFilter changes the logger's filtering behavior.
    /// </summary>
    [Fact]
    public void SetSeverityFilterChangesLogging()
    {
      var outputStream = new MemoryStream();
      var logger = new Logger(outputStream, SeverityLevel.Info);

      // Info should be logged initially
      logger.Info("first message");

      // Change filter to Error — Info should now be filtered
      logger.SetSeverityFilter(SeverityLevel.Error);
      logger.Info("this should not appear");
      logger.Error("second message");

      outputStream.Seek(0, SeekOrigin.Begin);
      var reader = new StreamReader(outputStream);

      var line1 = reader.ReadLine();
      var line2 = reader.ReadLine();

      Assert.Contains("msg=\"first message\"", line1);
      Assert.Contains("level=error", line2);
      Assert.Contains("msg=\"second message\"", line2);
    }

    /// <summary>
    /// Tests WithData using the string-pairs overload.
    /// </summary>
    [Fact]
    public void WithDataStringPairsOverload()
    {
      var outputStream = new MemoryStream();
      using var logger = new Logger(outputStream).WithData("service", "api");

      logger.Info("test");

      outputStream.Seek(0, SeekOrigin.Begin);
      var reader = new StreamReader(outputStream);
      var output = reader.ReadLine();

      Assert.Contains("service=api", output);
    }

    /// <summary>
    /// Tests chaining multiple WithData calls accumulates all fields.
    /// </summary>
    [Fact]
    public void WithDataChainingAccumulatesFields()
    {
      var outputStream = new MemoryStream();
      using var logger = new Logger(outputStream)
          .WithData("service", "api")
          .WithData("env", "prod")
          .WithData("version", "1.0");

      logger.Info("test");

      outputStream.Seek(0, SeekOrigin.Begin);
      var reader = new StreamReader(outputStream);
      var output = reader.ReadLine();

      Assert.Contains("service=api", output);
      Assert.Contains("env=prod", output);
      Assert.Contains("version=1.0", output);
    }

    /// <summary>
    /// Tests that an odd-length string array throws ArgumentException.
    /// </summary>
    [Fact]
    public void OddLengthKvpairsThrowsArgumentException()
    {
      var outputStream = new MemoryStream();
      using var logger = new Logger(outputStream);

      Assert.Throws<ArgumentException>(() => logger.Log(SeverityLevel.Info, "msg", "key_without_value"));
    }

    /// <summary>
    /// Tests that an odd-length array in WithData throws ArgumentException.
    /// </summary>
    [Fact]
    public void WithDataOddLengthThrowsArgumentException()
    {
      var outputStream = new MemoryStream();
      using var logger = new Logger(outputStream);

      Assert.Throws<ArgumentException>(() => logger.WithData("key_without_value"));
    }

    /// <summary>
    /// Tests that null values in key-value pairs are rendered as "null".
    /// </summary>
    [Fact]
    public void NullValueInKvpairRenderedAsNull()
    {
      var outputStream = new MemoryStream();
      using var logger = new Logger(outputStream);

      logger.Log(SeverityLevel.Info, new KeyValuePair<string, string>("key", null));

      outputStream.Seek(0, SeekOrigin.Begin);
      var reader = new StreamReader(outputStream);
      var output = reader.ReadLine();

      Assert.Contains("key=null", output);
    }

    /// <summary>
    /// Tests that tab characters in values cause quoting.
    /// </summary>
    [Fact]
    public void TabCharacterInValueCausesQuoting()
    {
      var outputStream = new MemoryStream();
      using var logger = new Logger(outputStream);

      logger.Info("test", "data", "hello\tworld");

      outputStream.Seek(0, SeekOrigin.Begin);
      var reader = new StreamReader(outputStream);
      var output = reader.ReadLine();

      Assert.Contains("data=\"hello\tworld\"", output);
    }

    /// <summary>
    /// Tests that double quotes in values are escaped.
    /// </summary>
    [Fact]
    public void QuotesInValueAreEscaped()
    {
      var outputStream = new MemoryStream();
      using var logger = new Logger(outputStream);

      logger.Info("test", "data", "say \"hello\"");

      outputStream.Seek(0, SeekOrigin.Begin);
      var reader = new StreamReader(outputStream);
      var output = reader.ReadLine();

      Assert.Contains("data=\"say \\\"hello\\\"\"", output);
    }

    /// <summary>
    /// Tests that carriage returns in values are escaped.
    /// </summary>
    [Fact]
    public void CarriageReturnInValueIsEscaped()
    {
      var outputStream = new MemoryStream();
      using var logger = new Logger(outputStream);

      logger.Info("test", "data", "line1\r\nline2");

      outputStream.Seek(0, SeekOrigin.Begin);
      var reader = new StreamReader(outputStream);
      var output = reader.ReadLine();

      Assert.Contains("data=line1\\r\\nline2", output);
    }

    /// <summary>
    /// Tests that empty key-value pairs in kvpairs are skipped.
    /// </summary>
    [Fact]
    public void EmptyKeyIsSkipped()
    {
      var outputStream = new MemoryStream();
      using var logger = new Logger(outputStream);

      logger.Log(
          SeverityLevel.Info,
          new KeyValuePair<string, string>("valid", "yes"),
          new KeyValuePair<string, string>("  ", "skip"),
          new KeyValuePair<string, string>("also_valid", "yes"));

      outputStream.Seek(0, SeekOrigin.Begin);
      var reader = new StreamReader(outputStream);
      var output = reader.ReadLine();

      Assert.Contains("valid=yes", output);
      Assert.Contains("also_valid=yes", output);
      Assert.DoesNotContain("skip", output);
    }

    /// <summary>
    /// Tests that all severity levels produce the correct lowercase string.
    /// </summary>
    [Fact]
    public void AllSeverityLevelsProduceCorrectOutput()
    {
      var levels = new[] { SeverityLevel.Trace, SeverityLevel.Debug, SeverityLevel.Info, SeverityLevel.Warn, SeverityLevel.Error, SeverityLevel.Fatal };
      var expected = new[] { "trace", "debug", "info", "warn", "error", "fatal" };

      for (int i = 0; i < levels.Length; i++)
      {
        var outputStream = new MemoryStream();
        var logger = new Logger(outputStream, SeverityLevel.Trace);

        logger.Log(levels[i], "test");

        outputStream.Seek(0, SeekOrigin.Begin);
        var reader = new StreamReader(outputStream);
        var output = reader.ReadLine();

        Assert.Contains($"level={expected[i]}", output);
      }
    }

    /// <summary>
    /// Tests that concurrent logging from multiple threads does not corrupt output.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async System.Threading.Tasks.Task ConcurrentLoggingDoesNotCorruptOutput()
    {
      var outputStream = new MemoryStream();
      var logger = new Logger(outputStream, SeverityLevel.Info);

      var tasks = new System.Threading.Tasks.Task[10];
      for (int i = 0; i < tasks.Length; i++)
      {
        var index = i;
        tasks[i] = System.Threading.Tasks.Task.Run(() =>
        {
          for (int j = 0; j < 50; j++)
          {
            logger.Info($"thread {index} message {j}");
          }
        });
      }

      await System.Threading.Tasks.Task.WhenAll(tasks);

      outputStream.Seek(0, SeekOrigin.Begin);
      var reader = new StreamReader(outputStream);
      var lines = new List<string>();
      string line;
      while ((line = reader.ReadLine()) != null)
      {
        lines.Add(line);
      }

      // All 500 messages should be present
      Assert.Equal(500, lines.Count);

      // Each line should be well-formed with ts= and level=
      foreach (var l in lines)
      {
        Assert.Contains("ts=", l);
        Assert.Contains("level=info", l);
      }
    }

    /// <summary>
    /// Tests concurrent logging with WithData-derived loggers sharing the same stream.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async System.Threading.Tasks.Task ConcurrentWithDataLoggersShareStream()
    {
      var outputStream = new MemoryStream();
      var baseLogger = new Logger(outputStream, SeverityLevel.Info);
      var logger1 = baseLogger.WithData("source", "logger1");
      var logger2 = baseLogger.WithData("source", "logger2");

      var tasks = new[]
      {
        System.Threading.Tasks.Task.Run(() =>
        {
          for (int i = 0; i < 100; i++)
          {
            logger1.Info($"msg {i}");
          }
        }),
        System.Threading.Tasks.Task.Run(() =>
        {
          for (int i = 0; i < 100; i++)
          {
            logger2.Info($"msg {i}");
          }
        }),
      };

      await System.Threading.Tasks.Task.WhenAll(tasks);

      outputStream.Seek(0, SeekOrigin.Begin);
      var reader = new StreamReader(outputStream);
      var lines = new List<string>();
      string line;
      while ((line = reader.ReadLine()) != null)
      {
        lines.Add(line);
      }

      Assert.Equal(200, lines.Count);

      foreach (var l in lines)
      {
        Assert.Contains("ts=", l);
        Assert.Contains("level=info", l);
      }
    }

    /// <summary>
    /// Tests IsEnabled returns true for levels at or above the filter.
    /// </summary>
    [Fact]
    public void IsEnabledBoundaryCheck()
    {
      var logger = new Logger(Stream.Null, SeverityLevel.Warn);

      Assert.False(logger.IsEnabled(SeverityLevel.Trace));
      Assert.False(logger.IsEnabled(SeverityLevel.Debug));
      Assert.False(logger.IsEnabled(SeverityLevel.Info));
      Assert.True(logger.IsEnabled(SeverityLevel.Warn));
      Assert.True(logger.IsEnabled(SeverityLevel.Error));
      Assert.True(logger.IsEnabled(SeverityLevel.Fatal));
    }

    /// <summary>
    /// Tests that SeverityLevel.Off disables all logging.
    /// </summary>
    [Fact]
    public void SeverityLevelOffDisablesAllLogging()
    {
      var outputStream = new MemoryStream();
      var logger = new Logger(outputStream, SeverityLevel.Off);

      logger.Log(SeverityLevel.Trace, "should not appear");
      logger.Log(SeverityLevel.Debug, "should not appear");
      logger.Log(SeverityLevel.Info, "should not appear");
      logger.Log(SeverityLevel.Warn, "should not appear");
      logger.Log(SeverityLevel.Error, "should not appear");
      logger.Log(SeverityLevel.Fatal, "should not appear");

      outputStream.Seek(0, SeekOrigin.Begin);
      var reader = new StreamReader(outputStream);
      Assert.Null(reader.ReadLine());
    }

    /// <summary>
    /// Tests IsEnabled returns false for all levels when filter is Off.
    /// </summary>
    [Fact]
    public void IsEnabledReturnsFalseWhenFilterIsOff()
    {
      var logger = new Logger(Stream.Null, SeverityLevel.Off);

      Assert.False(logger.IsEnabled(SeverityLevel.Trace));
      Assert.False(logger.IsEnabled(SeverityLevel.Debug));
      Assert.False(logger.IsEnabled(SeverityLevel.Info));
      Assert.False(logger.IsEnabled(SeverityLevel.Warn));
      Assert.False(logger.IsEnabled(SeverityLevel.Error));
      Assert.False(logger.IsEnabled(SeverityLevel.Fatal));
      Assert.False(logger.IsEnabled(SeverityLevel.Off));
    }

    /// <summary>
    /// Tests that backslashes in values are escaped.
    /// </summary>
    [Fact]
    public void BackslashInValueIsEscaped()
    {
      var outputStream = new MemoryStream();
      using var logger = new Logger(outputStream);

      logger.Info("test", "path", "C:\\Users\\test");

      outputStream.Seek(0, SeekOrigin.Begin);
      var reader = new StreamReader(outputStream);
      var output = reader.ReadLine();

      Assert.Contains("path=C:\\\\Users\\\\test", output);
    }

    /// <summary>
    /// Tests that empty string values are output correctly.
    /// </summary>
    [Fact]
    public void EmptyStringValueIsOutput()
    {
      var outputStream = new MemoryStream();
      using var logger = new Logger(outputStream);

      logger.Log(SeverityLevel.Info, new KeyValuePair<string, string>("key", string.Empty));

      outputStream.Seek(0, SeekOrigin.Begin);
      var reader = new StreamReader(outputStream);
      var output = reader.ReadLine();

      Assert.Contains("key=", output);
    }

    /// <summary>
    /// Tests that unicode characters in keys are replaced with underscores.
    /// </summary>
    [Fact]
    public void UnicodeInKeyIsReplacedWithUnderscore()
    {
      var outputStream = new MemoryStream();
      using var logger = new Logger(outputStream);

      logger.Log(SeverityLevel.Info, new KeyValuePair<string, string>("café", "latte"));

      outputStream.Seek(0, SeekOrigin.Begin);
      var reader = new StreamReader(outputStream);
      var output = reader.ReadLine();

      Assert.Contains("caf_=latte", output);
    }

    /// <summary>
    /// Tests that unicode/emoji in values passes through correctly.
    /// </summary>
    [Fact]
    public void UnicodeEmojiInValuePassesThrough()
    {
      var outputStream = new MemoryStream();
      using var logger = new Logger(outputStream);

      logger.Info("test", "emoji", "🚀");

      outputStream.Seek(0, SeekOrigin.Begin);
      var reader = new StreamReader(outputStream);
      var output = reader.ReadLine();

      Assert.Contains("emoji=🚀", output);
    }

    /// <summary>
    /// Tests that values containing equals signs are output correctly.
    /// </summary>
    [Fact]
    public void ValueContainingEqualsSign()
    {
      var outputStream = new MemoryStream();
      using var logger = new Logger(outputStream);

      logger.Info("test", "query", "key1=value1");

      outputStream.Seek(0, SeekOrigin.Begin);
      var reader = new StreamReader(outputStream);
      var output = reader.ReadLine();

      Assert.Contains("query=key1=value1", output);
    }

    /// <summary>
    /// Tests that consecutive special characters in keys collapse to a single underscore.
    /// </summary>
    [Fact]
    public void ConsecutiveSpecialCharsInKeyCollapseToSingleUnderscore()
    {
      var outputStream = new MemoryStream();
      using var logger = new Logger(outputStream);

      logger.Log(SeverityLevel.Info, new KeyValuePair<string, string>("@@@invalid@@@", "value"));

      outputStream.Seek(0, SeekOrigin.Begin);
      var reader = new StreamReader(outputStream);
      var output = reader.ReadLine();

      Assert.Contains("_invalid_=value", output);
    }

    /// <summary>
    /// Tests that values with mixed special characters are all escaped correctly.
    /// </summary>
    [Fact]
    public void MixedSpecialCharsInValueAllEscaped()
    {
      var outputStream = new MemoryStream();
      using var logger = new Logger(outputStream);

      logger.Info("test", "data", "line1\r\nline2\t\"quoted\"\\path");

      outputStream.Seek(0, SeekOrigin.Begin);
      var reader = new StreamReader(outputStream);
      var output = reader.ReadLine();

      Assert.Contains("data=\"line1\\r\\nline2\t\\\"quoted\\\"\\\\path\"", output);
    }

    /// <summary>
    /// Tests that WithData with an empty array does not affect output.
    /// </summary>
    [Fact]
    public void WithDataEmptyArrayNoEffect()
    {
      var outputStream = new MemoryStream();
      using var logger = new Logger(outputStream).WithData(Array.Empty<KeyValuePair<string, string>>());

      logger.Info("test");

      outputStream.Seek(0, SeekOrigin.Begin);
      var reader = new StreamReader(outputStream);
      var output = reader.ReadLine();

      Assert.Contains("msg=\"test\"", output);
      Assert.DoesNotContain("=  ", output);
    }

    /// <summary>
    /// Tests that a null message throws ArgumentNullException.
    /// </summary>
    [Fact]
    public void NullMessageThrowsArgumentNullException()
    {
      var outputStream = new MemoryStream();
      using var logger = new Logger(outputStream);

      Assert.Throws<ArgumentNullException>(() => logger.Log(SeverityLevel.Info, (string)null));
    }

    /// <summary>
    /// Tests that a key consisting only of underscores is valid.
    /// </summary>
    [Fact]
    public void AllUnderscoreKeyIsValid()
    {
      var outputStream = new MemoryStream();
      using var logger = new Logger(outputStream);

      logger.Log(SeverityLevel.Info, new KeyValuePair<string, string>("___", "value"));

      outputStream.Seek(0, SeekOrigin.Begin);
      var reader = new StreamReader(outputStream);
      var output = reader.ReadLine();

      Assert.Contains("___=value", output);
    }

    /// <summary>
    /// Tests logging with typed integer values.
    /// </summary>
    [Fact]
    public void LogWithTypedIntegerValues()
    {
      var outputStream = new MemoryStream();
      using var logger = new Logger(outputStream);

      logger.Log(SeverityLevel.Info, "request", "status", 200, "duration_ms", 42);

      outputStream.Seek(0, SeekOrigin.Begin);
      var reader = new StreamReader(outputStream);
      var output = reader.ReadLine();

      Assert.Contains("status=200", output);
      Assert.Contains("duration_ms=42", output);
    }

    /// <summary>
    /// Tests logging with typed boolean values.
    /// </summary>
    [Fact]
    public void LogWithTypedBooleanValues()
    {
      var outputStream = new MemoryStream();
      using var logger = new Logger(outputStream);

      logger.Log(SeverityLevel.Info, "config", "debug", true, "verbose", false);

      outputStream.Seek(0, SeekOrigin.Begin);
      var reader = new StreamReader(outputStream);
      var output = reader.ReadLine();

      Assert.Contains("debug=True", output);
      Assert.Contains("verbose=False", output);
    }

    /// <summary>
    /// Tests logging with mixed typed values via extension method.
    /// </summary>
    [Fact]
    public void InfoWithTypedValues()
    {
      var outputStream = new MemoryStream();
      using var logger = new Logger(outputStream);

      logger.Info("test", "count", 5, "rate", 3.14);

      outputStream.Seek(0, SeekOrigin.Begin);
      var reader = new StreamReader(outputStream);
      var output = reader.ReadLine();

      Assert.Contains("count=5", output);
      Assert.Contains("rate=3.14", output);
    }

    /// <summary>
    /// Tests that null typed values are rendered as null.
    /// </summary>
    [Fact]
    public void LogWithNullTypedValue()
    {
      var outputStream = new MemoryStream();
      using var logger = new Logger(outputStream);

      logger.Log(SeverityLevel.Info, "test", "key", (object)null);

      outputStream.Seek(0, SeekOrigin.Begin);
      var reader = new StreamReader(outputStream);
      var output = reader.ReadLine();

      Assert.Contains("key=null", output);
    }

    /// <summary>
    /// Tests that typed values are filtered with zero cost when level is disabled.
    /// </summary>
    [Fact]
    public void TypedValuesFilteredWhenDisabled()
    {
      var outputStream = new MemoryStream();
      using var logger = new Logger(outputStream, SeverityLevel.Error);

      logger.Info("should not log", "count", 42);

      outputStream.Seek(0, SeekOrigin.Begin);
      var reader = new StreamReader(outputStream);
      Assert.Null(reader.ReadLine());
    }

    /// <summary>
    /// Tests logging with DateTime typed values.
    /// </summary>
    [Fact]
    public void LogWithTypedDateTimeValue()
    {
      var outputStream = new MemoryStream();
      using var logger = new Logger(outputStream);
      var dt = new DateTime(2026, 3, 22, 12, 0, 0, DateTimeKind.Utc);

      logger.Log(SeverityLevel.Info, "event", "occurred_at", dt);

      outputStream.Seek(0, SeekOrigin.Begin);
      var reader = new StreamReader(outputStream);
      var output = reader.ReadLine();

      Assert.Contains("occurred_at=", output);
      Assert.Contains("2026", output);
    }

    /// <summary>
    /// Tests logging with enum typed values.
    /// </summary>
    [Fact]
    public void LogWithTypedEnumValue()
    {
      var outputStream = new MemoryStream();
      using var logger = new Logger(outputStream);

      logger.Log(SeverityLevel.Info, "test", "level", SeverityLevel.Warn);

      outputStream.Seek(0, SeekOrigin.Begin);
      var reader = new StreamReader(outputStream);
      var output = reader.ReadLine();

      Assert.Contains("level=Warn", output);
    }

    /// <summary>
    /// Tests that odd-length object array throws ArgumentException.
    /// </summary>
    [Fact]
    public void TypedValuesOddLengthThrows()
    {
      var outputStream = new MemoryStream();
      using var logger = new Logger(outputStream);

      Assert.Throws<ArgumentException>(() =>
          logger.Log(SeverityLevel.Info, "test", new object[] { "key_only" }));
    }

    /// <summary>
    /// Tests typed Debug extension method.
    /// </summary>
    [Fact]
    public void DebugWithTypedValues()
    {
      var outputStream = new MemoryStream();
      using var logger = new Logger(outputStream, SeverityLevel.Debug);

      logger.Debug("test", "count", 10);

      outputStream.Seek(0, SeekOrigin.Begin);
      var reader = new StreamReader(outputStream);
      var output = reader.ReadLine();

      Assert.Contains("level=debug", output);
      Assert.Contains("count=10", output);
    }

    /// <summary>
    /// Tests typed Warn extension method.
    /// </summary>
    [Fact]
    public void WarnWithTypedValues()
    {
      var outputStream = new MemoryStream();
      using var logger = new Logger(outputStream);

      logger.Warn("test", "retries", 3);

      outputStream.Seek(0, SeekOrigin.Begin);
      var reader = new StreamReader(outputStream);
      var output = reader.ReadLine();

      Assert.Contains("level=warn", output);
      Assert.Contains("retries=3", output);
    }

    /// <summary>
    /// Tests typed Error extension method.
    /// </summary>
    [Fact]
    public void ErrorWithTypedValues()
    {
      var outputStream = new MemoryStream();
      using var logger = new Logger(outputStream);

      logger.Error("failed", "code", 500);

      outputStream.Seek(0, SeekOrigin.Begin);
      var reader = new StreamReader(outputStream);
      var output = reader.ReadLine();

      Assert.Contains("level=error", output);
      Assert.Contains("code=500", output);
    }

    /// <summary>
    /// Tests mixing string and object values produces same output.
    /// </summary>
    [Fact]
    public void TypedAndStringOverloadsProduceSameOutput()
    {
      var stringStream = new MemoryStream();
      using var stringLogger = new Logger(stringStream);
      stringLogger.Log(SeverityLevel.Info, "test", "count", "42");

      var typedStream = new MemoryStream();
      using var typedLogger = new Logger(typedStream);
      typedLogger.Log(SeverityLevel.Info, "test", "count", 42);

      stringStream.Seek(0, SeekOrigin.Begin);
      typedStream.Seek(0, SeekOrigin.Begin);

      var stringOutput = new StreamReader(stringStream).ReadLine();
      var typedOutput = new StreamReader(typedStream).ReadLine();

      // Both should contain count=42 (timestamps will differ)
      Assert.Contains("count=42", stringOutput);
      Assert.Contains("count=42", typedOutput);
    }
  }
}