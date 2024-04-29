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
        ["msg"] = "test message",
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
                new KeyValuePair<string, object>("msg", "test message"),
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

    private ExtensionLoggerConfiguration GetConfiguration()
    {
      var config = new ExtensionLoggerConfiguration();
      config.LogLevel["test"] = LogLevel.Information;
      return config;
    }
  }
}