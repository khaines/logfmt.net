// Copyright (c) Ken Haines. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Logfmt.ExtensionLogging
{
    using System;
    using System.Collections.Concurrent;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    /// <summary>
    /// Extension provider implementing the <see cref="Microsoft.Extensions.Logging.ILoggerProvider" /> interface.
    /// </summary>
    public sealed class ExtensionLoggerProvider : ILoggerProvider
    {
        private const string Category = "category";
        private readonly IDisposable onChangeToken;
        private readonly ConcurrentDictionary<string, ExtensionLogger> loggers = new (StringComparer.OrdinalIgnoreCase);
        private ExtensionLoggerConfiguration currentConfig;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtensionLoggerProvider"/> class.
        /// </summary>
        /// <param name="config">The <see cref="Logfmt.ExtensionLogging.ExtensionLoggerConfiguration" /> logging configuration.</param>
        public ExtensionLoggerProvider(IOptionsMonitor<ExtensionLoggerConfiguration> config)
        {
            currentConfig = config.CurrentValue;
            onChangeToken = config.OnChange(updatedConfig => currentConfig = updatedConfig);
        }

        /// <inheritdoc/>
        public ILogger CreateLogger(string categoryName)
        {
            Logger newLogger = null;
            if (currentConfig.LogLevel.ContainsKey(categoryName))
            {
                newLogger = new Logger(currentConfig.LogLevel[categoryName].ToSeverityLevel()).WithData(Category, categoryName);
            }
            else if (currentConfig.LogLevel.ContainsKey("Default"))
            {
                newLogger = new Logger(currentConfig.LogLevel["Default"].ToSeverityLevel()).WithData(Category, categoryName);
            }
            else
            {
                newLogger = new Logger(SeverityLevel.Off);
            }

            return loggers.GetOrAdd(categoryName, new ExtensionLogger(newLogger, GetCurrentConfig, categoryName));
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            // noop
            loggers.Clear();
            onChangeToken.Dispose();
        }

        private ExtensionLoggerConfiguration GetCurrentConfig() => currentConfig;
    }
}