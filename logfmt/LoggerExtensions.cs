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
namespace Logfmt
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

    public static string ToLower(this SeverityLevel level)
    {
      return level.ToString().ToLower();
    }

  }
}