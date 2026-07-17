// Copyright (c) Ken Haines. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Logfmt.Tests
{
  using System.Collections.Generic;
  using System.IO;
  using Logfmt;
  using Xunit;

  /// <summary>
  /// Tests covering unicode handling in keys and values across the Logger and LogfmtParser.
  /// </summary>
  public class UnicodeHandlingTests
  {
    /// <summary>
    /// Tests that CJK characters in values pass through and round-trip.
    /// </summary>
    [Fact]
    public void CjkCharactersInValuesRoundTrip()
    {
      var outputStream = new MemoryStream();
      using var logger = new Logger(outputStream);

      logger.Info("greeting", "zh", "\u4e2d\u6587", "ja", "\u3053\u3093\u306b\u3061\u306f", "ko", "\uc548\ub155\ud558\uc138\uc694");

      outputStream.Seek(0, SeekOrigin.Begin);
      var output = new StreamReader(outputStream).ReadLine();
      var dict = ParseToDict(output);

      // Raw wire form is verbatim (exact space-delimited tokens), so a symmetric encode/decode
      // transform -- even a suffix -- cannot hide behind the round-trip.
      var tokens = output.Split(' ');
      Assert.Contains("zh=\u4e2d\u6587", tokens);
      Assert.Contains("ja=\u3053\u3093\u306b\u3061\u306f", tokens);
      Assert.Contains("ko=\uc548\ub155\ud558\uc138\uc694", tokens);

      Assert.Equal("\u4e2d\u6587", dict["zh"]);
      Assert.Equal("\u3053\u3093\u306b\u3061\u306f", dict["ja"]);
      Assert.Equal("\uc548\ub155\ud558\uc138\uc694", dict["ko"]);
    }

    /// <summary>
    /// Tests that an emoji with a skin-tone modifier round-trips in a value.
    /// </summary>
    [Fact]
    public void EmojiWithSkinToneInValueRoundTrips()
    {
      var outputStream = new MemoryStream();
      using var logger = new Logger(outputStream);

      var emoji = "\U0001F44D\U0001F3FD";
      logger.Info("react", "emoji", emoji);

      outputStream.Seek(0, SeekOrigin.Begin);
      var output = new StreamReader(outputStream).ReadLine();
      var dict = ParseToDict(output);

      Assert.Contains("emoji=" + emoji, output.Split(' '));
      Assert.Equal(emoji, dict["emoji"]);
    }

    /// <summary>
    /// Tests that combining diacritical marks round-trip in a value.
    /// </summary>
    [Fact]
    public void CombiningDiacriticalMarksInValueRoundTrip()
    {
      var outputStream = new MemoryStream();
      using var logger = new Logger(outputStream);

      var combined = "e\u0301\u0302n\u0303";
      logger.Info("m", "text", combined);

      outputStream.Seek(0, SeekOrigin.Begin);
      var output = new StreamReader(outputStream).ReadLine();
      var dict = ParseToDict(output);

      Assert.Contains("text=" + combined, output.Split(' '));
      Assert.Equal(combined, dict["text"]);
    }

    /// <summary>
    /// Tests that surrogate pairs round-trip in a value.
    /// </summary>
    [Fact]
    public void SurrogatePairsInValueRoundTrip()
    {
      var outputStream = new MemoryStream();
      using var logger = new Logger(outputStream);

      var surrogate = "\U0001F600\U00020BB7\U0001F680";
      logger.Info("m", "chars", surrogate);

      outputStream.Seek(0, SeekOrigin.Begin);
      var output = new StreamReader(outputStream).ReadLine();
      var dict = ParseToDict(output);

      Assert.Contains("chars=" + surrogate, output.Split(' '));
      Assert.Equal(surrogate, dict["chars"]);
    }

    /// <summary>
    /// Tests that unicode characters in keys are converted to underscores by the Logger.
    /// </summary>
    [Fact]
    public void UnicodeKeysAreConvertedToUnderscores()
    {
      var outputStream = new MemoryStream();
      using var logger = new Logger(outputStream);

      logger.Info("m", "user\u00e9", "v1", "\U0001F600key", "v2");

      outputStream.Seek(0, SeekOrigin.Begin);
      var output = new StreamReader(outputStream).ReadLine();
      var dict = ParseToDict(output);

      // Exact key names pin the sanitization (and its consecutive-underscore collapse), so a
      // double-underscore ("__key") or value-corruption mutation cannot survive a Contains substring.
      Assert.Equal("v1", dict["user_"]);
      Assert.Equal("v2", dict["_key"]);
      Assert.False(dict.ContainsKey("user\u00e9"));
      Assert.DoesNotContain("\u00e9", output);
      Assert.DoesNotContain("\U0001F600", output);
    }

    /// <summary>
    /// Tests that a key consisting entirely of unicode collapses to a single underscore.
    /// </summary>
    [Fact]
    public void FullyUnicodeKeyIsConvertedToSingleUnderscore()
    {
      var outputStream = new MemoryStream();
      using var logger = new Logger(outputStream);

      logger.Info("m", "\u4e2d\u6587", "v");

      outputStream.Seek(0, SeekOrigin.Begin);
      var output = new StreamReader(outputStream).ReadLine();
      var dict = ParseToDict(output);

      Assert.Equal("v", dict["_"]);
      Assert.False(dict.ContainsKey("\u4e2d\u6587"));
      Assert.DoesNotContain("\u4e2d", output);
    }

    /// <summary>
    /// Tests that the parser handles unicode values directly, both quoted and unquoted.
    /// </summary>
    [Fact]
    public void ParserHandlesUnicodeValuesDirectly()
    {
      var dict = ParseToDict("emoji=\U0001F600 cjk=\u4e2d\u6587 quoted=\"caf\u00e9 \u4e2d\u6587\"");

      Assert.Equal("\U0001F600", dict["emoji"]);
      Assert.Equal("\u4e2d\u6587", dict["cjk"]);
      Assert.Equal("caf\u00e9 \u4e2d\u6587", dict["quoted"]);
    }

    /// <summary>
    /// Tests that a message and values combining several unicode categories round-trip through the logger and parser.
    /// </summary>
    [Fact]
    public void MixedUnicodeRoundTripsThroughLoggerAndParser()
    {
      var outputStream = new MemoryStream();
      using var logger = new Logger(outputStream);

      logger.Info("\u4e2d\u6587 message \U0001F600", "emoji", "\U0001F44D\U0001F3FD", "accent", "caf\u00e9", "cjk", "\u65e5\u672c\u8a9e");

      outputStream.Seek(0, SeekOrigin.Begin);
      var output = new StreamReader(outputStream).ReadLine();
      var dict = ParseToDict(output);

      Assert.Equal("\u4e2d\u6587 message \U0001F600", dict["msg"]);
      Assert.Equal("\U0001F44D\U0001F3FD", dict["emoji"]);
      Assert.Equal("caf\u00e9", dict["accent"]);
      Assert.Equal("\u65e5\u672c\u8a9e", dict["cjk"]);
    }

    /// <summary>
    /// Tests that logging unicode content in bulk produces correct output for every entry.
    /// Real throughput measurement belongs to the benchmark suite.
    /// </summary>
    [Fact]
    public void BulkUnicodeLoggingProducesCorrectOutput()
    {
      var outputStream = new MemoryStream();
      using var logger = new Logger(outputStream);

      for (int i = 0; i < 500; i++)
      {
        logger.Info("\u4e2d\u6587", "emoji", "\U0001F600", "seq", $"{i}");
      }

      outputStream.Seek(0, SeekOrigin.Begin);
      var reader = new StreamReader(outputStream);
      var count = 0;
      string line;
      while ((line = reader.ReadLine()) != null)
      {
        var dict = ParseToDict(line);
        Assert.Equal("\u4e2d\u6587", dict["msg"]);
        Assert.Equal("\U0001F600", dict["emoji"]);
        Assert.Equal($"{count}", dict["seq"]);
        count++;
      }

      Assert.Equal(500, count);
    }

    /// <summary>
    /// Tests that unicode pseudo-separators in a value cannot break the record boundary or inject a
    /// forged field, because the logfmt encoder and parser only treat ASCII whitespace as a delimiter.
    /// </summary>
    [Fact]
    public void UnicodeSeparatorsInValueDoNotBreakRecordOrInjectField()
    {
      foreach (var separator in new[] { "\u2028", "\u2029", "\u0085", "\u00a0", "\u2003" })
      {
        var outputStream = new MemoryStream();
        using var logger = new Logger(outputStream);

        var value = "a" + separator + "injected=evil";
        logger.Info("m", "k", value);

        outputStream.Seek(0, SeekOrigin.Begin);
        var reader = new StreamReader(outputStream);
        var firstLine = reader.ReadLine();
        var secondLine = reader.ReadLine();

        // Exactly one record: the unicode separator did not start a new line.
        Assert.Null(secondLine);

        // The separator is not a delimiter: the value stays intact in one field, with no forged key.
        var dict = ParseToDict(firstLine);
        Assert.Equal(value, dict["k"]);
        Assert.False(dict.ContainsKey("injected"));
      }
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
