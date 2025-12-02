// Copyright (c) Ken Haines. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
namespace Logfmt.OpenTelemetryLogging;

using OpenTelemetry;
using OpenTelemetry.Logs;

/// <summary>
/// Extension methods for <see cref="OpenTelemetryLoggerOptions"/> to add Logfmt console exporter.
/// </summary>
public static class ConsoleExporterLoggingExtensions
{
    /// <summary>
    /// Adds Logfmt console exporter to the <see cref="OpenTelemetryLoggerOptions"/>.
    /// </summary>
    /// <param name="loggerOptions">The <see cref="OpenTelemetryLoggerOptions"/> to add the exporter to.</param>
    /// <returns>The <see cref="OpenTelemetryLoggerOptions"/> so that additional calls can be chained.</returns>
    public static OpenTelemetryLoggerOptions AddLogfmtConsoleExporter(this OpenTelemetryLoggerOptions loggerOptions)
    {
        ArgumentNullException.ThrowIfNull(loggerOptions);
#pragma warning disable CA2000
        return loggerOptions.AddProcessor(new SimpleLogRecordExportProcessor(new ConsoleLogExporter()));
#pragma warning restore CA2000
    }
}