// Copyright (c) Ken Haines. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Logfmt.ExtensionLogging
{
  using Logfmt;
  using Microsoft.Extensions.Logging;

  /// <summary>
  /// Helper methods used with the Extension logging support.
  /// </summary>
  public static class ExtensionLoggerExtensions
  {
    /// <summary>
    /// Converts the <see cref="Microsoft.Extensions.Logging.LogLevel"/> value into a <see cref="Logfmt.SeverityLevel"/> value.
    /// </summary>
    /// <param name="level">Value to convert.</param>
    /// <returns>the resulting value.</returns>
    internal static SeverityLevel ToSeverityLevel(this LogLevel level)
    {
      switch (level)
      {
        case LogLevel.Trace:
          return SeverityLevel.Trace;

        case LogLevel.Debug:
          return SeverityLevel.Debug;

        case LogLevel.Information:
          return SeverityLevel.Info;

        case LogLevel.Warning:
          return SeverityLevel.Warn;

        case LogLevel.Error:
          return SeverityLevel.Error;

        case LogLevel.Critical:
          return SeverityLevel.Fatal;
        case LogLevel.None:
          return SeverityLevel.Off;
        default:
          return SeverityLevel.Off;
      }
    }
  }
}