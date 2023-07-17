// Copyright (c) Ken Haines. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
namespace Logfmt
{
  using System;
  using System.Diagnostics.CodeAnalysis;
  using System.Globalization;

  /// <summary>
  /// Helper methods for use with the <see cref="Logfmt.Logger" /> class.
  /// </summary>
  public static class LoggerExtensions
  {
    /// <summary>
    /// Creates a log event with a severity of Information.
    /// </summary>
    /// <param name="logger">the current logger instance.</param>
    /// <param name="msg">The log message.</param>
    /// <param name="kvpairs">The labels and values to include.</param>
    public static void Info(this Logger logger, string msg, params string[] kvpairs)
    {
      ArgumentNullException.ThrowIfNull(logger, nameof(logger));
      logger.Log(SeverityLevel.Info, msg, kvpairs);
    }

    /// <summary>
    /// Creates a log event with a severity of Debug.
    /// </summary>
    /// <param name="logger">the current logger instance.</param>
    /// <param name="msg">The log message.</param>
    /// <param name="kvpairs">The labels and values to include.</param>
    public static void Debug(this Logger logger, string msg, params string[] kvpairs)
    {
      ArgumentNullException.ThrowIfNull(logger, nameof(logger));
      logger.Log(SeverityLevel.Debug, msg, kvpairs);
    }

    /// <summary>
    /// Creates a log event with a severity of Warning.
    /// </summary>
    /// <param name="logger">the current logger instance.</param>
    /// <param name="msg">The log message.</param>
    /// <param name="kvpairs">The labels and values to include.</param>
    public static void Warn(this Logger logger, string msg, params string[] kvpairs)
    {
      ArgumentNullException.ThrowIfNull(logger, nameof(logger));
      logger.Log(SeverityLevel.Warn, msg, kvpairs);
    }

    /// <summary>
    /// Creates a log event with a severity of Error.
    /// </summary>
    /// <param name="logger">the current logger instance.</param>
    /// <param name="msg">The log message.</param>
    /// <param name="kvpairs">The labels and values to include.</param>
    public static void Error(this Logger logger, string msg, params string[] kvpairs)
    {
      ArgumentNullException.ThrowIfNull(logger, nameof(logger));
      logger.Log(SeverityLevel.Error, msg, kvpairs);
    }

    /// <summary>
    /// Convert the SeverityLevel enum to a lowercase string.
    /// </summary>
    /// <param name="level">the severity level.</param>
    /// <returns>a lower case string of the SeverityLevel.</returns>
    [SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase", Justification = "Logfmt requires lowercase strings.")]
    internal static string ToLower(this SeverityLevel level)
    {
      return level.ToString().ToLower(CultureInfo.InvariantCulture);
    }
  }
}