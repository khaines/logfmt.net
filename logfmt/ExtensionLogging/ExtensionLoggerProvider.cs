// Copyright (c) Ken Haines. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Logfmt.ExtensionLogging;

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

/// <summary>
/// Extension provider implementing the <see cref="Microsoft.Extensions.Logging.ILoggerProvider" /> interface.
/// </summary>
public sealed class ExtensionLoggerProvider : ILoggerProvider
{
    private const string Category = "category";
    private readonly IDisposable? _onChangeToken;
    private readonly ConcurrentDictionary<string, ExtensionLogger> _loggers = new (StringComparer.OrdinalIgnoreCase);
    private ExtensionLoggerConfiguration _currentConfig;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExtensionLoggerProvider"/> class.
    /// </summary>
    /// <param name="config">The <see cref="Logfmt.ExtensionLogging.ExtensionLoggerConfiguration" /> logging configuration.</param>
    public ExtensionLoggerProvider(IOptionsMonitor<ExtensionLoggerConfiguration> config)
    {
        ArgumentNullException.ThrowIfNull(config, nameof(config));
        _currentConfig = config.CurrentValue ?? new ExtensionLoggerConfiguration();
        _onChangeToken = config.OnChange(updatedConfig => _currentConfig = updatedConfig);
    }

    /// <inheritdoc/>
    [SuppressMessage(
      "Microsoft.Reliability",
      "CA2000:DisposeObjectsBeforeLosingScope",
      Justification = "The created logger instance has a longer lifetime than the method it is created in.")]
    public ILogger CreateLogger(string categoryName)
    {
        if (!_currentConfig.LogLevel.TryGetValue(categoryName, out LogLevel logLevel) && !_currentConfig.LogLevel.TryGetValue("Default", out logLevel))
        {
            logLevel = LogLevel.None;
        }

        if (!_loggers.TryGetValue(categoryName, out ExtensionLogger? extLogger))
        {
            extLogger = new ExtensionLogger(new Logger(logLevel.ToSeverityLevel()).WithData(Category, categoryName), GetCurrentConfig, categoryName);
            _ = _loggers.TryAdd(categoryName, extLogger);
        }

        return extLogger;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        // noop
        _loggers.Clear();
        _onChangeToken?.Dispose();
    }

    private ExtensionLoggerConfiguration GetCurrentConfig() => _currentConfig;
}