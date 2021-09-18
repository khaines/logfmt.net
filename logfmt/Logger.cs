// Copyright (c) Ken Haines. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Logfmt
{
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Linq;
  using System.Reflection.Metadata.Ecma335;
  using System.Text;
  using System.Text.RegularExpressions;
  using Microsoft.VisualBasic;

  /// <summary>
  /// The logfmt logger. Outputs data to the underlying stream as a string using the `logfmt` format.
  /// </summary>
  public class Logger
  {
    private const string Date = "ts";
    private const string Message = "msg";
    private const string Level = "level";
    private const string Fieldformat = "{0}={1}";

    private const char Spacer = ' ';

    // will match spaces and other invalid characters that should not be in the key field
    private readonly Regex keyNameFilter = new Regex("([^a-z0-9_])+", RegexOptions.IgnoreCase & RegexOptions.Compiled);

    private readonly TextWriter output;
    private readonly Stream outputStream;
    private List<KeyValuePair<string, string>> includedData;

    /// <summary>
    /// Initializes a new instance of the <see cref="Logger"/> class.
    /// </summary>
    public Logger()
        : this(Console.OpenStandardOutput())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Logger"/> class.
    /// </summary>
    /// <param name="stream">The stream to output log lines to.</param>
    public Logger(Stream stream)
    {
      outputStream = stream;
      output = new StreamWriter(outputStream);
      includedData = new List<KeyValuePair<string, string>>();
    }

    /// <summary>
    /// Creates a new logger with the provided parameters.
    /// </summary>
    /// <param name="kvpairs">labels and values to include with log output.</param>
    /// <returns>A new <see cref="Logfmt.Logger"/> instance.</returns>
    public Logger WithData(params KeyValuePair<string, string>[] kvpairs)
    {
      var newLogger = new Logger(outputStream)
      {
        includedData = includedData,
      };
      newLogger.includedData.AddRange(kvpairs);

      return newLogger;
    }

    /// <summary>
    /// Creates a new logger with the provided parameters.
    /// </summary>
    /// <param name="kvpairs">labels and values to include with log output.</param>
    /// <returns>A new <see cref="Logfmt.Logger"/> instance.</returns>
    public Logger WithData(params string[] kvpairs)
    {
      CheckParamArrayLength(kvpairs);
      var pairs = new List<KeyValuePair<string, string>>();
      for (var i = 0; i < kvpairs.Length; i += 2)
      {
        pairs.Add(new KeyValuePair<string, string>(kvpairs[i], kvpairs[i + 1]));
      }

      return this.WithData(pairs.ToArray());
    }

    /// <summary>
    /// Writes a log entry to the underlying stream.
    /// </summary>
    /// <param name="severity">The severity of the log entry.</param>
    /// <param name="kvpairs">labels and values to include with the entry.</param>
    public void Log(SeverityLevel severity, params KeyValuePair<string, string>[] kvpairs)
    {
      var buffer = new StringBuilder();

      // Date in ISO8601 format
      buffer.AppendFormat(Fieldformat, Date, DateTime.UtcNow.ToString("o"));
      buffer.Append(Spacer);

      // severity level
      buffer.AppendFormat(Fieldformat, Level, severity.ToLower());

      // parameter pairs
      foreach (var pair in kvpairs.Where(kv => !string.IsNullOrWhiteSpace(kv.Key)))
      {
        buffer.Append(Spacer);

        // data pair
        buffer.AppendFormat(Fieldformat, PrepareKeyField(pair.Key), PrepareValueField(pair.Value));
      }

      // default data to be included
      foreach (var pair in includedData.Where(kv => !string.IsNullOrWhiteSpace(kv.Key)))
      {
        buffer.Append(Spacer);

        // data pair
        buffer.AppendFormat(Fieldformat, PrepareKeyField(pair.Key), PrepareValueField(pair.Value));
      }

      if (outputStream.CanWrite)
      {
        output.WriteLine(buffer.ToString());
        output.Flush();
      }
    }

    /// <summary>
    /// Writes a log entry to the underlying stream.
    /// </summary>
    /// <param name="severity">The severity of the log entry.</param>
    /// <param name="msg">the log message value.</param>
    /// <param name="kvpairs">labels and values to include with the entry.</param>
    public void Log(SeverityLevel severity, string msg, params string[] kvpairs)
    {
      CheckParamArrayLength(kvpairs);

      var pairs = new List<KeyValuePair<string, string>>
      {
        new KeyValuePair<string, string>(Message, msg),
      };
      for (var i = 0; i < kvpairs.Length; i += 2)
      {
        pairs.Add(new KeyValuePair<string, string>(kvpairs[i], kvpairs[i + 1]));
      }

      Log(severity, pairs.ToArray());
    }

    private static void CheckParamArrayLength<T>(T[] kvpairs)
    {
      if (kvpairs.Length % 2 != 0)
      {
        throw new ArgumentException("kvpairs must be an array with an even number of elements");
      }
    }

    private static string PrepareValueField(string value)
    {
      if (value.Contains(" "))
      {
        value = value.Replace("\"", "\\\"");
        value = "\"" + value + "\"";
      }

      return value;
    }

    private string PrepareKeyField(string key)
    {
      return keyNameFilter.Replace(key, "_");
    }
  }
}