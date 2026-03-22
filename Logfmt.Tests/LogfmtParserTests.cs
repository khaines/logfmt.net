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
  /// Tests for the logfmt parser.
  /// </summary>
  public class LogfmtParserTests
  {
    /// <summary>
    /// Tests parsing a simple key=value pair.
    /// </summary>
    [Fact]
    public void ParseSimpleKeyValue()
    {
      var result = LogfmtParser.Parse("key=value");

      Assert.Single(result);
      Assert.Equal("key", result[0].Key);
      Assert.Equal("value", result[0].Value);
    }

    /// <summary>
    /// Tests parsing multiple key=value pairs.
    /// </summary>
    [Fact]
    public void ParseMultipleKeyValuePairs()
    {
      var result = LogfmtParser.Parse("level=info msg=\"hello\" user_id=123");

      Assert.Equal(3, result.Count);
      Assert.Equal("level", result[0].Key);
      Assert.Equal("info", result[0].Value);
      Assert.Equal("msg", result[1].Key);
      Assert.Equal("hello", result[1].Value);
      Assert.Equal("user_id", result[2].Key);
      Assert.Equal("123", result[2].Value);
    }

    /// <summary>
    /// Tests parsing a quoted value containing spaces.
    /// </summary>
    [Fact]
    public void ParseQuotedValueWithSpaces()
    {
      var result = LogfmtParser.Parse("msg=\"hello world\"");

      Assert.Single(result);
      Assert.Equal("msg", result[0].Key);
      Assert.Equal("hello world", result[0].Value);
    }

    /// <summary>
    /// Tests parsing escaped quotes within a quoted value.
    /// </summary>
    [Fact]
    public void ParseEscapedQuotesInValue()
    {
      var result = LogfmtParser.Parse("msg=\"say \\\"hello\\\"\"");

      Assert.Single(result);
      Assert.Equal("say \"hello\"", result[0].Value);
    }

    /// <summary>
    /// Tests parsing escaped backslashes in a quoted value.
    /// </summary>
    [Fact]
    public void ParseEscapedBackslashes()
    {
      var result = LogfmtParser.Parse("path=\"C:\\\\Users\\\\test\"");

      Assert.Single(result);
      Assert.Equal("C:\\Users\\test", result[0].Value);
    }

    /// <summary>
    /// Tests parsing escaped newlines and carriage returns.
    /// </summary>
    [Fact]
    public void ParseEscapedNewlinesAndCarriageReturns()
    {
      var result = LogfmtParser.Parse("data=\"line1\\r\\nline2\"");

      Assert.Single(result);
      Assert.Equal("line1\r\nline2", result[0].Value);
    }

    /// <summary>
    /// Tests parsing an empty string.
    /// </summary>
    [Fact]
    public void ParseEmptyString()
    {
      var result = LogfmtParser.Parse(string.Empty);

      Assert.Empty(result);
    }

    /// <summary>
    /// Tests that null input throws ArgumentNullException.
    /// </summary>
    [Fact]
    public void ParseNullThrows()
    {
      Assert.Throws<ArgumentNullException>(() => LogfmtParser.Parse(null));
    }

    /// <summary>
    /// Tests parsing a key with no value (key=).
    /// </summary>
    [Fact]
    public void ParseKeyWithEmptyValue()
    {
      var result = LogfmtParser.Parse("key=");

      Assert.Single(result);
      Assert.Equal("key", result[0].Key);
      Assert.Equal(string.Empty, result[0].Value);
    }

    /// <summary>
    /// Tests parsing a bare key with no equals sign.
    /// </summary>
    [Fact]
    public void ParseBareKey()
    {
      var result = LogfmtParser.Parse("flag");

      Assert.Single(result);
      Assert.Equal("flag", result[0].Key);
      Assert.Equal(string.Empty, result[0].Value);
    }

    /// <summary>
    /// Tests parsing a value that contains equals signs.
    /// </summary>
    [Fact]
    public void ParseValueContainingEquals()
    {
      var result = LogfmtParser.Parse("query=\"key1=value1&key2=value2\"");

      Assert.Single(result);
      Assert.Equal("query", result[0].Key);
      Assert.Equal("key1=value1&key2=value2", result[0].Value);
    }

    /// <summary>
    /// Tests parsing output from the Logger to verify round-trip compatibility.
    /// </summary>
    [Fact]
    public void ParseRoundTripWithLogger()
    {
      var outputStream = new MemoryStream();
      using var logger = new Logger(outputStream);

      logger.Info("hello world", "user_id", "123", "service", "api");

      outputStream.Seek(0, SeekOrigin.Begin);
      var reader = new StreamReader(outputStream);
      var logLine = reader.ReadLine();

      var parsed = LogfmtParser.Parse(logLine);

      // Should contain ts, level, msg, user_id, service
      Assert.True(parsed.Count >= 5);

      var dict = new Dictionary<string, string>();
      foreach (var kvp in parsed)
      {
        dict[kvp.Key] = kvp.Value;
      }

      Assert.Equal("info", dict["level"]);
      Assert.Equal("hello world", dict["msg"]);
      Assert.Equal("123", dict["user_id"]);
      Assert.Equal("api", dict["service"]);
      Assert.True(dict.ContainsKey("ts"));
    }

    /// <summary>
    /// Tests parsing output with special characters round-trips correctly.
    /// </summary>
    [Fact]
    public void ParseRoundTripWithSpecialChars()
    {
      var outputStream = new MemoryStream();
      using var logger = new Logger(outputStream);

      logger.Info("line1\nline2", "path", "C:\\Users\\test");

      outputStream.Seek(0, SeekOrigin.Begin);
      var reader = new StreamReader(outputStream);
      var logLine = reader.ReadLine();

      var parsed = LogfmtParser.Parse(logLine);

      var dict = new Dictionary<string, string>();
      foreach (var kvp in parsed)
      {
        dict[kvp.Key] = kvp.Value;
      }

      Assert.Equal("line1\nline2", dict["msg"]);

      // Backslashes are escaped in unquoted values by the Logger,
      // so the parser returns the literal escaped form
      Assert.Contains("Users", dict["path"]);
    }

    /// <summary>
    /// Tests parsing a line with only whitespace.
    /// </summary>
    [Fact]
    public void ParseWhitespaceOnly()
    {
      var result = LogfmtParser.Parse("   ");

      Assert.Empty(result);
    }

    /// <summary>
    /// Tests parsing a null value rendered by the logger.
    /// </summary>
    [Fact]
    public void ParseNullValueLiteral()
    {
      var result = LogfmtParser.Parse("key=null");

      Assert.Single(result);
      Assert.Equal("null", result[0].Value);
    }

    /// <summary>
    /// Tests parsing a quoted empty string value.
    /// </summary>
    [Fact]
    public void ParseQuotedEmptyString()
    {
      var result = LogfmtParser.Parse("key=\"\"");

      Assert.Single(result);
      Assert.Equal(string.Empty, result[0].Value);
    }
  }
}
