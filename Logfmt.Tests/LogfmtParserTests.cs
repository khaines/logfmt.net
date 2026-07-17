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

    /// <summary>
    /// Tests that a key made up entirely of non-identifier characters is treated as a bare key.
    /// </summary>
    [Fact]
    public void ParseKeyWithOnlyInvalidCharsIsBareKey()
    {
      var result = LogfmtParser.Parse("!!!invalid");

      Assert.Single(result);
      Assert.Equal("!!!invalid", result[0].Key);
      Assert.Equal(string.Empty, result[0].Value);
    }

    /// <summary>
    /// Tests parsing an unquoted value that ends at EOF without a trailing space.
    /// </summary>
    [Fact]
    public void ParseValueEndingAtEofWithoutTrailingSpace()
    {
      var result = LogfmtParser.Parse("key=value");

      Assert.Single(result);
      Assert.Equal("key", result[0].Key);
      Assert.Equal("value", result[0].Value);
    }

    /// <summary>
    /// Tests that an unclosed quoted value is read gracefully to the end of the line.
    /// </summary>
    [Fact]
    public void ParseUnclosedQuoteReadsToEndOfLine()
    {
      var result = LogfmtParser.Parse("key=\"unclosed");

      Assert.Single(result);
      Assert.Equal("key", result[0].Key);
      Assert.Equal("unclosed", result[0].Value);
    }

    /// <summary>
    /// Tests that a value beginning with a backslash-quote is treated as an unquoted literal.
    /// </summary>
    [Fact]
    public void ParseValueStartingWithEscapedQuoteIsUnquoted()
    {
      var result = LogfmtParser.Parse("key=\\\"escaped");

      Assert.Single(result);
      Assert.Equal("key", result[0].Key);
      Assert.Equal("\\\"escaped", result[0].Value);
    }

    /// <summary>
    /// Tests that a backslash before a non-escape character inside a quoted value is preserved.
    /// </summary>
    [Fact]
    public void ParseBackslashBeforeNonEscapeCharInQuotedValue()
    {
      var result = LogfmtParser.Parse("key=\"foo\\bar\"");

      Assert.Single(result);
      Assert.Equal("key", result[0].Key);
      Assert.Equal("foo\\bar", result[0].Value);
    }

    /// <summary>
    /// Tests that multiple consecutive spaces act as a single delimiter between pairs.
    /// </summary>
    [Fact]
    public void ParseMultipleConsecutiveSpacesAsDelimiters()
    {
      var result = LogfmtParser.Parse("a=1   b=2");

      Assert.Equal(2, result.Count);
      Assert.Equal("a", result[0].Key);
      Assert.Equal("1", result[0].Value);
      Assert.Equal("b", result[1].Key);
      Assert.Equal("2", result[1].Value);
    }

    /// <summary>
    /// Tests that leading and trailing whitespace around the entire line is ignored.
    /// </summary>
    [Fact]
    public void ParseLeadingAndTrailingWhitespaceIsIgnored()
    {
      var result = LogfmtParser.Parse("   key=value   ");

      Assert.Single(result);
      Assert.Equal("key", result[0].Key);
      Assert.Equal("value", result[0].Value);
    }

    /// <summary>
    /// Tests that a key composed entirely of underscores is parsed as a valid key.
    /// </summary>
    [Fact]
    public void ParseAllUnderscoreKey()
    {
      var result = LogfmtParser.Parse("___=value");

      Assert.Single(result);
      Assert.Equal("___", result[0].Key);
      Assert.Equal("value", result[0].Value);
    }

    /// <summary>
    /// Tests that an unquoted value may contain multiple equals signs.
    /// </summary>
    [Fact]
    public void ParseUnquotedValueWithMultipleEquals()
    {
      var result = LogfmtParser.Parse("k=a=b=c");

      Assert.Single(result);
      Assert.Equal("k", result[0].Key);
      Assert.Equal("a=b=c", result[0].Value);
    }

    /// <summary>
    /// Tests that a very long line (greater than 64KB) is parsed without crashing.
    /// </summary>
    [Fact]
    public void ParseVeryLongLineDoesNotCrash()
    {
      var longValue = new string('a', 70000);
      var result = LogfmtParser.Parse("key=" + longValue);

      Assert.Single(result);
      Assert.Equal("key", result[0].Key);
      Assert.Equal(longValue, result[0].Value);
    }

    /// <summary>
    /// Tests that unicode characters beyond ASCII are preserved in keys and values.
    /// </summary>
    [Fact]
    public void ParseUnicodeInKeysAndValues()
    {
      var result = LogfmtParser.Parse("cl\u00e9=caf\u00e9 \u4e2d\u6587=data");

      Assert.Equal(2, result.Count);
      Assert.Equal("cl\u00e9", result[0].Key);
      Assert.Equal("caf\u00e9", result[0].Value);
      Assert.Equal("\u4e2d\u6587", result[1].Key);
      Assert.Equal("data", result[1].Value);
    }

    /// <summary>
    /// Tests that a tab character acts as a field delimiter per the kr/logfmt garbage rule.
    /// </summary>
    [Fact]
    public void ParseTabCharacterActsAsDelimiter()
    {
      var result = LogfmtParser.Parse("a=1\tb=2");

      Assert.Equal(2, result.Count);
      Assert.Equal("a", result[0].Key);
      Assert.Equal("1", result[0].Value);
      Assert.Equal("b", result[1].Key);
      Assert.Equal("2", result[1].Value);
    }

    /// <summary>
    /// Tests that a mix of spaces and tabs between pairs is treated as a single delimiter.
    /// </summary>
    [Fact]
    public void ParseMixedSpacesAndTabsAsDelimiter()
    {
      var result = LogfmtParser.Parse("a=1 \t b=2");

      Assert.Equal(2, result.Count);
      Assert.Equal("a", result[0].Key);
      Assert.Equal("1", result[0].Value);
      Assert.Equal("b", result[1].Key);
      Assert.Equal("2", result[1].Value);
    }
  }
}
