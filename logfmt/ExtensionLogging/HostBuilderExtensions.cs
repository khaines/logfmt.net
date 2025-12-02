// Copyright (c) Ken Haines. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Logfmt.ExtensionLogging;

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;

/// <summary>
/// Extension class for adding helper methods to instances of IHostBuilder.
/// </summary>
public static class HostBuilderExtensions
{
    /// <summary>
    /// Adds the Logfmt.net's extension logger provider to the IHostBuilder's service collection.
    /// </summary>
    /// <param name="builder">The <see cref="IHostBuilder"/> instance.</param>
    /// <param name="configuration">The <see cref="Logfmt.ExtensionLogging.ExtensionLoggerConfiguration"/> used to determine logging levels.</param>
    /// <returns>The modified <see cref="IHostBuilder"/> instance.</returns>
    public static IHostBuilder UseLogfmtLogging(
        this IHostBuilder builder,
        Action<ExtensionLoggerConfiguration> configuration)
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        if (configuration == null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }

        builder.ConfigureLogging(logBuilder =>
        {
            logBuilder.ClearProviders().AddConfiguration();

            logBuilder.Services.AddSingleton<ILoggerProvider, ExtensionLoggerProvider>();

            LoggerProviderOptions.RegisterProviderOptions
                <ExtensionLoggerConfiguration, ExtensionLoggerProvider>(logBuilder.Services);

            logBuilder.Services.Configure(configuration);
        });

        return builder;
    }
}