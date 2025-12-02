// Copyright (c) Ken Haines. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Logfmt.ExtensionLogging;

using System.Globalization;
using Logfmt;
using Microsoft.Extensions.Logging;

/// <summary>
/// Implementation of Microsoft.Extensions.Logging.ILogger.
/// </summary>
public class ExtensionLogger : ILogger
{
    private readonly Func<ExtensionLoggerConfiguration> getCurrentConfig;
    private readonly string categoryName;
    private Logger logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExtensionLogger"/> class.
    /// </summary>
    /// <param name="logger">Instance of <see cref="Logger"/> to use by this extension logger.</param>
    /// <param name="getCurrentConfig">Function to get the logger configuration.</param>
    /// <param name="categoryName">Name of the category to log.</param>
    public ExtensionLogger(Logger logger, Func<ExtensionLoggerConfiguration> getCurrentConfig, string categoryName)
    {
        (this.logger, this.getCurrentConfig, this.categoryName) = (logger, getCurrentConfig, categoryName);
    }

    /// <inheritdoc/>
    public bool IsEnabled(LogLevel logLevel)
    {
        var config = getCurrentConfig();
        if (config.LogLevel.TryGetValue(this.categoryName, out var categoryLevel))
        {
            return categoryLevel <= logLevel;
        }

        if (config.LogLevel.TryGetValue("Default", out var defaultLevel))
        {
            return defaultLevel <= logLevel;
        }

        return false;
    }

    /// <summary>
    /// Adds the provided parameters to log.
    /// </summary>
    /// <param name="kvpairs">labels and values to include with log output.</param>
    public void WithData(params KeyValuePair<string, string>[] kvpairs)
    {
        this.logger = this.logger.WithData(kvpairs);
    }

    /// <inheritdoc/>
    public IDisposable? BeginScope<TState>(TState state)
        where TState : notnull
    {
        return NoOpScope.Instance;
    }

    /// <inheritdoc/>
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        var sevLevel = logLevel.ToSeverityLevel();
        if (!IsEnabled(logLevel))
        {
            return;
        }

        var props = new Dictionary<string, string>();
        if (state is IEnumerable<KeyValuePair<string, object>> stateProperties)
        {
            // add properties from the state object if it was a collection of pairs
            foreach (var prop in stateProperties)
            {
                if (prop.Value != null)
                {
                    props[prop.Key] = prop.Value.ToString() ?? string.Empty;
                }
            }
        }

        // create a message field if there is a formatter defined
        if (formatter != null)
        {
            props[Logger.MessageKey] = formatter(state, exception);
        }

        // event id
        if (eventId.Id != 0 || !string.IsNullOrWhiteSpace(eventId.Name))
        {
            props["event_id"] = eventId.Id.ToString(CultureInfo.InvariantCulture);
            if (!string.IsNullOrWhiteSpace(eventId.Name))
            {
                props["event_name"] = eventId.Name!;
            }
        }

        logger.Log(sevLevel, props.ToArray());
    }
}