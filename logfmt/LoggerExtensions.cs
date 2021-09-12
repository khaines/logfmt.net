namespace logfmt
{

  using System.Collections.Generic;
  using Microsoft.Extensions.Logging;
  public static class LoggerExtensions
  {
    public static void Info(this Logger logger, string msg, params string[] kvpairs)
    {
      logger.Log(SeverityLevel.Info, msg, kvpairs);
    }

    public static void Debug(this Logger logger, string msg, params string[] kvpairs)
    {
      logger.Log(SeverityLevel.Debug, msg, kvpairs);
    }

    public static void Warn(this Logger logger, string msg, params string[] kvpairs)
    {
      logger.Log(SeverityLevel.Warn, msg, kvpairs);
    }

    public static void Error(this Logger logger, string msg, params string[] kvpairs)
    {
      logger.Log(SeverityLevel.Error, msg, kvpairs);
    }
  }
}