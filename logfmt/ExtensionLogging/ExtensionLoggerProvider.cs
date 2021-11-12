// Copyright (c) Ken Haines. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Logfmt.ExtensionLogging
{
  using Microsoft.Extensions.Logging;

  /// <summary>
  /// Extension provider implementing the <see cref="Microsoft.Extensions.Logging.ILoggerProvider" /> interface.
  /// </summary>
  public sealed class ExtensionLoggerProvider : ILoggerProvider
  {
    private const string Category = "category";
    private readonly Logger logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExtensionLoggerProvider"/> class.
    /// </summary>
    /// <param name="baseLogger">The <see cref="Logfmt.Logger" /> instance to use for generating new instances from.</param>
    public ExtensionLoggerProvider(Logger baseLogger)
    {
      logger = baseLogger;
    }

    /// <inheritdoc/>
    public ILogger CreateLogger(string categoryName)
    {
      return new ExtensionLogger(logger.WithData(Category, categoryName));
    }

    /// <inheritdoc/>
    public void Dispose()
    {
      // noop
    }
  }
}