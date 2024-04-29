namespace Logfmt.OpenTelemetry
{
    using System;
    using global::OpenTelemetry;
    using global::OpenTelemetry.Logs;
    public static class ConsoleExporterLoggingExtensions
    {
        public static OpenTelemetryLoggerOptions AddConsoleExporter(this OpenTelemetryLoggerOptions loggerOptions)
        {
            ArgumentNullException.ThrowIfNull(loggerOptions);
#pragma warning disable CA2000
            return loggerOptions.AddProcessor(new SimpleLogRecordExportProcessor(new ConsoleLogExporter()));
#pragma warning restore CA2000
        }
    }
}