// Copyright (c) Ken Haines. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Logfmt.ExtensionLogging;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;

/// <summary>
/// Extension methods for adding logfmt logging to an <see cref="ILoggingBuilder"/>.
/// </summary>
public static class LoggingBuilderExtensions
{
    /// <summary>
    /// Adds the logfmt logging provider to the <see cref="ILoggingBuilder"/>.
    /// </summary>
    /// <param name="builder">The <see cref="ILoggingBuilder"/> to add the provider to.</param>
    /// <returns>The <see cref="ILoggingBuilder"/> so that additional calls can be chained.</returns>
    public static ILoggingBuilder AddLogfmt(this ILoggingBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.AddConfiguration();
        builder.Services.AddSingleton<ILoggerProvider, ExtensionLoggerProvider>();
        LoggerProviderOptions.RegisterProviderOptions
            <ExtensionLoggerConfiguration, ExtensionLoggerProvider>(builder.Services);

        return builder;
    }

    /// <summary>
    /// Adds the logfmt logging provider to the <see cref="ILoggingBuilder"/> with the specified configuration.
    /// </summary>
    /// <param name="builder">The <see cref="ILoggingBuilder"/> to add the provider to.</param>
    /// <param name="configure">An action to configure the <see cref="ExtensionLoggerConfiguration"/>.</param>
    /// <returns>The <see cref="ILoggingBuilder"/> so that additional calls can be chained.</returns>
    public static ILoggingBuilder AddLogfmt(this ILoggingBuilder builder, Action<ExtensionLoggerConfiguration> configure)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(configure);

        builder.AddLogfmt();
        builder.Services.Configure(configure);

        return builder;
    }
}
