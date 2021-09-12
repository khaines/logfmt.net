namespace logfmt
{

  using System.Collections.Generic;
  using Microsoft.Extensions.Logging;
  public static class LoggerExtensions
  {

    public static void Info(this Logger logger, string msg, params KeyValuePair<string, string>[] kvpairs)
    {
      logger.Log(msg, SeverityLevel.Info, kvpairs);
    }

    public static void Debug(this Logger logger, string msg, params KeyValuePair<string, string>[] kvpairs)
    {
      logger.Log(msg, SeverityLevel.Debug, kvpairs);
    }


    public static void Warn(this Logger logger, string msg, params KeyValuePair<string, string>[] kvpairs)
    {
      logger.Log(msg, SeverityLevel.Warn, kvpairs);
    }


    public static void Error(this Logger logger, string msg, params KeyValuePair<string, string>[] kvpairs)
    {
      logger.Log(msg, SeverityLevel.Error, kvpairs);
    }
  }
}