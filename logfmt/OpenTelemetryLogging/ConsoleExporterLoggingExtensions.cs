namespace Logfmt.OpenTelemetryLogging
{
    using System;
    using OpenTelemetry;
    using OpenTelemetry.Logs;
    public static class ConsoleExporterLoggingExtensions
    {
        public static OpenTelemetryLoggerOptions AddLogfmtConsoleExporter(this OpenTelemetryLoggerOptions loggerOptions)
        {
            ArgumentNullException.ThrowIfNull(loggerOptions);
#pragma warning disable CA2000
            return loggerOptions.AddProcessor(new SimpleLogRecordExportProcessor(new ConsoleLogExporter()));
#pragma warning restore CA2000
        }
    }
}