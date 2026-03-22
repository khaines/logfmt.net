// Copyright (c) Ken Haines. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Logfmt.ExtensionLogging;

using Microsoft.Extensions.Logging;

/// <summary>
/// The configuration object used by the logger. Holds per-category log level settings.
/// Use the "Default" key to set the fallback log level when no category-specific level is configured.
/// </summary>
public class ExtensionLoggerConfiguration
{
    /// <summary>
    /// Gets the dictionary of log levels keyed by category name.
    /// Use "Default" as the key to set the fallback log level.
    /// </summary>
    public Dictionary<string, LogLevel> LogLevel { get; } = new Dictionary<string, LogLevel>();
}
