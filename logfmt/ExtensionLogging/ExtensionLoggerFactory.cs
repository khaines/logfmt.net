// Copyright (c) Ken Haines. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Logfmt.ExtensionLogging
{
  using Microsoft.Extensions.Logging;

  /// <summary>
  /// Extension logger factory for creating new instances of <see cref="Microsoft.Extensions.Logging.ILogger" />
  /// from the <see cref="Logfmt.ExtensionLogging.ExtensionLoggerProvider" />.
  /// </summary>
  public sealed class ExtensionLoggerFactory : ILoggerFactory
  {
    private readonly ExtensionLoggerProvider _loggerProvider;
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExtensionLoggerFactory"/> class.
    /// </summary>
    /// <param name="loggerProvider">Backing provider which creates new logger instances.</param>
    public ExtensionLoggerFactory(ExtensionLoggerProvider loggerProvider)
    {
      this._loggerProvider = loggerProvider;
      _logger = this._loggerProvider.CreateLogger("ExtensionLoggerFactory");
    }

    /// <inheritdoc/>
    public void AddProvider(ILoggerProvider provider)
    {
      // currently noop
      // TODO: decide what to do with multiple providers
      var msg = LoggerMessage.Define<ILoggerProvider>(LogLevel.Warning, new EventId(0, "AddProvider"), "Ignoring added provider: {Provider}");
      msg(_logger, provider, null);
    }

    /// <inheritdoc/>
    public ILogger CreateLogger(string categoryName)
    {
      return _loggerProvider.CreateLogger(categoryName);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
      // currently noop
    }
  }
}