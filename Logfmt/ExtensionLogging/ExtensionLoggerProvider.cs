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
    private readonly Logger _rootLogger;
    private ExtensionLoggerConfiguration _currentConfig;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExtensionLoggerProvider"/> class that writes to stdout.
    /// </summary>
    /// <param name="config">The <see cref="Logfmt.ExtensionLogging.ExtensionLoggerConfiguration" /> logging configuration.</param>
    [SuppressMessage(
      "Microsoft.Reliability",
      "CA2000:DisposeObjectsBeforeLosingScope",
      Justification = "The root logger lives as long as the provider and is intentionally not disposed (see Dispose).")]
    public ExtensionLoggerProvider(IOptionsMonitor<ExtensionLoggerConfiguration> config)
        : this(config, new Logger(SeverityLevel.Trace))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExtensionLoggerProvider"/> class that writes through the
    /// given core <see cref="Logger"/>. Every category logger is derived from it, so all categories share its
    /// output stream and write lock. The provider does not dispose it.
    /// </summary>
    /// <param name="config">The <see cref="Logfmt.ExtensionLogging.ExtensionLoggerConfiguration" /> logging configuration.</param>
    /// <param name="logger">
    /// The core logger whose stream and lock every category logger shares. The provider takes over its
    /// severity filter, immediately resetting it to <see cref="SeverityLevel.Trace"/> and keeping it there
    /// for the life of the provider (the <c>ILogger</c> configuration is the only severity gate; see the
    /// remarks in the constructor body). Pass an instance dedicated to this provider -- calling
    /// <c>SetSeverityFilter</c> on it afterwards, or logging through it directly outside the
    /// provider, will not behave as an unfiltered <see cref="Logger"/> instance normally would.
    /// </param>
    public ExtensionLoggerProvider(IOptionsMonitor<ExtensionLoggerConfiguration> config, Logger logger)
    {
        ArgumentNullException.ThrowIfNull(config, nameof(config));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        if (config.CurrentValue == null)
        {
            throw new InvalidOperationException("ExtensionLoggerConfiguration is missing or invalid. Please ensure logging configuration is provided.");
        }

        // The core Logger is intentionally unfiltered (Trace): ExtensionLogger.IsEnabled reads the
        // live configuration on every call and is the single severity gate. Baking a level into the
        // core Logger would double-gate and defeat runtime level-lowering (#70).
        _rootLogger = logger;
        _rootLogger.SetSeverityFilter(SeverityLevel.Trace);
        _currentConfig = config.CurrentValue;
        _onChangeToken = config.OnChange(updatedConfig => _currentConfig = updatedConfig);
    }

    /// <inheritdoc/>
    public ILogger CreateLogger(string categoryName)
    {
        // Every category logger is derived from ONE root Logger via WithData, so all categories share
        // the same output stream and the same write lock. Creating a separate Logger per category
        // (a) opened one stdout handle per category, which was never released, and (b) gave each
        // category its own lock, so lines longer than the writer buffer from different categories
        // could interleave on stdout and corrupt each other.
        return _loggers.GetOrAdd(
            categoryName,
            name => new ExtensionLogger(_rootLogger.WithData(Category, name), GetCurrentConfig, name));
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        // The root Logger (and the category loggers derived from it) is intentionally NOT disposed
        // here: each Log() flushes immediately (so no buffered data is lost), ILogger instances handed
        // out earlier may still be in use, and disposing would close the stdout handle. We only drop
        // the cache and unsubscribe the options-change token.
        _loggers.Clear();
        _onChangeToken?.Dispose();
    }

    private ExtensionLoggerConfiguration GetCurrentConfig() => _currentConfig;
}