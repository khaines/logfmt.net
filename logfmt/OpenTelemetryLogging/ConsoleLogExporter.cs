// Copyright (c) Ken Haines. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
namespace Logfmt.OpenTelemetryLogging;

using System.Globalization;
using System.Runtime.CompilerServices;
using Logfmt.ExtensionLogging;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Logs;

/// <summary>
/// A <see cref="BaseExporter{T}"/> that outputs logs to the console in Logfmt format.
/// </summary>
public class ConsoleLogExporter : BaseExporter<LogRecord>
{
    private readonly Logger _logger;
    private bool _isDisposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConsoleLogExporter"/> class.
    /// </summary>
    public ConsoleLogExporter()
        : this(new Logger())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConsoleLogExporter"/> class.
    /// </summary>
    /// <param name="logger">The <see cref="Logfmt.Logger"/> instance to use for output.</param>
    public ConsoleLogExporter(Logger logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public override ExportResult Export(in Batch<LogRecord> batch)
    {
        if (_isDisposed)
        {
            Console.Error.WriteLine("Warning: Attempted to export logs using a disposed ConsoleLogExporter.");
            return ExportResult.Failure;
        }

        foreach (var record in batch)
        {
            _logger.Log(record.LogLevel.ToSeverityLevel(), ExtractAttributes(record));
        }

        return ExportResult.Success;
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            _logger.Dispose();
            _isDisposed = true;
        }

        base.Dispose(disposing);
    }

    private static KeyValuePair<string, string>[] ExtractAttributes(LogRecord record)
    {
        var attributes = new Dictionary<string, string>();

        attributes[Logger.MessageKey] = record.FormattedMessage ?? record.Body ?? string.Empty;
        if (record.Exception is not null)
        {
            attributes["exception_msg"] = record.Exception.Message;
            attributes["exception_stack"] = record.Exception.StackTrace ?? string.Empty;
        }

        if (record.CategoryName is not null)
        {
            attributes["category"] = record.CategoryName;
        }

        if (record.EventId.Id != 0 || !string.IsNullOrWhiteSpace(record.EventId.Name))
        {
            attributes["event_id"] = record.EventId.Id.ToString(CultureInfo.InvariantCulture);
            if (record.EventId.Name != null)
            {
                attributes["event_name"] = record.EventId.Name;
            }
        }

        if (record.TraceId != default)
        {
            attributes["trace_id"] = record.TraceId.ToString();
            attributes["span_id"] = record.SpanId.ToString();
            attributes["trace_flags"] = record.TraceFlags.ToString();
        }

        if (record.Attributes is not null)
        {
            foreach (var a in record.Attributes)
            {
                attributes[a.Key] = a.Value?.ToString() ?? "null";
            }
        }

        return attributes.ToArray();
    }
}