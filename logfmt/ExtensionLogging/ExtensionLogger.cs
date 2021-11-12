// Copyright (c) Ken Haines. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Logfmt.ExtensionLogging
{
  using System;
  using System.Collections.Generic;
  using System.Globalization;
  using System.Linq;
  using Logfmt;
  using Microsoft.Extensions.Logging;

  /// <summary>
  /// Implementation of Microsoft.Extensions.Logging.ILogger.
  /// </summary>
  public class ExtensionLogger : ILogger
  {
    private readonly Logger logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExtensionLogger"/> class.
    /// </summary>
    /// <param name="logger">Instance of <see cref="Logger"/> to use by this extension logger.</param>
    public ExtensionLogger(Logger logger)
    {
      this.logger = logger;
    }

    /// <inheritdoc/>
    public bool IsEnabled(LogLevel logLevel)
    {
      // TODO: add config to enable/disable logging levels.
      return true;
    }

    /// <inheritdoc/>
    public IDisposable BeginScope<TState>(TState state) => NoOpScope.Instance;

    /// <inheritdoc/>
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
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
          props[prop.Key] = prop.Value.ToString();
        }
      }

      // create a message field if there is a formatter defined
      if (formatter != null)
      {
        props["msg"] = formatter(state, exception);
      }

      // event id
      if (eventId.Id != 0 || !string.IsNullOrWhiteSpace(eventId.Name))
      {
        props["event_id"] = eventId.Id.ToString(CultureInfo.InvariantCulture);
        props["event_name"] = eventId.Id.ToString(CultureInfo.InvariantCulture);
      }

      logger.Log(sevLevel, props.ToArray());
    }
  }
}