// Copyright (c) Ken Haines. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Logfmt;

using System.Globalization;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using Microsoft.VisualBasic;

/// <summary>
/// The logfmt logger. Outputs data to the underlying stream as a string using the `logfmt` format.
/// </summary>
public sealed class Logger : IDisposable
{
    /// <summary>
    /// The key used for the timestamp field.
    /// </summary>
    public const string DateKey = "ts";

    /// <summary>
    /// The key used for the message field.
    /// </summary>
    public const string MessageKey = "msg";

    /// <summary>
    /// The key used for the severity level field.
    /// </summary>
    public const string LevelKey = "level";
    private const string FieldFormat = "{0}={1}";

    private const char Spacer = ' ';

    private readonly TextWriter _output;
    private readonly Stream _outputStream;
    private List<KeyValuePair<string, string>> includedData;

    private SeverityLevel levelFilter;

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
    /// <param name="levelFilter">Optional severity level filter for log output.</param>
    public Logger(SeverityLevel levelFilter)
    : this(Console.OpenStandardOutput(), levelFilter)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Logger"/> class.
    /// </summary>
    /// <param name="stream">The stream to output log lines to.</param>
    /// <param name="levelFilter">Optional severity level filter for log output.</param>
    public Logger(Stream stream, SeverityLevel levelFilter = SeverityLevel.Info)
    {
        this.levelFilter = levelFilter;
        _outputStream = stream;
        _output = new StreamWriter(_outputStream);
        includedData = new List<KeyValuePair<string, string>>();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _output?.Dispose();
        _outputStream?.Dispose();
    }

    /// <summary>
    /// Creates a new logger with the provided parameters.
    /// </summary>
    /// <param name="kvpairs">labels and values to include with log output.</param>
    /// <returns>A new <see cref="Logfmt.Logger"/> instance.</returns>
    public Logger WithData(params KeyValuePair<string, string>[] kvpairs)
    {
        var newLogger = new Logger(_outputStream, this.levelFilter)
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
        ArgumentNullException.ThrowIfNull(kvpairs);
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
        if (!IsEnabled(severity))
        {
            return;
        }

        var buffer = new StringBuilder();

        // Date in ISO8601 format
        buffer.AppendFormat(CultureInfo.InvariantCulture, FieldFormat, DateKey, DateTime.UtcNow.ToString("o"));
        buffer.Append(Spacer);

        // severity level
        buffer.AppendFormat(CultureInfo.InvariantCulture, FieldFormat, LevelKey, severity.ToLower());

        // parameter pairs
        foreach (var pair in kvpairs.Where(kv => !string.IsNullOrWhiteSpace(kv.Key)))
        {
            buffer.Append(Spacer);

            // data pair
            AppendKeyField(buffer, pair.Key);
            buffer.Append('=');
            AppendValueField(buffer, pair.Key, pair.Value);
        }

        // default data to be included
        foreach (var pair in includedData.Where(kv => !string.IsNullOrWhiteSpace(kv.Key)))
        {
            buffer.Append(Spacer);

            // data pair
            AppendKeyField(buffer, pair.Key);
            buffer.Append('=');
            AppendValueField(buffer, pair.Key, pair.Value);
        }

        if (_outputStream.CanWrite)
        {
            _output.WriteLine(buffer.ToString());
            _output.Flush();
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
        if (!IsEnabled(severity))
        {
            return;
        }

        ArgumentNullException.ThrowIfNull(msg);
        ArgumentNullException.ThrowIfNull(kvpairs);

        CheckParamArrayLength(kvpairs);

        var pairs = new List<KeyValuePair<string, string>>
      {
        new KeyValuePair<string, string>(MessageKey, msg),
      };
        for (var i = 0; i < kvpairs.Length; i += 2)
        {
            pairs.Add(new KeyValuePair<string, string>(kvpairs[i], kvpairs[i + 1]));
        }

        Log(severity, pairs.ToArray());
    }

    /// <summary>
    /// Checks if a given severity level is enabled for log output.
    /// </summary>
    /// <param name="level">The level for which to check.</param>
    /// <returns>true if enabled.</returns>
    public bool IsEnabled(SeverityLevel level)
    {
        return level >= levelFilter;
    }

    /// <summary>
    /// Changes the severity level filter of this logger instance to the specified level.
    /// </summary>
    /// <param name="level">The level for which allow logging.</param>
    public void SetSeverityFilter(SeverityLevel level)
    {
        levelFilter = level;
    }

    private static void CheckParamArrayLength<T>(T[] kvpairs)
    {
        if (kvpairs.Length % 2 != 0)
        {
            throw new ArgumentException("kvpairs must be an array with an even number of elements");
        }
    }

    private static void AppendKeyField(StringBuilder buffer, string key)
    {
        bool lastWasUnderscore = false;
        foreach (char c in key)
        {
            if (IsValidKeyChar(c))
            {
                buffer.Append(c);
                lastWasUnderscore = false;
            }
            else
            {
                if (!lastWasUnderscore)
                {
                    buffer.Append('_');
                    lastWasUnderscore = true;
                }
            }
        }
    }

    private static bool IsValidKeyChar(char c)
    {
        return (c >= 'a' && c <= 'z') ||
               (c >= 'A' && c <= 'Z') ||
               (c >= '0' && c <= '9') ||
               c == '_';
    }

    private static void AppendValueField(StringBuilder buffer, string key, string value)
    {
        // Handle null values
        if (value == null)
        {
            buffer.Append("null");
            return;
        }

        bool needsQuotes = key == MessageKey;
        bool hasSpecialChars = false;

        for (int i = 0; i < value.Length; i++)
        {
            char c = value[i];
            if (c == ' ' || c == '\t')
            {
                needsQuotes = true;
            }
            else if (c == '"' || c == '\r' || c == '\n')
            {
                hasSpecialChars = true;
            }
        }

        if (needsQuotes)
        {
            buffer.Append('"');
        }

        if (hasSpecialChars)
        {
            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];
                switch (c)
                {
                    case '"':
                        buffer.Append("\\\"");
                        break;
                    case '\r':
                        buffer.Append("\\r");
                        break;
                    case '\n':
                        buffer.Append("\\n");
                        break;
                    default:
                        buffer.Append(c);
                        break;
                }
            }
        }
        else
        {
            buffer.Append(value);
        }

        if (needsQuotes)
        {
            buffer.Append('"');
        }
    }
}