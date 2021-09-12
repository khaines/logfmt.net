

namespace logfmt.ExtensionLogging
{
  using System;
  using Microsoft.Extensions.Logging;
  using Microsoft.Extensions.Logging.Abstractions;

  public class Logger : logfmt.Logger, ILogger
  {


    // ILogger methods

    public bool IsEnabled(LogLevel logLevel)
    {
      // TODO: add config to enable/disable logging levels.
      return true;
    }

    public IDisposable BeginScope<TState>(TState state) => NoOpScope.Instance;


    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
      if (!IsEnabled(logLevel))
      {
        return;
      }
      var entry = new LogEntry<TState>(logLevel, "logfmt.net", eventId, state, exception, formatter);
    }
  }
}