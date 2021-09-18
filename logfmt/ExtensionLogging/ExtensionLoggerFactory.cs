// Copyright (c) Ken Haines. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Logfmt.ExtensionLogging
{
  using Microsoft.Extensions.Logging;

  /// <summary>
  /// Extension logger factory for creating new instaces of <see cref="Microsoft.Extensions.Logging.ILogger" />
  /// from the <see cref="Logfmt.ExtensionLogging.ExtensionLoggerProvider" />.
  /// </summary>
  public sealed class ExtensionLoggerFactory : ILoggerFactory
  {
    private readonly ExtensionLoggerProvider loggerProvider;
    private readonly ILogger logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExtensionLoggerFactory"/> class.
    /// </summary>
    /// <param name="loggerProvider">Backing provider which creates new logger instances.</param>
    public ExtensionLoggerFactory(ExtensionLoggerProvider loggerProvider)
    {
      this.loggerProvider = loggerProvider;
      logger = this.loggerProvider.CreateLogger("ExtensionLoggerFactory");
    }

    /// <inheritdoc/>
    public void AddProvider(ILoggerProvider provider)
    {
      // currently noop
      // TODO: decide what to do with multiple providers
      logger.LogWarning("Ignoring added provider", "provider", provider);
    }

    /// <inheritdoc/>
    public ILogger CreateLogger(string category)
    {
      return loggerProvider.CreateLogger(category);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
      // currently noop
    }
  }
}