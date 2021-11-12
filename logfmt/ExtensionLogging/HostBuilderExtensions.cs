// Copyright (c) Ken Haines. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Logfmt.ExtensionLogging
{
  using System;
  using Microsoft.Extensions.DependencyInjection;
  using Microsoft.Extensions.Hosting;
  using Microsoft.Extensions.Logging;

  /// <summary>
  /// Extension class for adding helper methods to instances of IHostBuilder.
  /// </summary>
  public static class HostBuilderExtensions
  {
    /// <summary>
    /// Adds the Logfmt.net's extension logger provider to the IHostBuilder's service collection.
    /// </summary>
    /// <param name="builder">The <see cref="IHostBuilder"/> instance.</param>
    /// <param name="logger">An optional <see cref="Logger"/> instance to use. A default instance will be created if not provided.</param>
    /// <returns>The modified <see cref="IHostBuilder"/> instance.</returns>
    public static IHostBuilder UseLogfmtLogging(
        this IHostBuilder builder,
        Logger logger = null)
    {
      if (builder == null)
      {
        throw new ArgumentNullException(nameof(builder));
      }

      builder.ConfigureServices(collection =>
      {
        var provider = new ExtensionLoggerProvider(logger ?? new Logger());
        collection.AddSingleton<ILoggerFactory>(services => new ExtensionLoggerFactory(provider));
      });

      return builder;
    }
  }
}