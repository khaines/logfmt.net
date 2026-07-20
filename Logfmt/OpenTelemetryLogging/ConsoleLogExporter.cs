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
            // A disposed exporter reports Failure rather than writing a diagnostic to a possibly
            // hostile or redirected stderr (which could throw and break the never-throw contract).
            return ExportResult.Failure;
        }

        foreach (var record in batch)
        {
            try
            {
                _logger.Log(record.LogLevel.ToSeverityLevel(), ExtractAttributes(record));
            }
            catch (Exception ex)
            {
                // Never let a single malformed record fail the whole batch export: a member the
                // extraction reads (e.g. a hostile Exception.StackTrace) can still throw, so contain it
                // here, emit a best-effort diagnostic, and continue so the remaining records export.
                try
                {
                    _logger.Log(SeverityLevel.Error, new KeyValuePair<string, string>(Logger.MessageKey, $"[EXPORT ERROR: {Logger.SafeExceptionMessage(ex)}]"));
                }
                catch (Exception)
                {
                    // Swallow -- upholding the never-throw contract even if the diagnostic write fails.
                }
            }
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
        var attributes = new List<KeyValuePair<string, string>>(8);

        attributes.Add(new KeyValuePair<string, string>(Logger.MessageKey, record.FormattedMessage ?? record.Body ?? string.Empty));
        if (record.Exception is not null)
        {
            attributes.Add(new KeyValuePair<string, string>("exception_msg", Logger.SafeExceptionMessage(record.Exception)));
            attributes.Add(new KeyValuePair<string, string>("exception_stack", record.Exception.StackTrace ?? string.Empty));
        }

        if (record.CategoryName is not null)
        {
            attributes.Add(new KeyValuePair<string, string>("category", record.CategoryName));
        }

        if (record.EventId.Id != 0 || !string.IsNullOrWhiteSpace(record.EventId.Name))
        {
            attributes.Add(new KeyValuePair<string, string>("event_id", record.EventId.Id.ToString(CultureInfo.InvariantCulture)));
            if (record.EventId.Name != null)
            {
                attributes.Add(new KeyValuePair<string, string>("event_name", record.EventId.Name));
            }
        }

        if (record.TraceId != default)
        {
            attributes.Add(new KeyValuePair<string, string>("trace_id", record.TraceId.ToString()));
            attributes.Add(new KeyValuePair<string, string>("span_id", record.SpanId.ToString()));
            attributes.Add(new KeyValuePair<string, string>("trace_flags", record.TraceFlags.ToString()));
        }

        if (record.Attributes is not null)
        {
            foreach (var a in record.Attributes)
            {
                attributes.Add(new KeyValuePair<string, string>(a.Key, a.Value is null ? "null" : Logger.SafeToString(a.Value)));
            }
        }

        return attributes.ToArray();
    }
}