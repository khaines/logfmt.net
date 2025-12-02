// Copyright (c) Ken Haines. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Logfmt.ExtensionLogging;

using Logfmt;
using Microsoft.Extensions.Logging;

/// <summary>
/// Helper methods used with the Extension logging support.
/// </summary>
public static class ExtensionLoggerExtensions
{
    /// <summary>
    /// Converts the <see cref="Microsoft.Extensions.Logging.LogLevel"/> value into a <see cref="Logfmt.SeverityLevel"/> value.
    /// </summary>
    /// <param name="level">Value to convert.</param>
    /// <returns>the resulting value.</returns>
    internal static SeverityLevel ToSeverityLevel(this LogLevel level)
    {
        return level switch
        {
            LogLevel.Trace => SeverityLevel.Trace,
            LogLevel.Debug => SeverityLevel.Debug,
            LogLevel.Information => SeverityLevel.Info,
            LogLevel.Warning => SeverityLevel.Warn,
            LogLevel.Error => SeverityLevel.Error,
            LogLevel.Critical => SeverityLevel.Fatal,
            LogLevel.None => SeverityLevel.Off,
            _ => SeverityLevel.Off,
        };
    }
}