/*
MIT License

Copyright (c) 2021 Ken Haines

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

namespace Logfmt.ExtensionLogging
{
  using System;
  using System.Collections.Generic;
  using System.Globalization;
  using System.Linq;
  using Logfmt;
  using Microsoft.Extensions.Logging;


  public class ExtensionLogger : ILogger
  {
    private Logger _logger;

    public ExtensionLogger(Logger logger)
    {
      _logger = logger;
    }


    // ILogger methods

    public bool IsEnabled(LogLevel logLevel)
    {
      // TODO: add config to enable/disable logging levels.
      return true;
    }

    public IDisposable BeginScope<TState>(TState state) => NoOpScope.Instance;


    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
      var sevLevel = logLevel.ToSeverityLevel();
      if (!IsEnabled(logLevel))
      {
        return;
      }
      var properties = new List<KeyValuePair<string, string>>();
      if (state is IEnumerable<KeyValuePair<string, object>> stateProperties)
      {
        // add properties from the state object if it was a collection of pairs
        properties.AddRange(stateProperties.Select(kv => new KeyValuePair<string, string>(kv.Key, kv.Value.ToString())));
      }

      // create a message field if there is a formatter defined
      if (formatter != null)
      {
        properties.Add(new KeyValuePair<string, string>("msg", formatter(state, exception)));
      }

      // event id 
      if (eventId.Id != 0 || !string.IsNullOrWhiteSpace(eventId.Name))
      {
        properties.Add(new KeyValuePair<string, string>("event_id", eventId.Id.ToString(CultureInfo.InvariantCulture)));
        properties.Add(new KeyValuePair<string, string>("event_name", eventId.Id.ToString(CultureInfo.InvariantCulture)));
      }
      _logger.Log(sevLevel, properties.ToArray());
    }
  }
}