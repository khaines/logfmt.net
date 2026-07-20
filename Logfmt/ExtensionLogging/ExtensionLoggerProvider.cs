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
        if (config.CurrentValue == null)
        {
            throw new InvalidOperationException("ExtensionLoggerConfiguration is missing or invalid. Please ensure logging configuration is provided.");
        }

        _currentConfig = config.CurrentValue;
        _onChangeToken = config.OnChange(updatedConfig => _currentConfig = updatedConfig);
    }

    /// <inheritdoc/>
    [SuppressMessage(
      "Microsoft.Reliability",
      "CA2000:DisposeObjectsBeforeLosingScope",
      Justification = "The created logger instance has a longer lifetime than the method it is created in.")]
    public ILogger CreateLogger(string categoryName)
    {
        // The core Logger is intentionally unfiltered (Trace): ExtensionLogger.IsEnabled reads the
        // live configuration on every call and is the single severity gate. Baking the creation-time
        // level into the core Logger would double-gate and defeat runtime level-lowering (#70).
        return _loggers.GetOrAdd(
            categoryName,
            name => new ExtensionLogger(new Logger(SeverityLevel.Trace).WithData(Category, name), GetCurrentConfig, name));
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        // The cached ExtensionLoggers wrap core Loggers that write to Console.OpenStandardOutput().
        // They are intentionally NOT disposed here: each Log() flushes immediately (so no buffered
        // data is lost) and disposing would close the shared stdout handle. We only drop the cache
        // and unsubscribe the options-change token.
        _loggers.Clear();
        _onChangeToken?.Dispose();
    }

    private ExtensionLoggerConfiguration GetCurrentConfig() => _currentConfig;
}