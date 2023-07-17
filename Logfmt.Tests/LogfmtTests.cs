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
      var logger = new Logger(outputStream).WithData(new KeyValuePair<string, string>("module", "foo"));

      // write a log entry
      logger.Info("hello logs!");

      outputStream.Seek(0, SeekOrigin.Begin);
      var reader = new StreamReader(outputStream);

      var output = reader.ReadLine();

      Assert.Contains("level=info msg=\"hello logs!\" module=foo", output);
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
      catch (Exception e)
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
      var logger = new Logger(outputStream);

      // write a log entry, but use a KVPair key containing a space. The spaces should be replaced with an underscore
      logger.Log(SeverityLevel.Info, "hello logs!", "not valid key", "blue");

      outputStream.Seek(0, SeekOrigin.Begin);
      var reader = new StreamReader(outputStream);

      var output = reader.ReadLine();

      // validate the output.
      Assert.Contains(
          "level=info msg=\"hello logs!\" not_valid_key=blue",
          output);
    }

    /// <summary>
    /// Testing debug output mode.
    /// </summary>
    [Fact]
    public void LogDebugOutput()
    {
      var outputStream = new MemoryStream();
      var logger = new Logger(outputStream, SeverityLevel.Debug);

      // write a log entry
      logger.Debug(msg: "hello logs!");

      outputStream.Seek(0, SeekOrigin.Begin);
      var reader = new StreamReader(outputStream);

      var output = reader.ReadLine();

      Assert.Contains("level=debug msg=\"hello logs!\"", output);
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
  }
}